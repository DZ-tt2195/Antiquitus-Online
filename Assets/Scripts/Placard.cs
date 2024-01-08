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

    public void TrashRPC(int playerOrder)
    {
        if (PhotonNetwork.IsConnected)
        {
            pv.RPC("TrashPlacard", RpcTarget.All, playerOrder);
        }
        else
        {
            StartCoroutine(TrashPlacard(playerOrder));
        }
    }

    [PunRPC]
    IEnumerator TrashPlacard(int playerOrder)
    {
        Player player = null;
        try
        {
            player = Manager.instance.playerordergameobject[playerOrder];
            player.listOfPlacard.Remove(this);
        }
        catch { /*do nothing*/}

        float zRot = Random.Range(-45f, 45f);
        this.transform.SetAsLastSibling();

        StartCoroutine(this.MovePlacard(new float[] { this.transform.localPosition.x, this.transform.localPosition.y - 100 }, new float[] { 0, 0, zRot }, 0.3f));
        StartCoroutine(this.FadeAway(0.3f));
        if (player != null) player.SortPlacards();
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

    public void MovePlacardRPC(float[] newPosition, float[] newRotation, float waitTime)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("MovePlacard", RpcTarget.All, newPosition, newRotation, waitTime);
        else
            StartCoroutine(MovePlacard(newPosition, newRotation, waitTime));
    }

    [PunRPC]
    public IEnumerator MovePlacard(float[] newPosition, float[] newRotation, float waitTime)
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
}
