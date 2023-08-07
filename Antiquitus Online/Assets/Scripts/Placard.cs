using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Placard : MonoBehaviour
{
    [HideInInspector] public int rep;
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public Image image;
    [HideInInspector] public SendChoice choicescript;
    [HideInInspector] public Sprite originalImage;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>();
        image = GetComponent<Image>();
        originalImage = image.sprite;
        choicescript = GetComponent<SendChoice>();
    }

    // Start is called before the first frame update
    void Start()
    {
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
