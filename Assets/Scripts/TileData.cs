using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class TileData : MonoBehaviourPunCallbacks
{
    public int row;
    public int column;
    public int position;
    public bool faceup = false;
    public Sprite facedownsprite;
    public Card mycard;

    [HideInInspector] public List<TileData> adjacentTiles = new List<TileData>();

    [HideInInspector] public PhotonView pv;
    [HideInInspector] public SendChoice choicescript;
    [HideInInspector] public Image image;

    // Start is called before the first frame update
    void Awake()
    {
        this.name = $"Tile {position}";
        pv = GetComponent<PhotonView>();
        image = GetComponent<Image>();
        choicescript = GetComponent<SendChoice>();
    }

    private void Start()
    {
        adjacentTiles.Add(Manager.instance.FindTile(this.row - 1, this.column));
        adjacentTiles.Add(Manager.instance.FindTile(this.row + 1, this.column));
        adjacentTiles.Add(Manager.instance.FindTile(this.row, this.column - 1));
        adjacentTiles.Add(Manager.instance.FindTile(this.row, this.column + 1));
    }

    [PunRPC]
    public void NullCard()
    {
        mycard = null;
        image.sprite = null;
    }

    [PunRPC]
    public void ReplaceCard()
    {
        object[] sendingdata = new object[1];
        sendingdata[0] = this.position;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(Manager.AssignCardToSiteEvent, sendingdata, raiseEventOptions, SendOptions.SendReliable);
    }

    [PunRPC]
    public void NewCard(int cardID, bool x)
    {
        mycard = PhotonView.Find(cardID).gameObject.GetComponent<Card>();
        mycard.transform.SetParent(null);
        this.pv.RPC("FlipTile", RpcTarget.All, x);
    }

    [PunRPC]
    public void FlipTile()
    {
        this.pv.RPC("FlipTile", RpcTarget.All, !faceup);
    }

    [PunRPC]
    public void FlipTile(bool x)
    {
        if (x)
        {
            this.image.sprite = mycard.image.sprite;
            faceup = true;
        }
        else
        {
            this.image.sprite = facedownsprite;
            faceup = false;
        }
    }
}
