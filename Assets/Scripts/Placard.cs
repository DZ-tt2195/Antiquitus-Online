using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using MyBox;

public class Placard : MonoBehaviour
{
    [ReadOnly] public int rep;
    [ReadOnly] public PhotonView pv;
    [ReadOnly] public Image image;
    [ReadOnly] public SendChoice choicescript;
    [ReadOnly] public Sprite originalImage;
    [ReadOnly] public string logName;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>();
        image = GetComponent<Image>();
        originalImage = image.sprite;
        choicescript = GetComponent<SendChoice>();
    }

    void Start()
    {
        logName = this.name;
        Setup();
    }

    public virtual void Setup()
    {

    }

    public virtual bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        return false;
    }

    [PunRPC]
    public void TrashThis(int playerOrder)
    {
        this.gameObject.transform.SetParent(Manager.instance.trash);
        if (playerOrder > -1)
            Manager.instance.playerordergameobject[playerOrder].listOfPlacard.Remove(this);
    }
}
