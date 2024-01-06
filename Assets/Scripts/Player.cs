using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;
using MyBox;

public class Player : MonoBehaviourPunCallbacks
{

#region Variables

    [ReadOnly] public PhotonView pv;
    [ReadOnly] public int playerposition;

    [Foldout("Cards", true)]
    [SerializeField] Transform cardhand;
    [SerializeField] Transform placardhand;
    [ReadOnly] public List<Card> listOfHand = new List<Card>();
    [ReadOnly] public List<Placard> listOfPlacard = new List<Placard>();

    [Foldout("UI", true)]
    Canvas canvas;
    Button resign;
    [ReadOnly] public PlayerButton myButton;
    [ReadOnly] Transform arrow;
    Transform storePlayers;
    int movePosition;

    [Foldout("Decisions", true)]

    [ReadOnly] public string choice;
    [ReadOnly] public Placard chosenPlacard;
    [ReadOnly] public Card chosencard;
    [ReadOnly] public TileData chosentile;
    [ReadOnly] public WeaponBox chosenbox;
    [ReadOnly] public BoneArrow chosenarrow;

    [Foldout("Turns", true)]
    [ReadOnly] public bool waiting;
    [ReadOnly] public bool turnon;
    [ReadOnly] public Card cardthisturn;
    [ReadOnly] public TileData tilethisturn;
    [ReadOnly] public int cardsdiscarded;
    [ReadOnly] public int cardsrevealed;
    [ReadOnly] public bool eventactivated;

    [Foldout("Score", true)]
    [ReadOnly] public int turns;
    [ReadOnly] public int reputation;
    [ReadOnly] public int personalsubmissions;

    #endregion

#region Setup

    private void Awake()
    {
        resign = GameObject.Find("Resign Button").GetComponent<Button>();
        storePlayers = GameObject.Find("Store Players").transform;
        arrow = GameObject.Find("Arrow").transform;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            this.name = pv.Owner.NickName;
        if (!PhotonNetwork.IsConnected && this.pv.AmController)
            resign.onClick.AddListener(ResignTime);

        this.transform.SetParent(storePlayers);
    }

    [PunRPC]
    public void AssignInfo(int position)
    {
        this.transform.localPosition = new Vector3(-280 + 2000 * playerposition, 0, 0);
        this.playerposition = position;
        this.myButton = GameObject.Find($"{this.name}'s Button").GetComponent<PlayerButton>();
        UpdateButtonText();

        if (PhotonNetwork.IsConnected)
        {
            pv.RPC("ButtonClick", RpcTarget.All);

            if (pv.AmOwner)
                ClickMe();
        }
        else
        {
            ButtonClick();
            ClickMe();
        }
    }

    #endregion

#region Buttons

    public void UpdateButtonTextRPC()
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("UpdateButtonText", RpcTarget.All);
        else
            UpdateButtonText();
    }

    [PunRPC]
    void UpdateButtonText()
    {
        myButton.textbox.text = $"{this.name}: {reputation} REP";
    }

    [PunRPC]
    void ButtonClick()
    {
        movePosition = -2000 * playerposition;
        myButton.button.onClick.AddListener(ClickMe);
    }

    void ClickMe()
    {
        storePlayers.transform.localPosition = new Vector3(movePosition, 0, 0);
        arrow.transform.SetParent(myButton.transform);
        arrow.transform.localPosition = new Vector3(160, 0, 0);
    }

    void ResignTime()
    {
        Manager.instance.GameOver($"{this.name} has resigned.", this.playerposition);
    }

    #endregion

