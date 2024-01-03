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
    public SubmissionDepiction SD;
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

    [PunRPC]
    public void MadeSubmission(string total, int[] placardIDs, int[] cardIDs)
    {
        SubmissionDepiction newSD = Instantiate(SD, depiction.transform);
        newSD.plusPoints.text = total;
        for (int i = 0; i<placardIDs.Length; i++)
            PhotonView.Find(placardIDs[i]).transform.SetParent(newSD.placards);
        for (int i = 0; i<cardIDs.Length; i++)
            PhotonView.Find(cardIDs[i]).transform.SetParent(newSD.cards);
    }
}
