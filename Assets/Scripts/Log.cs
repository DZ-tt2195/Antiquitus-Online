using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using MyBox;

public class Log : MonoBehaviour
{
    public static Log instance;
    [ReadOnly] public PhotonView pv;
    [ReadOnly] TMP_Text textBox;
    [ReadOnly] RectTransform textRT;
    [ReadOnly] Scrollbar scroll;
    int linesOfText = 0;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        textBox = this.transform.GetChild(0).GetComponent<TMP_Text>();
        textRT = textBox.GetComponent<RectTransform>();
        scroll = this.transform.GetChild(1).GetComponent<Scrollbar>();
        instance = this;
        textBox.text = "";
    }

    void Update()
    {
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
                AddText($"Test {linesOfText}");
        #endif
    }

    public void AddTextSecret(string text)
    {
        if (this.pv.IsMine)
        {
            if (PhotonNetwork.IsConnected)
                pv.RPC("AddText", RpcTarget.All, text);
            else
                AddText(text);
        }
    }

    public void AddTextRPC(string text)
    {
        if (PhotonNetwork.IsConnected)
            pv.RPC("AddText", RpcTarget.All, text);
        else
            AddText(text);
    }

    [PunRPC]
    void AddText(string text)
    {
        linesOfText++;
        textBox.text += text + "\n";

        if (linesOfText >= 31)
        {
            textRT.sizeDelta = new Vector2(510, textRT.sizeDelta.y+34.5f);

            if (scroll.value <= 0.2f)
            {
                textRT.localPosition = new Vector2(-40, textRT.localPosition.y + 25);
                scroll.value = 0;
            }
        }
    }
}
