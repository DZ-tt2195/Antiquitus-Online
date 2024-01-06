using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TextManager : MonoBehaviour
{
    public static TextManager instance;
    [HideInInspector] public PhotonView pv;
    public int waiting;

    public int[] placardIDs;

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
    }

    public IEnumerator WaitingTime(Player player)
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers > 1)
        {
            placardIDs = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
            waiting = PhotonNetwork.CurrentRoom.MaxPlayers;

            for (int i = 0; i < Manager.instance.playerordergameobject.Count; i++)
            {
                Player nextPlayer = Manager.instance.playerordergameobject[i];
                nextPlayer.pv.RPC("Text", nextPlayer.pv.Controller, player.pv.Controller);
            }

            while (waiting > 0)
            {
                yield return null;
            }

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                Manager.instance.playerordergameobject[i].pv.RPC("RemovePlacard", RpcTarget.All, placardIDs[i]);
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                if (i == 0)
                    Manager.instance.playerordergameobject[i].pv.RPC("SendPlacard", RpcTarget.All, placardIDs[^1]);
                else
                    Manager.instance.playerordergameobject[i].pv.RPC("SendPlacard", RpcTarget.All, placardIDs[i-1]);
            }

            for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                Manager.instance.playerordergameobject[j].pv.RPC("WaitDone", RpcTarget.All);
        }
    }


    [PunRPC]
    public void GetPlacard(int position, int placard)
    {
        placardIDs[position] = placard;
        waiting--;
    }
}
