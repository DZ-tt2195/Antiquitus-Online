using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyBox;

public class SendChoice : MonoBehaviour
{
    [ReadOnly] public Player choosingplayer;
    [SerializeField] Button button;
    [ReadOnly] public Image image;

    public TMP_Text textbox;
    [SerializeField] Image border;
    [ReadOnly] public bool enableBorder = false;

    [ReadOnly] public Card card;
    [ReadOnly] public TileData mytile;
    [ReadOnly] public WeaponBox mybox;
    [ReadOnly] public BoneArrow myarrow;
    [ReadOnly] public Placard myPlacard;

    void Awake()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(SendName);

        card = this.GetComponent<Card>();
        mytile = this.GetComponent<TileData>();
        mybox = this.GetComponent<WeaponBox>();
        myarrow = this.GetComponent<BoneArrow>();
        myPlacard = this.GetComponent<Placard>();

        if (mybox != null || myarrow != null)
        {
            border = this.GetComponent<Image>();
        }
        else if (card != null)
        {
            border = this.transform.GetChild(0).GetComponent<Image>();
            border.transform.localPosition = new Vector2(0, 0);
            border.rectTransform.sizeDelta = new Vector2(175, 175);
        }
        else if (myPlacard != null)
        {
            border = this.transform.GetChild(0).GetComponent<Image>();
            border.transform.localPosition = new Vector2(0, 0);
            border.rectTransform.sizeDelta = new Vector2(260, 180);
        }
        else if (mytile != null)
        {
            border = this.transform.GetChild(0).GetComponent<Image>();
            border.transform.localPosition = new Vector2(0, 0);
            border.rectTransform.sizeDelta = new Vector2(175, 175);
        }
    }

    private void FixedUpdate()
    {
        if (border != null && enableBorder)
        {
            border.SetAlpha(Manager.instance.opacity);
        }
        else if (border != null && !enableBorder)
        {
            border.SetAlpha(0);
        }
    }

    public void EnableButton(Player player, bool border)
    {
        this.gameObject.SetActive(true);
        button.interactable = true;
        choosingplayer = player;
        enableBorder = border;
    }

    public void DisableButton()
    {
        button.interactable = false;
        enableBorder = false;
    }

    public void SendName()
    {
        if (textbox != null)
            choosingplayer.choice = textbox.text;
        else
            choosingplayer.choice = this.name;

        choosingplayer.chosencard = card;
        choosingplayer.chosentile = mytile;
        choosingplayer.chosenbox = mybox;
        choosingplayer.chosenarrow = myarrow;
        choosingplayer.chosenPlacard = myPlacard;
    }

}
