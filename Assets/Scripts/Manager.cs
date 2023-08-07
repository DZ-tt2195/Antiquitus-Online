using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour, IOnEventCallback
{
    public static Manager instance;
    public int remainingsubmissions;
    public List<PlayerButton> playerButtonClone;

    public Sprite back1;
    public Sprite back2;
    public Sprite back4;

    Transform abilityCollector;
    Transform textCollector;
    public SendChoice cardButton;
    public SendChoice textButton;
    public SendChoice placardButton;
    List<SendChoice> buttonsInCollector = new List<SendChoice>();

    public List<TileData> listoftiles = new List<TileData>();
    public List<BoneArrow> listofarrows = new List<BoneArrow>();
    public List<WeaponBox> listofboxes = new List<WeaponBox>();

    [HideInInspector] public Transform trash;
    [HideInInspector] public TMP_Text instructions;
    [HideInInspector] public Transform deck;
    [HideInInspector] public Transform discard;
    [HideInInspector] public Transform rep1deck;
    [HideInInspector] public Transform rep2deck;
    [HideInInspector] public Transform rep4deck;

    [HideInInspector] public List<Player> playerordergameobject = new List<Player>();
    [HideInInspector] public List<Photon.Realtime.Player> playerorderphotonlist = new List<Photon.Realtime.Player>();

    [HideInInspector] public float opacity = 1;
    [HideInInspector] public bool decrease = true;
    [HideInInspector] public bool gameon = false;

    public enum Sorting { suit, rank }
    public Sorting sorting = Sorting.suit;

    [HideInInspector] public const byte AddNextPlayerEvent = 1;
    [HideInInspector] public const byte AssignCardToSiteEvent = 2;
    [HideInInspector] public const byte SubmissionEvent = 3;
    [HideInInspector] public const byte GameOverEvent = 4;

    Button sortingButton;
    TMP_Text submissionText;
    TMP_Text sortingText;
    GameObject blownUp;
    TMP_Text endText;
    Button leaveRoom;

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    void Awake()
    {
        instance = this;
        submissionText = GameObject.Find("Submissions Left").GetComponent<TMP_Text>();
        instructions = GameObject.Find("Instructions").GetComponent<TMP_Text>();
        deck = GameObject.Find("Deck").transform;
        discard = GameObject.Find("Discard").transform;
        rep1deck = GameObject.Find("1REP Deck").transform;
        rep2deck = GameObject.Find("2REP Deck").transform;
        rep4deck = GameObject.Find("4REP Deck").transform;
        abilityCollector = GameObject.Find("Ability Collector").transform;
        textCollector = GameObject.Find("Text Collector").transform;
        sortingButton = GameObject.Find("Sorting Button").GetComponent<Button>();
        sortingText = sortingButton.GetComponentInChildren<TMP_Text>();
        sortingButton.onClick.AddListener(ChangeSorting);
        blownUp = GameObject.Find("Blown Up");
        endText = GameObject.Find("Endtext").GetComponent<TMP_Text>();
        leaveRoom = GameObject.Find("Leave Room").GetComponent<Button>();
    }

    public void Update()
    {
        submissionText.text = $"{remainingsubmissions} Submissions Left";
        if (Input.GetMouseButtonDown(0))
            blownUp.SetActive(false);
    }

    public void ChangeSorting()
    {
        if (sorting == Sorting.suit)
        {
            sorting = Sorting.rank;
            sortingText.text = "Sorted by rank";
            for (int i = 0; i < playerordergameobject.Count; i++)
            {
                playerordergameobject[i].SortHandByRank();
            }
        }
        else
        {
            sorting = Sorting.suit;
            sortingText.text = "Sorted by suit";
            for (int i = 0; i < playerordergameobject.Count; i++)
            {
                playerordergameobject[i].SortHandBySuit();
            }
        }
    }

    void Start()
    {
        leaveRoom.onClick.AddListener(Quit);
        endText.transform.parent.gameObject.SetActive(false);
        blownUp.SetActive(false);
        abilityCollector.gameObject.SetActive(false);
        textCollector.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            deck.Shuffle();
            rep1deck.Shuffle();
            rep2deck.Shuffle();
            rep4deck.Shuffle();
        }
        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        remainingsubmissions = 3 * PhotonNetwork.CurrentRoom.MaxPlayers;
        GameObject x = GameObject.Find("Store Players").gameObject;
        while (x.transform.childCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            instructions.text = $"Waiting for more players...({PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
            yield return null;
        }

        instructions.text = "Everyone's in! Setting up...";
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
        {
            List<Photon.Realtime.Player> playerassignment = new List<Photon.Realtime.Player>();
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                playerassignment.Add(PhotonNetwork.PlayerList[i]);

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                object[] sendingdata = new object[2];
                sendingdata[0] = i;

                int randomremove = UnityEngine.Random.Range(0, playerassignment.Count);
                sendingdata[1] = playerassignment[randomremove];
                playerassignment.RemoveAt(randomremove);

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(AddNextPlayerEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
            }

            for (int i = 0; i<listoftiles.Count; i++)
            {
                object[] sendingdata = new object[1];
                sendingdata[0] = i;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                { Receivers = ReceiverGroup.MasterClient };
                PhotonNetwork.RaiseEvent(AssignCardToSiteEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
            }

            yield return new WaitForSeconds(1f);
            listoftiles[0].pv.RPC("FlipTile", RpcTarget.All, true);
            listoftiles[4].pv.RPC("FlipTile", RpcTarget.All, true);
            listoftiles[12].pv.RPC("FlipTile", RpcTarget.All, true);
            listoftiles[20].pv.RPC("FlipTile", RpcTarget.All, true);
            listoftiles[24].pv.RPC("FlipTile", RpcTarget.All, true);

            for (int i = 0; i < playerordergameobject.Count; i++)
            {
                playerordergameobject[i].pv.RPC("RequestDraw", RpcTarget.MasterClient, 2);
                playerordergameobject[i].pv.RPC("RequestPlacard", RpcTarget.MasterClient, 1);
                playerordergameobject[i].pv.RPC("RequestPlacard", RpcTarget.MasterClient, 2);
                playerordergameobject[i].pv.RPC("RequestPlacard", RpcTarget.MasterClient, 4);
            }
            gameon = true;
            while (gameon)
            {
                for (int i = 0; i < playerordergameobject.Count; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    yield return playerordergameobject[i].TakeTurnRPC(playerordergameobject[i].photonplayer);

                    if (remainingsubmissions == 0)
                        break;
                }
            }

            GameOver("All submissions have been made.", -1);
        }
    }

    public void GameOver(string endText, int resignPosition)
    {
        Debug.Log($"{endText}, {resignPosition}");
        object[] sendingdata = new object[2];
        sendingdata[0] = endText;
        sendingdata[1] = resignPosition;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameOverEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == AddNextPlayerEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            int playerposition = (int)data[0];
            Photon.Realtime.Player playername = (Photon.Realtime.Player)data[1];

            playerordergameobject.Add(GameObject.Find(playername.NickName).GetComponent<Player>());
            playerorderphotonlist.Add(playername);

            PlayerButton nextButton = playerButtonClone[playerposition];
            nextButton.transform.localPosition = new Vector3(-1000, 300 - 100 * playerposition, 0);
            nextButton.name = $"{playername.NickName}'s Button";
            nextButton.transform.localScale = new Vector3(2.5f, 2.5f, 0);

            switch (playerposition)
            {
                case 0:
                    nextButton.image.color = Color.red;
                    break;
                case 1:
                    nextButton.image.color = Color.blue;
                    break;
                case 2:
                    nextButton.image.color = Color.yellow;
                    break;
                case 3:
                    nextButton.image.color = Color.white;
                    break;
            }

            playerordergameobject[playerposition].pv.RPC("AssignInfo", RpcTarget.All, playerposition, playername);
        }
        else if (photonEvent.Code == AssignCardToSiteEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            int tileposition = (int)data[0];

            if (deck.childCount == 0)
            {
                instance.discard.Shuffle();
                while (instance.discard.childCount > 0)
                    instance.discard.GetChild(0).SetParent(Manager.instance.deck);
            }
            listoftiles[tileposition].pv.RPC("NewCard", RpcTarget.All, instance.deck.GetChild(0).GetComponent<PhotonView>().ViewID, false);
        }
        else if (photonEvent.Code == GameOverEvent)
        {
            object[] data = (object[])photonEvent.CustomData;
            string endgame = (string)data[0];
            int resigningPlayer = (int)data[1];
            Player rp = null;

            endText.transform.parent.gameObject.SetActive(true);
            instructions.text = endgame;
            abilityCollector.gameObject.SetActive(false);
            textCollector.gameObject.SetActive(false);
            blownUp.SetActive(false);

            if (resigningPlayer > -1)
                rp = playerordergameobject[resigningPlayer];

            playerordergameobject = playerordergameobject.OrderByDescending(o => o.reputation).ToList();
            endText.text = "";
            int playerTracker = 1;

            for (int i = 0; i< playerordergameobject.Count; i++)
            {
                if (i != resigningPlayer)
                {
                    endText.text += $"\n{playerTracker}: {playerordergameobject[i].name}, {playerordergameobject[i].reputation} REP";
                    playerTracker++;
                }
            }
            if (rp != null)
                endText.text += $"\n\nResigned: {rp.name}: {rp.reputation} REP";
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void AddCardButton(Player player, Card card, bool enabled)
    {
        abilityCollector.gameObject.SetActive(true);
        SendChoice nextCard = Instantiate(cardButton, abilityCollector.GetChild(1));
        nextCard.card = card;
        nextCard.image.sprite = card.image.sprite;
        if (enabled)
            nextCard.EnableButton(player, false);
        buttonsInCollector.Add(nextCard);
    }

    public void AddPlacardButton(Player player, Placard placard, bool enabled)
    {
        abilityCollector.gameObject.SetActive(true);
        SendChoice nextCard = Instantiate(placardButton, abilityCollector.GetChild(1));
        nextCard.myPlacard = placard;
        nextCard.image.sprite = placard.image.sprite;
        if (enabled)
            nextCard.EnableButton(player, false);
        buttonsInCollector.Add(nextCard);
    }

    public void AddTextButton(Player player, string text, bool enabled)
    {
        textCollector.gameObject.SetActive(true);
        SendChoice nextCard = Instantiate(textButton, textCollector.GetChild(1));
        nextCard.textbox.text = text;
        if (enabled)
            nextCard.EnableButton(player, false);
        buttonsInCollector.Add(nextCard);
    }

    public void ClearButtons()
    {
        abilityCollector.gameObject.SetActive(false);
        textCollector.gameObject.SetActive(false);
        while (buttonsInCollector.Count > 0)
        {
            Destroy(buttonsInCollector[0].gameObject);
            buttonsInCollector.RemoveAt(0);
        }
    }

    public void NewImage(Image image, bool card)
    {
        blownUp.transform.SetAsLastSibling();
        blownUp.SetActive(true);

        blownUp.transform.GetChild(0).GetComponent<Image>().sprite = image.sprite;
        blownUp.transform.GetChild(0).gameObject.SetActive(!card);

        blownUp.transform.GetChild(1).gameObject.SetActive(card);

        blownUp.transform.GetChild(2).gameObject.GetComponent<Image>().sprite = image.sprite;
        blownUp.transform.GetChild(2).gameObject.SetActive(card);
    }

    public void Quit()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("1. Lobby");
    }
}
