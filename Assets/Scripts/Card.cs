using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;
using MyBox;

public class Card : MonoBehaviour
{
    public enum CardType { Coin, Bone, Weapon, Text, Treasure, CaveIn };
    [ReadOnly] public CardType type;
    [ReadOnly] public bool eventtile;
    [ReadOnly] public string logName;
    public int rank;

    [ReadOnly] public PhotonView pv;
    [ReadOnly] public Image image;
    [ReadOnly] public Sprite originalSprite;
    [ReadOnly] public SendChoice choicescript;
    [ReadOnly] public int suitCode;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>();
        image = GetComponent<Image>();
        choicescript = GetComponent<SendChoice>();
        originalSprite = image.sprite;
    }

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

    public void TrashRPC(int playerOrder)
    {
        if (PhotonNetwork.IsConnected)
        {
            pv.RPC("TrashCard", RpcTarget.All, playerOrder);
        }
        else
        {
            StartCoroutine(TrashCard(playerOrder));
        }
    }

    [PunRPC]
    IEnumerator TrashCard(int playerOrder)
    {
        Player player = null;
        try
        {
            player = Manager.instance.playerordergameobject[playerOrder];
            player.listOfHand.Remove(this);
        }
        catch { /*do nothing*/}

        float zRot = Random.Range(-45f, 45f);
        this.transform.SetAsLastSibling();

        MoveCardRPC(new float[] { this.transform.localPosition.x, this.transform.localPosition.y-100 }, new float[] { 0, 0, zRot }, 0.3f);
        StartCoroutine(this.FadeAway(0.3f));
        if (player != null) player.SortHand();
        yield return new WaitForSeconds(0.3f);

        this.transform.SetParent(Manager.instance.trash);
        this.transform.localPosition = new Vector2(-1000, 0);
    }

    protected IEnumerator FadeAway(float totalTime)
    {
        float elapsedTime = 0;
        while (elapsedTime < totalTime)
        {
            this.image.SetAlpha(1f - (elapsedTime / totalTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.image.SetAlpha(0);
        StartCoroutine(Unfade());
    }

    IEnumerator Unfade()
    {
        yield return new WaitForSeconds(2f);
        image.SetAlpha(0);
    }

    public void MoveCardRPC(float[] newPosition, float[] newRotation, float waitTime)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("MoveCard", RpcTarget.All, newPosition, newRotation, waitTime);
        else
            StartCoroutine(MoveCard(newPosition, newRotation, waitTime));
    }

    [PunRPC]
    public IEnumerator MoveCard(float[] newPosition, float[] newRotation, float waitTime)
    {
        float elapsedTime = 0;
        Vector2 originalPos = this.transform.localPosition;
        Vector3 originalRot = this.transform.localEulerAngles;

        Vector2 newPos = new Vector2(newPosition[0], newPosition[1]);
        Vector3 newRot = new Vector3(newRotation[0], newRotation[1], newRotation[2]);

        while (elapsedTime < waitTime)
        {
            this.transform.localPosition = Vector2.Lerp(originalPos, newPos, elapsedTime / waitTime);
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localPosition = newPos;
        this.transform.localEulerAngles = newRot;
    }

    public IEnumerator FlipCard(float totalTime, Sprite newSprite)
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
        float elapsedTime = 0f;

        Vector3 originalRot = this.transform.localEulerAngles;
        Vector3 newRot = new(0, 90, 0);

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.sprite = newSprite;
        elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localEulerAngles = originalRot;
    }

    public bool IsPublic()
    {
        return (this.image.sprite == originalSprite);
    }
}