#region Turns

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

    public IEnumerator TakeTurnRPC()
    {
        if (PhotonNetwork.IsConnected)
            photonView.RPC("TakeTurn", pv.Controller);
        else
            StartCoroutine(TakeTurn());

        turnon = true;
        while (turnon)
            yield return null;
    }

    [PunRPC]
    void TurnOver()
    {
        turnon = false;
    }

    [PunRPC]
    IEnumerator TakeTurn()
    {
        turns++;
        ClickMe();

        if (PhotonNetwork.IsConnected)
            photonView.RPC("WaitForPlayer", RpcTarget.Others, this.name);

        Log.instance.AddTextRPC($"");
        Log.instance.AddTextRPC($"{this.name}'s Turn");

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
        if (PhotonNetwork.IsConnected) photonView.RPC("TurnOver", RpcTarget.All); else TurnOver();
    }


    IEnumerator ChooseTileInSite()
    {
        for (int i = 0; i < Manager.instance.listoftiles.Count; i++)
        {
            TileData nexttile = Manager.instance.listoftiles[i];
            if (nexttile.mycard.IsPublic() && !nexttile.mycard.eventtile)
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
        if (PhotonNetwork.IsConnected) pv.RPC("SendDraw", RpcTarget.All, cardIDs); else SendDraw(cardIDs);
        chosentile.CardFromDeckRPC();
        Log.instance.AddTextRPC($"");
    }

    IEnumerator ResolveAdjacentTile(TileData nextTile)
    {
        if (nextTile != null)
        {
            if (nextTile.mycard.IsPublic())
            {
                cardsdiscarded++;
                Card discardedCard = nextTile.mycard;
                Log.instance.AddTextRPC($"{this.name} discards {nextTile.mycard.logName}.");

                nextTile.CardFromDeckRPC();
                DiscardCardRPC(discardedCard);
                yield return discardedCard.OnDiscardEffect(this);
            }
            else
            {
                cardsrevealed++;
                nextTile.FlipCardRPC(true);
                Log.instance.AddTextRPC($"{this.name} reveals {nextTile.mycard.logName}.");
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
                    Log.instance.AddTextRPC($"");
                    int totalPoints = (successfulPlacards.Count - 1);
                    int[] cardIDs = new int[submittedCards.Count];
                    int[] placardIDs = new int[successfulPlacards.Count];

                    for (int i = 0; i < submittedCards.Count; i++)
                    {
                        cardIDs[i] = submittedCards[i].pv.ViewID;
                        Log.instance.AddTextRPC($"{this.name} submits {submittedCards[i].logName}.");
                        submittedCards[i].pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
                    }
                    for (int i = 0; i < successfulPlacards.Count; i++)
                    {
                        totalPoints += successfulPlacards[i].rep;
                        placardIDs[i] = successfulPlacards[i].pv.ViewID;
                        Log.instance.AddTextRPC($"{this.name} submits {successfulPlacards[i].logName}.");
                        successfulPlacards[i].pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
                    }

                    if (PhotonNetwork.IsConnected) this.pv.RPC("MadeSubmission", RpcTarget.All, totalPoints); else MadeSubmission(totalPoints);
                    myButton.pv.RPC("MadeSubmission", RpcTarget.All, $"{this.name}: +{totalPoints}", placardIDs, cardIDs);
                    Log.instance.AddTextRPC($"{this.name} scores {totalPoints} REP.");

                    yield return ChoosePlacard();
                }
            }
        }
    }

    [PunRPC]
    void MadeSubmission(int total)
    {
        Manager.instance.remainingsubmissions--;
        this.reputation += total;
        this.personalsubmissions++;
        this.UpdateButtonText();
    }

#endregion

#region Cards

    public void SortHand()
    {
        float firstCalc = Mathf.Round(canvas.transform.localScale.x * 4) / 4f;
        float multiplier = firstCalc / 0.25f;

        for (int i = 0; i < listOfHand.Count; i++)
        {
            Card nextCard = listOfHand[i];
            float startingX = (listOfHand.Count > 7) ? (-300 - (150 * multiplier)) : (listOfHand.Count - 1) * (-50 - 25 * multiplier);
            float difference = (listOfHand.Count > 7) ? (-300 - (150 * multiplier)) * -2 / (listOfHand.Count - 1) : 100 + (50 * multiplier);
            Vector2 newPosition = new(startingX + difference * i, -600 * canvas.transform.localScale.x);
            StartCoroutine(nextCard.MoveCard(newPosition, nextCard.transform.localEulerAngles, 0.3f));
        }

        UpdateButtonTextRPC();
    }

    public void DiscardCardRPC(Card card)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("DiscardCard", RpcTarget.All, card.pv.ViewID);
        else
            DiscardCard(card);
    }

    void DiscardCard(Card discardMe)
    {
        listOfHand.Remove(discardMe);
        SortHand();

        discardMe.transform.SetParent(Manager.instance.discard);
        StartCoroutine(discardMe.MoveCard(new Vector2(-2000, -330), new Vector3(0, 0, 0), 0.3f));
    }

    [PunRPC]
    void DiscardCard(int cardID)
    {
        Card discardMe = PhotonView.Find(cardID).GetComponent<Card>();
        DiscardCard(discardMe);
    }

    public void DrawCardRPC(int cardsToDraw)
    {
        if (PhotonNetwork.IsConnected)
            this.pv.RPC("RequestDraw", RpcTarget.MasterClient, cardsToDraw);
        else
            RequestDraw(cardsToDraw);
    }

    [PunRPC]
    void RequestDraw(int cardsToDraw)
    {
        Card[] listOfDraw = new Card[cardsToDraw];
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

            Card nextCard = Manager.instance.deck.GetChild(0).GetComponent<Card>();
            nextCard.transform.SetParent(null);
            lookingAtCards++;

            if (nextCard.eventtile)
            {
                DiscardCardRPC(nextCard);
            }
            else
            {
                listOfDraw[drawnCards] = nextCard;
                cardIDs[drawnCards] = nextCard.pv.ViewID;
                drawnCards++;
            }
        }

        if (PhotonNetwork.IsConnected)
            pv.RPC("SendDraw", RpcTarget.All, cardIDs);
        else
            AddToHand(listOfDraw);
    }

    void AddToHand(Card[] listOfDraw)
    {
        foreach (Card card in listOfDraw)
        {
            listOfHand.Add(card);
            card.transform.SetParent(cardhand);
            card.transform.localPosition = new Vector2(0, -1000);

            if (PhotonNetwork.IsConnected)
                Log.instance.AddTextRPC($"{this.name} puts {card.logName} in their hand.");
            else
                Log.instance.AddTextRPC($"{this.name} put {card.logName} in your hand.");
        }

        if (Manager.instance.sorting == Manager.Sorting.suit)
            SortHandBySuit();
        else
            SortHandByRank();
    }

    [PunRPC]
    void SendDraw(int[] cardIDs)
    {
        Card[] listOfCards = new Card[cardIDs.Length];
        for (int i = 0; i < cardIDs.Length; i++)
            listOfCards[i] = PhotonView.Find(cardIDs[i]).gameObject.GetComponent<Card>();

        AddToHand(listOfCards);
    }

    public void SortHandBySuit()
    {
        listOfHand = listOfHand.OrderBy(o => o.suitCode).ToList();
        SortHand();
    }

    public void SortHandByRank()
    {
        listOfHand = listOfHand.OrderBy(o => o.rank).ToList();
        SortHand();
    }

    #endregion

