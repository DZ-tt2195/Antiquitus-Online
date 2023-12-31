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
    [ReadOnly] Scrollbar scroll;
    [SerializeField] RectTransform RT;
    [SerializeField] TMP_Text textBoxClone;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        scroll = this.transform.GetChild(1).GetComponent<Scrollbar>();
        instance = this;
    }

    void Update()
    {
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
                AddText($"Test {RT.transform.childCount+1}");
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
        TMP_Text newText = Instantiate(textBoxClone, RT.transform);
        newText.text = text;

        if (RT.transform.childCount >= 28)
        {
            RT.sizeDelta = new Vector2(530, RT.sizeDelta.y+50);

            if (scroll.value <= 0.2f)
            {
                scroll.value = 0;
                RT.transform.localPosition = new Vector3(-30, RT.transform.localPosition.y + 25, 0);
            }
        }
    }
}
