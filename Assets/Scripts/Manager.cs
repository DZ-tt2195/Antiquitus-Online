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
using MyBox;

public class Manager : MonoBehaviour, IOnEventCallback
{

#region Variables

    public static Manager instance;

    [ReadOnly] public const byte AddNextPlayerEvent = 1;
    [ReadOnly] public const byte SubmissionEvent = 3;
    [ReadOnly] public const byte GameOverEvent = 4;

    [Foldout("Prefabs", true)]
        [SerializeField] List<PlayerButton> playerButtonClone;
        public Sprite back1;
        public Sprite back2;
        public Sprite back4;
        [SerializeField] Player playerClone;
        [SerializeField] SendChoice cardButton;
        [SerializeField] SendChoice textButton;
        [SerializeField] SendChoice placardButton;
    
    [Foldout("Lists", true)]
        public int remainingsubmissions;
        [ReadOnly] public List<TileData> listoftiles = new List<TileData>();
        public List<BoneArrow> listofarrows = new List<BoneArrow>();
        public List<WeaponBox> listofboxes = new List<WeaponBox>();
        [ReadOnly] public List<Player> playerordergameobject = new List<Player>();

    [Foldout("Zones", true)]
        [ReadOnly] public Transform trash;
        [ReadOnly] public TMP_Text instructions;
        [ReadOnly] public Transform deck;
        [ReadOnly] public Transform discard;
        [ReadOnly] public Transform rep1deck;
        [ReadOnly] public Transform rep2deck;
        [ReadOnly] public Transform rep4deck;

    [Foldout("UI", true)]
        Transform abilityCollector;
        Transform textCollector;
        List<SendChoice> buttonsInCollector = new List<SendChoice>();
        [ReadOnly] public GameObject blownUp;
        [ReadOnly] public TMP_Text endText;
        [ReadOnly] public Button leaveRoom;
        Button sortingButton;
        TMP_Text submissionText;
        TMP_Text sortingText;

    [Foldout("Misc", true)]
        [ReadOnly] public float opacity = 1;
        [ReadOnly] public bool decrease = true;
        [ReadOnly] public bool gameon = false;
        public enum Sorting { suit, rank }
        [ReadOnly] public Sorting sorting = Sorting.suit;

    #endregion

#region Setup

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
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
    }

    void Start()
    {
        leaveRoom.onClick.AddListener(Quit);
        endText.transform.parent.gameObject.SetActive(false);
        blownUp.SetActive(false);
        abilityCollector.gameObject.SetActive(false);
        textCollector.gameObject.SetActive(false);
        remainingsubmissions = (PhotonNetwork.IsConnected) ? 3 * PhotonNetwork.CurrentRoom.MaxPlayers : 3;

        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            deck.Shuffle();
            rep1deck.Shuffle();
            rep2deck.Shuffle();
            rep4deck.Shuffle();
        }

        if (PhotonNetwork.IsConnected)
        {
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            Player nextPlayer = Instantiate(playerClone);
            nextPlayer.transform.SetParent(GameObject.Find("Store Players").transform);
            nextPlayer.name = "You";
            AddPlayer(0, "You");
            StartCoroutine(PlayGame());
        }
    }

    IEnumerator WaitForPlayer()
    {
        GameObject x = GameObject.Find("Store Players");
        while (x.transform.childCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            instructions.text = $"Waiting for more players...({PhotonNetwork.PlayerList.Length}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
            yield return null;
        }

        instructions.text = "Everyone's in! Setting up...";
        yield return new WaitForSeconds(1f);

        if (PhotonNetwork.IsMasterClient)
        {
            yield return GetPlayers();
            yield return PlayGame();
        }
    }

    IEnumerator GetPlayers()
    {
        yield return null;
        List<Photon.Realtime.Player> playerassignment = new List<Photon.Realtime.Player>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            playerassignment.Add(PhotonNetwork.PlayerList[i]);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            object[] sendingdata = new object[2];
            sendingdata[0] = i;

            int randomremove = UnityEngine.Random.Range(0, playerassignment.Count);
            sendingdata[1] = playerassignment[randomremove].NickName;
            playerassignment.RemoveAt(randomremove);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(AddNextPlayerEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    void AddPlayer(int position, string name)
    {
        playerordergameobject.Add(GameObject.Find(name).GetComponent<Player>());

        PlayerButton nextButton = playerButtonClone[position];
        nextButton.transform.localPosition = new Vector3(-1100, 300 - 100 * position, 0);
        nextButton.name = $"{name}'s Button";

        switch (position)
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

        if (PhotonNetwork.IsConnected) playerordergameobject[position].pv.RPC("AssignInfo", RpcTarget.All, position); else playerordergameobject[position].AssignInfo(position);

    }

    #endregion

#region Gameplay

    IEnumerator PlayGame()
    {
        for (int i = 0; i < listoftiles.Count; i++)
            listoftiles[i].CardFromDeckRPC();

        yield return new WaitForSeconds(0.5f);

        listoftiles[0].FlipCardRPC(true);
        listoftiles[4].FlipCardRPC(true);
        listoftiles[12].FlipCardRPC(true);
        listoftiles[20].FlipCardRPC(true);
        listoftiles[24].FlipCardRPC(true);

        for (int i = 0; i < playerordergameobject.Count; i++)
        {
            playerordergameobject[i].DrawCardRPC(2);
            playerordergameobject[i].RequestPlacardRPC(1);
            playerordergameobject[i].RequestPlacardRPC(2);
            playerordergameobject[i].RequestPlacardRPC(4);
        }

        yield return new WaitForSeconds(0.5f);
        gameon = true;

        while (gameon)
        {
            for (int i = 0; i < playerordergameobject.Count; i++)
            {
                yield return playerordergameobject[i].TakeTurnRPC();
                yield return new WaitForSeconds(0.5f);

                if (remainingsubmissions <= 0)
                    gameon = false;
            }
        }

        GameOver("All submissions have been made.", -1);
    }

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
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
            string playername = (string)data[1];
            AddPlayer(playerposition, playername);
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
            Log.instance.transform.SetAsLastSibling();

            if (resigningPlayer > -1)
                rp = playerordergameobject[resigningPlayer];

            playerordergameobject = playerordergameobject.OrderByDescending(o => o.reputation).ToList();
            endText.text = "";
            int playerTracker = 1;

            for (int i = 0; i < playerordergameobject.Count; i++)
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

    #endregion

#region Misc

    public TileData FindTile(int row, int col)
    {
        if (row < 0 || row > 4 || col < 0 || col > 4)
            return null;
        else
            return this.listoftiles[row * 5 + col];
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
        SendChoice nextCard = Instantiate(placardButton, abilityCollector.GetChild(2));
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

#endregion

}