#region Placards

    public void SortPlacards()
    {
        float firstCalc = Mathf.Round(canvas.transform.localScale.x * 4) / 4f;
        float multiplier = firstCalc / 0.25f;

        for (int i = 0; i < listOfPlacard.Count; i++)
        {
            Placard nextCard = listOfPlacard[i];
            float startingY = 115 * multiplier;
            float difference = 50 * multiplier;
            Vector2 newPosition = new(850 * canvas.transform.localScale.x, startingY-difference*i);
            StartCoroutine(nextCard.MoveCard(newPosition, nextCard.transform.localEulerAngles, 0.3f));
        }

        UpdateButtonTextRPC();
    }

    [PunRPC]
    IEnumerator ChoosePlacard()
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

    public void RequestPlacardRPC(int value)
    {
        if (PhotonNetwork.IsConnected)
        {
            pv.RPC("RequestPlacard", RpcTarget.MasterClient, value);
        }
        else
        {
            RequestPlacard(value);
        }
    }

    [PunRPC]
    void RequestPlacard(int value)
    {
        Transform deckToFind = null;
        if (value == 1)
            deckToFind = Manager.instance.rep1deck.transform;
        if (value == 2)
            deckToFind = Manager.instance.rep2deck.transform;
        if (value == 4)
            deckToFind = Manager.instance.rep4deck.transform;

        Placard x = deckToFind.GetChild(0).GetComponent<Placard>();
        if (PhotonNetwork.IsConnected)
            photonView.RPC("SendPlacard", RpcTarget.All, x.pv.ViewID);
        else
            AddPlacard(x);
    }

    void AddPlacard(Placard placard)
    {
        placard.transform.SetParent(this.placardhand);
        listOfPlacard.Add(placard);
        placard.transform.localPosition = new Vector2(1000, 1000);

        if (!PhotonNetwork.IsConnected || pv.IsMine)
        {
            placard.image.sprite = placard.originalImage;
            Log.instance.AddTextSecret($"{this.name} gets {placard.logName}.");
        }
        else
        {
            Log.instance.AddTextRPC($"{this.name} gets a {placard.rep} REP placard.");
            switch (placard.rep)
            {
                case 1:
                    placard.image.sprite = Manager.instance.back1;
                    break;
                case 2:
                    placard.image.sprite = Manager.instance.back2;
                    break;
                case 4:
                    placard.image.sprite = Manager.instance.back4;
                    break;
            }
        }

        SortPlacards();
    }

    [PunRPC]
    void ReceivePlacard(int ID)
    {
        AddPlacard(PhotonView.Find(ID).gameObject.GetComponent<Placard>());
    }

    [PunRPC]
    public void RemovePlacard(int viewID)
    {
        Placard nextPlacard = PhotonView.Find(viewID).gameObject.GetComponent<Placard>();
        nextPlacard.transform.SetParent(null);
        listOfPlacard.Remove(nextPlacard);
    }

#endregion

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

            Log.instance.AddTextRPC($"{this.name} trashes {chosenPlacard.logName}.");
            chosenPlacard.pv.RPC("TrashThis", RpcTarget.All, this.playerposition);
            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < listOfPlacard.Count; i++)
                listOfPlacard[i].choicescript.DisableButton();
        }

        GameObject.Find(requestingplayer.NickName).GetComponent<PhotonView>().RPC("WaitDone", requestingplayer);
    }

}