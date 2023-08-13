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
    [HideInInspector] public string logName;
    public int rank;

    [HideInInspector] public PhotonView pv;
    [HideInInspector] public Image image;
    [HideInInspector] public SendChoice choicescript;
    [HideInInspector] public int suitCode;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>();
        image = GetComponent<Image>();
        choicescript = GetComponent<SendChoice>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();

        switch (type)
        {
            case CardType.Coin:
                logName = $"a Coin{rank}";
                suitCode = 0 + rank;
                break;
            case CardType.Bone:
                logName = $"a Bone{rank}";
                suitCode = 10 + rank;
                break;
            case CardType.Weapon:
                logName = $"a Weapon{rank}";
                suitCode = 20 + rank;
                break;
            case CardType.Text:
                logName = $"a Text{rank}";
                suitCode = 30 + rank;
                break;
            case CardType.Treasure:
                logName = "a Treasure";
                break;
            case CardType.CaveIn:
                logName = "a Cave In";
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
