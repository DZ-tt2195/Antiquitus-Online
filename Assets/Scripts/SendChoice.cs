using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SendChoice : MonoBehaviour
{
    [HideInInspector] public Player choosingplayer;
    public Button button;
    public Image image;

    public TMP_Text textbox;
    public Image border;
    [HideInInspector] public bool enableBorder;

    [HideInInspector] public Card card;
    [HideInInspector] public TileData mytile;
    [HideInInspector] public WeaponBox mybox;
    [HideInInspector] public BoneArrow myarrow;
    [HideInInspector] public Placard myPlacard;

    // Start is called before the first frame update
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
            border.rectTransform.sizeDelta = new Vector2(120, 120);
        }
        else if (myPlacard != null)
        {
            border = this.transform.GetChild(0).GetComponent<Image>();
            border.transform.localPosition = new Vector2(0, 0);
            border.rectTransform.sizeDelta = new Vector2(140, 100);
        }
        else if (mytile != null)
        {
            border = this.transform.GetChild(0).GetComponent<Image>();
            border.transform.localPosition = new Vector2(0, 0);
            border.rectTransform.sizeDelta = new Vector2(280, 280);
        }
    }

    private void FixedUpdate()
    {
        if (border != null && enableBorder)
        {
            border.color = new Color(1, 1, 1, Manager.instance.opacity);
        }
        else if (border != null && !enableBorder)
        {
            border.color = new Color(1, 1, 1, 0);
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
