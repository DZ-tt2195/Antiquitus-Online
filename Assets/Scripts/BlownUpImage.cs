using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;

public class BlownUpImage : MonoBehaviour
{
    public static BlownUpImage instance;
    public GameObject mainObject;
    [SerializeField] GameObject reminderSheet;
    [SerializeField] Image card;
    [SerializeField] Image placard; 

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            mainObject.SetActive(false);
    }

    public void NewImage(Image image, bool isCard)
    {
        mainObject.transform.SetAsLastSibling();
        mainObject.SetActive(true);

        this.card.sprite = image.sprite;
        this.card.gameObject.SetActive(isCard);

        this.reminderSheet.SetActive(isCard);

        this.placard.sprite = image.sprite;
        this.placard.gameObject.SetActive(!isCard);
    }
}
