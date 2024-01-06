using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using MyBox;
using Unity.VisualScripting;

public class TileData : MonoBehaviourPunCallbacks
{
    [ReadOnly] public int row;
    [ReadOnly] public int column;
    [ReadOnly] public int position;
    [ReadOnly] public Sprite facedownsprite;
    [ReadOnly] public Card mycard;

    [ReadOnly] public List<TileData> adjacentTiles = new List<TileData>();
    [ReadOnly] PhotonView pv;
    [ReadOnly] public SendChoice choicescript;

    void Awake()
    {
        this.name = $"Tile {position}";
        pv = GetComponent<PhotonView>();
        choicescript = GetComponent<SendChoice>();
    }

    private void Start()
    {
        adjacentTiles.Add(Manager.instance.FindTile(this.row - 1, this.column));
        adjacentTiles.Add(Manager.instance.FindTile(this.row + 1, this.column));
        adjacentTiles.Add(Manager.instance.FindTile(this.row, this.column - 1));
        adjacentTiles.Add(Manager.instance.FindTile(this.row, this.column + 1));
    }

    public void NullCardRPC()
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("NullCard", RpcTarget.All);
        else
            NullCard();
    }

    [PunRPC]
    void NullCard()
    {
        if (mycard != null)
        {
            mycard.transform.SetParent(Manager.instance.discard);
            StartCoroutine(mycard.MoveCard(new Vector2(-2000, -330), new Vector3(0, 0, 0), 0.3f));
            mycard = null;
        }
    }

    public void CardFromDeckRPC()
    {
        if (PhotonNetwork.IsConnected)
        {
            pv.RPC("CardFromDeck", RpcTarget.MasterClient);
        }
        else
        {
            CardFromDeck();
        }
    }

    [PunRPC]
    void CardFromDeck()
    {
        if (Manager.instance.deck.childCount == 0)
        {
            Manager.instance.discard.Shuffle();
            while (Manager.instance.discard.childCount > 0)
                Manager.instance.discard.GetChild(0).SetParent(Manager.instance.deck);
        }

        NewCardRPC(Manager.instance.deck.GetChild(0).GetComponent<Card>(), false);
    }

    public void NewCardRPC(Card card, bool faceUp)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("NewCard", RpcTarget.All, card.pv.ViewID, faceUp);
        else
            NewCard(card, faceUp);
    }

    void NewCard(Card card, bool faceUp)
    {
        mycard = card;
        card.transform.SetParent(this.transform);
        card.transform.SetAsFirstSibling();
        card.image.sprite = faceUp ? card.originalSprite : facedownsprite;
        StartCoroutine(card.MoveCard(new Vector2(0, 0), new Vector3(0, 0), 0.3f));
    }

    [PunRPC]
    void NewCard(int cardID, bool faceUp)
    {
        NewCard(PhotonView.Find(cardID).gameObject.GetComponent<Card>(), faceUp);
    }

    public void FlipCardRPC(bool faceUp)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("FlipCard", RpcTarget.All, faceUp);
        else
            FlipCard(faceUp);
    }

    [PunRPC]
    void FlipCard(bool faceUp)
    {
        if (mycard != null)
        {
            StartCoroutine(mycard.FlipCard(0.2f, faceUp ? mycard.originalSprite : facedownsprite));
        }
    }

}
