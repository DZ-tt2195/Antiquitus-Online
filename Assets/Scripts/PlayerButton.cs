using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;
using MyBox;

public class PlayerButton : MonoBehaviour, IPointerClickHandler
{
    [ReadOnly] public Image image;
    [ReadOnly] public TMP_Text textbox;
    [ReadOnly] public Button button;
    [ReadOnly] public PhotonView pv;
    [SerializeField] SubmissionDepiction SD;
    [ReadOnly] public GameObject depiction;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        textbox = this.transform.GetChild(0).GetComponent<TMP_Text>();
        pv = GetComponent<PhotonView>();
        depiction = this.transform.GetChild(1).gameObject;
    }

    private void Start()
    {
        depiction.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            depiction.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            depiction.SetActive(true);
        }
    }

    public void SubmissionRPC(string total, Placard[] placards, Card[] cards)
    {
        if (PhotonNetwork.IsConnected)
        {
            int[] placardIDs = new int[placards.Length];
            int[] cardIDs = new int[cards.Length];

            for (int i = 0; i < placards.Length; i++)
                placardIDs[i] = placards[i].pv.ViewID;
            for (int i = 0; i < cardIDs.Length; i++)
                cardIDs[i] = cards[i].pv.ViewID;

            pv.RPC("MadeSubmission", RpcTarget.All, total, placardIDs, cardIDs);
        }
        else
        {
            MadeSubmission(total, placards, cards);
        }
    }

    void MadeSubmission(string total, Placard[] placards, Card[] cards)
    {
        SubmissionDepiction newSD = Instantiate(SD, depiction.transform);
        newSD.plusPoints.text = total;

        foreach (Placard next in placards)
        {
            next.transform.SetParent(newSD.placards);
            next.transform.localEulerAngles = new Vector3(0, 0, 0);
            next.image.SetAlpha(1);
        }
        foreach (Card next in cards)
        {
            next.transform.SetParent(newSD.cards);
            next.image.SetAlpha(1);
            next.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    [PunRPC]
    void MadeSubmission(string total, int[] placardIDs, int[] cardIDs)
    {
        Placard[] listOfPlacards = new Placard[placardIDs.Length];
        Card[] listOfCards = new Card[cardIDs.Length];

        for (int i = 0; i < placardIDs.Length; i++)
            listOfPlacards[i] = PhotonView.Find(placardIDs[i]).GetComponent<Placard>();
        for (int i = 0; i < cardIDs.Length; i++)
            listOfCards[i] = PhotonView.Find(cardIDs[i]).GetComponent<Card>();

        MadeSubmission(total, listOfPlacards, listOfCards);
    }
}
