using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class Card : MonoBehaviour
{
    [HideInInspector] public enum CardType { Coin, Bone, Weapon, Text, Treasure, CaveIn };
    [HideInInspector] public CardType type;
    [HideInInspector] public bool eventtile;
    public int rank;

    public PhotonView pv;
    public Image image;
    public SendChoice choicescript;
    [HideInInspector] public int suitCode;

    // Start is called before the first frame update
    void Start()
    {
        pv = this.GetComponent<PhotonView>();
        image = GetComponent<Image>();
        choicescript = GetComponent<SendChoice>();
        Setup();

        switch (type)
        {
            case CardType.Coin:
                suitCode = 0 + rank;
                break;
            case CardType.Bone:
                suitCode = 10 + rank;
                break;
            case CardType.Weapon:
                suitCode = 20 + rank;
                break;
            case CardType.Text:
                suitCode = 30 + rank;
                break;
        }
    }

    public virtual void Setup()
    {

    }

    public virtual IEnumerator OnTakeEffect(Player player)
    {
        yield return null;
    }

    public virtual IEnumerator OnDiscardEffect(Player player)
    {
        yield return null;
    }

    [PunRPC]
    public void TrashThis(int playerOrder)
    {
        this.gameObject.transform.SetParent(Manager.instance.trash);
        if (playerOrder > -1)
            Manager.instance.playerordergameobject[playerOrder].listOfHand.Remove(this);
    }
}
