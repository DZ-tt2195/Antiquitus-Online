using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;

public class Player : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public Transform cardhand;
    public Transform placardhand;
    public List<Card> listOfHand = new List<Card>();
    public List<Placard> listOfPlacard = new List<Placard>();

    public int playerposition;
    public Photon.Realtime.Player photonplayer;
    Button resign;

    [HideInInspector] public PlayerButton myButton;
    [HideInInspector] Transform arrow;
    Transform storePlayers;
    int movePosition;

    [HideInInspector] public bool waiting;
    [HideInInspector] public bool turnon;

    [HideInInspector] public string choice;
    [HideInInspector] public Placard chosenPlacard;
    [HideInInspector] public Card chosencard;
    [HideInInspector] public TileData chosentile;
    [HideInInspector] public WeaponBox chosenbox;
    [HideInInspector] public BoneArrow chosenarrow;

    public Card cardthisturn;
    public TileData tilethisturn;
    public int cardsdiscarded;
    public int cardsrevealed;
    public bool eventactivated;

    [HideInInspector] public int turns;
    public int reputation;
    [HideInInspector] public int personalsubmissions;

    private void Awake()
    {
        resign = GameObject.Find("Resign Button").GetComponent<Button>();
        storePlayers = GameObject.Find("Store Players").transform;
        arrow = GameObject.Find("Arrow").transform;
    }

    private void Start()
    {
        if (this.pv.AmController)
            resign.onClick.AddListener(ResignTime);
        this.name = pv.Owner.NickName;
        this.transform.SetParent(storePlayers);
    }

    public void ResignTime()
    {
        Manager.instance.GameOver($"{this.name} has resigned.", this.playerposition);
    }

    [PunRPC]
    public void AssignInfo(int position, Photon.Realtime.Player playername)
    {
        this.transform.localPosition = new Vector3(-280 + 2000 * playerposition, 0, 0);
        this.playerposition = position;
        this.photonplayer = playername;
        this.myButton = GameObject.Find($"{this.name}'s Button").GetComponent<PlayerButton>();
        photonView.RPC("UpdateButtonText", RpcTarget.All);
        photonView.RPC("ButtonClick", RpcTarget.All);

        if (pv.AmOwner)
            ClickMe();
    }

    [PunRPC]
    public void UpdateButtonText()
    {
        myButton.textbox.text = $"{this.name}: {reputation} REP";
    }

    [PunRPC]
    public void ButtonClick()
    {
        movePosition = -2000 * playerposition;
        myButton.button.onClick.AddListener(ClickMe);
    }

    public void ClickMe()
    {
        storePlayers.transform.localPosition = new Vector3(movePosition, 0, 0);
        arrow.transform.SetParent(myButton.transform);
        arrow.transform.localPosition = new Vector3(95.5f, 0, 0);
    }

    public Player GetPreviousPlayer()
    {
        if (this.playerposition == 0)
            return Manager.instance.playerordergameobject[^1];
        else
            return Manager.instance.playerordergameobject[this.playerposition - 1];
    }

    [PunRPC]
    IEnumerator WaitForPlayer(string playername)
    {
        waiting = true;
        Manager.instance.instructions.text = $"Waiting for {playername}...";
        while (waiting)
        {
            yield return null;
        }
    }

    [PunRPC]
    void WaitDone()
    {
        waiting = false;
    }

    public IEnumerator TakeTurnRPC(Photon.Realtime.Player requestingplayer)
    {
        photonView.RPC("TakeTurn", requestingplayer);
        turnon = true;
        while (turnon)
            yield return null;
        Debug.Log("turn finished");
    }

    [PunRPC]
    void TurnOver()
    {
        turnon = false;
    }

    [PunRPC]
    public IEnumerator TakeTurn()
    {
        yield return null;
        if (pv.IsMine)
        {
            turns++;
            ClickMe();
            photonView.RPC("WaitForPlayer", RpcTarget.Others, this.name);

            Log.instance.pv.RPC("AddText", RpcTarget.All, $"");
            Log.instance.pv.RPC("AddText", RpcTarget.All, $"{this.name}'s Turn");

            yield return ChooseTileInSite();
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < tilethisturn.adjacentTiles.Count; i++)
                yield return ResolveAdjacentTile(tilethisturn.adjacentTiles[i]);

            yield return cardthisturn.OnTakeEffect(this);

            yield return new WaitForSeconds(0.5f);

            yield return AskForSubmission();

            cardthisturn = null;
            tilethisturn = null;
            cardsdiscarded = 0;
            cardsrevealed = 0;
            eventactivated = false;
            choice = "";
            chosencard = null;
            chosentile = null;
            chosenbox = null;
            chosenarrow = null;
            chosenPlacard = null;
            photonView.RPC("TurnOver", RpcTarget.All);
        }
    }

    IEnumerator ChooseTileInSite()
    {
        for (int i = 0; i < Manager.instance.listoftiles.Count; i++)
        {
            TileData nexttile = Manager.instance.listoftiles[i];
            if (nexttile.faceup && !nexttile.mycard.eventtile)
                nexttile.choicescript.EnableButton(this, true);
        }

        choice = "";
        chosencard = null;
        chosentile = null;
        chosenbox = null;
        chosenarrow = null;

        Manager.instance.instructions.text = $"Take a non-event tile from the Site";
        while (choice == "")
            yield return null;

        for (int i = 0; i < Manager.instance.listoftiles.Count; i++)
            Manager.instance.listoftiles[i].choicescript.DisableButton();

        cardthisturn = chosentile.mycard;
        tilethisturn = chosentile;

        int[] cardIDs = new int[1];
        cardIDs[0] = cardthisturn.pv.ViewID;
        photonView.RPC("SendDraw", RpcTarget.All, cardIDs);
        chosentile.pv.RPC("ReplaceCard", RpcTarget.MasterClient);
        Log.instance.pv.RPC("AddText", RpcTarget.All, $"");
    }

    IEnumerator ResolveAdjacentTile(TileData nextTile)
    {
        if (nextTile != null)
        {
            if (nextTile.faceup)
            {
                cardsdiscarded++;
                Card discardedCard = nextTile.mycard;
                Log.instance.pv.RPC("AddText", RpcTarget.All, $"{this.name} discards {nextTile.mycard.logName}.");

                nextTile.pv.RPC("ReplaceCard", RpcTarget.MasterClient);
                photonView.RPC("DiscardCard", RpcTarget.All, discardedCard.pv.ViewID);
                yield return discardedCard.OnDiscardEffect(this);
            }
            else
            {
                cardsrevealed++;
                nextTile.pv.RPC("FlipTile", RpcTarget.All, true);
                Log.instance.pv.RPC("AddText", RpcTarget.All, $"{this.name} reveals {nextTile.mycard.logName}.");
            }
        }
    }

    IEnumerator AskForSubmission()
    {
        yield return null;
        if (listOfHand.Count >= 2 && listOfPlacard.Count >= 1)
        {
            List<Card> submittedCards = new List<Card>();
            List<Placard> submittedPlacards = new List<Placard>();

            Manager.instance.instructions.text = $"Make a submission?";
            Manager.instance.AddTextButton(this, "Don't Submit", true);
            Manager.instance.AddTextButton(this, "Confirm", true);

            for (int i = 0; i < listOfHand.Count; i++)
                listOfHand[i].choicescript.EnableButton(this, true);
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.EnableButton(this, true);

            while (true)
            {
                if (submittedPlacards.Count >= 3)
                    for (int i = 0; i < listOfPlacard.Count; i++)
                        listOfPlacard[i].choicescript.DisableButton();

                this.choice = "";
                this.chosencard = null;
                this.chosenPlacard = null;
                while (this.choice == "")
                    yield return null;

                if (choice == "Don't Submit")
                {
                    submittedCards.Clear();
                    submittedPlacards.Clear();
                    break;
                }
                else if (choice == "Confirm")
                {
                    break;
                }
                else if (chosencard != null)
                {
                    chosencard.choicescript.DisableButton();
                    Manager.instance.AddCardButton(this, chosencard, false);
                    submittedCards.Add(chosencard);
                }
                else if (chosenPlacard != null)
                {
                    chosenPlacard.choicescript.DisableButton();
                    Manager.instance.AddPlacardButton(this, chosenPlacard, false);
                    submittedPlacards.Add(chosenPlacard);
                }
            }

            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.DisableButton();
            for (int i = 0; i < listOfHand.Count; i++)
                listOfHand[i].choicescript.DisableButton();
            Manager.instance.ClearButtons();

            if (submittedCards.Count >= 2 && submittedPlacards.Count >= 1)
            {
                Placard StrengthInNumbers = null;
                List<Placard> successfulPlacards = new List<Placard>();

                for (int i = 0; i<submittedPlacards.Count; i++)
                {
                    if (submittedPlacards[i].name == "Strength in Numbers")
                        StrengthInNumbers = submittedPlacards[i];
                    else if (submittedPlacards[i].CanSubmit(this, submittedCards))
                    {
                        Debug.Log($"{submittedPlacards[i].name} was successful");
                        successfulPlacards.Add(submittedPlacards[i]);
                    }
                    else
                        Debug.Log($"{submittedPlacards[i].name} failed");
                }

                if (successfulPlacards.Count == 2 && StrengthInNumbers != null)
                {
                    successfulPlacards.Add(StrengthInNumbers);
                }

                if (successfulPlacards.Count >= 1)
                {
                    Log.instance.AddText($"");
                    int totalPoints = (successfulPlacards.Count - 1);
                    int[] cardIDs = new int[submittedCards.Count];
                    int[] placardIDs = new int[successfulPlacards.Count];

                    for (int i = 0; i < submittedCards.Count; i++)
                    {
                        cardIDs[i] = submittedCards[i].pv.ViewID;
                        Log.instance.AddText($"{this.name} submits {submittedCards[i].logName}.");
                        submittedCards[i].pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
                    }
                    for (int i = 0; i < successfulPlacards.Count; i++)
                    {
                        totalPoints += successfulPlacards[i].rep;
                        placardIDs[i] = successfulPlacards[i].pv.ViewID;
                        Log.instance.AddText($"{this.name} submits {successfulPlacards[i].logName}.");
                        successfulPlacards[i].pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
                    }
                    this.pv.RPC("MadeSubmission", RpcTarget.All, totalPoints);
                    myButton.pv.RPC("MadeSubmission", RpcTarget.All, $"{this.name}: +{totalPoints}", placardIDs, cardIDs);
                    yield return ChoosePlacard();
                }
            }
        }
    }

    [PunRPC]
    public void MadeSubmission(int total)
    {
        Manager.instance.remainingsubmissions--;
        this.reputation += total;
        Log.instance.AddText($"{this.name} scores {total} REP.");
        this.personalsubmissions++;
        this.UpdateButtonText();
    }

    [PunRPC]
    public void DiscardCard(int cardID)
    {
        PhotonView.Find(cardID).transform.SetParent(Manager.instance.discard);
    }

    [PunRPC]
    public void RequestDraw(int cardsToDraw)
    {
        int[] cardIDs = new int[cardsToDraw];
        int drawnCards = 0;
        int lookingAtCards = 0;

        while (drawnCards < cardIDs.Length)
        {
            if (Manager.instance.deck.childCount == 0)
            {
                Manager.instance.discard.Shuffle();
                while (Manager.instance.discard.childCount > 0)
                    Manager.instance.discard.GetChild(0).SetParent(Manager.instance.deck);
            }

            Card x = Manager.instance.deck.GetChild(0).GetComponent<Card>();
            lookingAtCards++;
            if (x.eventtile)
            {
                photonView.RPC("DiscardCard", RpcTarget.All, x.pv.ViewID);
            }
            else
            {
                cardIDs[drawnCards] = x.pv.ViewID;
                x.transform.SetParent(null);
                drawnCards++;
            }
        }

        photonView.RPC("SendDraw", RpcTarget.All, cardIDs);
    }

    [PunRPC]
    IEnumerator SendDraw(int[] cardIDs)
    {
        for (int i = 0; i < cardIDs.Length; i++)
        {
            yield return new WaitForSeconds(0.02f);
            Card nextcard = PhotonView.Find(cardIDs[i]).gameObject.GetComponent<Card>();
            Log.instance.AddText($"{this.name} puts {nextcard.logName} in their hand.");

            if (this.cardhand.childCount == 0)
            {
                nextcard.transform.SetParent(this.cardhand);
                listOfHand.Add(nextcard);
            }

            else if (Manager.instance.sorting == Manager.Sorting.suit)
                AddCardBySuit(nextcard, 0, cardhand.childCount - 1);
            else
                AddCardByRank(nextcard, 0, cardhand.childCount - 1);
            nextcard.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    [PunRPC]
    public IEnumerator ChoosePlacard()
    {
        if (placardhand.childCount < 5)
        {
            choice = "";
            Manager.instance.instructions.text = $"Choose a Placard deck to draw from.";
            Manager.instance.AddTextButton(this, "1 REP", true);
            Manager.instance.AddTextButton(this, "2 REP", true);
            Manager.instance.AddTextButton(this, "4 REP", true);

            while (this.choice == "")
                yield return null;
            Manager.instance.ClearButtons();

            if (this.choice == "1 REP")
                pv.RPC("RequestPlacard", RpcTarget.MasterClient, 1);
            else if (this.choice == "2 REP")
                pv.RPC("RequestPlacard", RpcTarget.MasterClient, 2);
            else if (this.choice == "4 REP")
                pv.RPC("RequestPlacard", RpcTarget.MasterClient, 4);
        }
    }

    [PunRPC]
    public void RequestPlacard(int value)
    {
        Transform deckToFind = null;
        if (value == 1)
            deckToFind = Manager.instance.rep1deck.transform;
        if (value == 2)
            deckToFind = Manager.instance.rep2deck.transform;
        if (value == 4)
            deckToFind = Manager.instance.rep4deck.transform;

        Placard x = deckToFind.GetChild(0).GetComponent<Placard>();
        photonView.RPC("SendPlacard", RpcTarget.All, x.pv.ViewID);
    }

    [PunRPC]
    public void RemovePlacard(int viewID)
    {
        Placard nextPlacard = PhotonView.Find(viewID).gameObject.GetComponent<Placard>();
        nextPlacard.transform.SetParent(null);
        listOfPlacard.Remove(nextPlacard);
    }

    [PunRPC]
    public void SendPlacard(int viewID)
    {
        Placard nextPlacard = PhotonView.Find(viewID).gameObject.GetComponent<Placard>();
        nextPlacard.transform.SetParent(this.placardhand);
        nextPlacard.transform.localScale = new Vector3(1, 1, 1);
        listOfPlacard.Add(nextPlacard);

        if (pv.IsMine)
        {
            nextPlacard.image.sprite = nextPlacard.originalImage;
            Log.instance.AddText($"{this.name} gets {nextPlacard.logName}.");
        }
        else
        {
            Log.instance.AddText($"{this.name} gets a {nextPlacard.rep} REP placard.");
            switch (nextPlacard.rep)
            {
                case 1:
                    nextPlacard.image.sprite = Manager.instance.back1;
                    break;
                case 2:
                    nextPlacard.image.sprite = Manager.instance.back2;
                    break;
                case 4:
                    nextPlacard.image.sprite = Manager.instance.back4;
                    break;
            }
        }
    }

    [PunRPC]
    public IEnumerator TrashPlacard(Photon.Realtime.Player requestingplayer)
    {
        if (placardhand.childCount >= 2)
        {
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.EnableButton(this, true);

            choice = "";
            chosenPlacard = null;
            Manager.instance.instructions.text = $"Trash one of your Placards.";
            while (choice == "")
                yield return null;

            Log.instance.pv.RPC("AddText", RpcTarget.All, $"{this.name} trashes {chosenPlacard.logName}.");
            chosenPlacard.pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.DisableButton();
        }

        GameObject.Find(requestingplayer.NickName).GetComponent<PhotonView>().RPC("WaitDone", requestingplayer);
    }

    void AddCardBySuit(Card nextCard, int low, int high)
    {
        if (high <= low)
        {
            Card lowCard = cardhand.GetChild(low).GetComponent<Card>();

            if (nextCard.suitCode > lowCard.suitCode)
            {
                listOfHand.Insert(low + 1, nextCard);
                nextCard.transform.SetParent(cardhand.transform);
                nextCard.transform.SetSiblingIndex(low + 1);
                return;
            }
            else
            {
                listOfHand.Insert(low, nextCard);
                nextCard.transform.SetParent(cardhand.transform);
                nextCard.transform.SetSiblingIndex(low);
                return;
            }
        }

        int mid = (low + high) / 2;
        Card midCard = cardhand.GetChild(mid).GetComponent<Card>();

        if (nextCard.suitCode == midCard.suitCode)
        {
            listOfHand.Insert(mid + 1, nextCard);
            nextCard.transform.SetParent(cardhand.transform);
            nextCard.transform.SetSiblingIndex(mid + 1);
            return;
        }

        if (nextCard.suitCode > midCard.suitCode)
            AddCardBySuit(nextCard, mid + 1, high);
        else
            AddCardBySuit(nextCard, low, mid - 1);
    }

    void AddCardByRank(Card nextCard, int low, int high)
    {
        if (high <= low)
        {
            Card lowCard = cardhand.GetChild(low).GetComponent<Card>();

            if (nextCard.rank > lowCard.rank)
            {
                listOfHand.Insert(low + 1, nextCard);
                nextCard.transform.SetParent(cardhand.transform);
                nextCard.transform.SetSiblingIndex(low + 1);
                return;
            }
            else
            {
                listOfHand.Insert(low, nextCard);
                nextCard.transform.SetParent(cardhand.transform);
                nextCard.transform.SetSiblingIndex(low);
                return;
            }
        }

        int mid = (low + high) / 2;
        Card midCard = cardhand.GetChild(mid).GetComponent<Card>();

        if (nextCard.rank == midCard.rank)
        {
            listOfHand.Insert(mid + 1, nextCard);
            nextCard.transform.SetParent(cardhand.transform);
            nextCard.transform.SetSiblingIndex(mid + 1);
            return;
        }

        if (nextCard.rank > midCard.rank)
            AddCardBySuit(nextCard, mid + 1, high);
        else
            AddCardBySuit(nextCard, low, mid - 1);
    }

    public void SortHandBySuit()
    {
        listOfHand = listOfHand.OrderBy(o => o.suitCode).ToList();
        for (int i = 0; i < listOfHand.Count; i++)
        {
            PhotonView.Find(listOfHand[i].pv.ViewID).transform.SetSiblingIndex(i);
        }
    }

    public void SortHandByRank()
    {
        listOfHand = listOfHand.OrderBy(o => o.rank).ToList();
        for (int i = 0; i < listOfHand.Count; i++)
        {
            PhotonView.Find(listOfHand[i].pv.ViewID).transform.SetSiblingIndex(i);
        }
    }

    [PunRPC]
    public IEnumerator Text(Photon.Realtime.Player requestingplayer)
    {
        if (listOfPlacard.Count == 1)
        {
            TextManager.instance.pv.RPC("GetPlacard", requestingplayer, this.playerposition, listOfPlacard[0].pv.ViewID);
        }
        else
        {
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.EnableButton(this, true);

            choice = "";
            chosenPlacard = null;
            Manager.instance.instructions.text = $"Pass one of your Placards to {GetPreviousPlayer().name}.";
            while (choice == "")
                yield return null;

            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.DisableButton();

            TextManager.instance.pv.RPC("GetPlacard", requestingplayer, this.playerposition, chosenPlacard.pv.ViewID);
        }
        Manager.instance.instructions.text = $"Waiting...";
    }

    [PunRPC]
    public IEnumerator Coin(Photon.Realtime.Player requestingplayer)
    {
        yield return ChoosePlacard();
        GameObject.Find(requestingplayer.NickName).GetComponent<PhotonView>().RPC("WaitDone", requestingplayer);
    }
}