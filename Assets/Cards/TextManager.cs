using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MyBox;

public class TextManager : MonoBehaviour
{
    public static TextManager instance;
    [ReadOnly] PhotonView pv;
    [ReadOnly] int waiting;
    [ReadOnly] int[] passPlacards;

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
    }

    public IEnumerator WaitingTime(Player thisPlayer)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom.MaxPlayers > 1)
        {
            passPlacards = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
            waiting = PhotonNetwork.CurrentRoom.MaxPlayers;

            for (int i = 0; i < Manager.instance.playerordergameobject.Count; i++)
            {
                Player nextPlayer = Manager.instance.playerordergameobject[i];
                this.pv.RPC("Text", nextPlayer.pv.Controller, nextPlayer.playerposition, thisPlayer.playerposition);
            }

            while (waiting > 0)
            {
                yield return null;
            }

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                Manager.instance.playerordergameobject[i].pv.RPC("RemovePlacard", RpcTarget.All, passPlacards[i]);
            }

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
            {
                if (i == 0)
                    Manager.instance.playerordergameobject[i].pv.RPC("ReceivePlacard", RpcTarget.All, passPlacards[^1]);
                else
                    Manager.instance.playerordergameobject[i].pv.RPC("ReceivePlacard", RpcTarget.All, passPlacards[i-1]);
            }

            for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                Manager.instance.playerordergameobject[j].pv.RPC("WaitDone", RpcTarget.All);
        }
        else
        {
            Log.instance.AddTextRPC("There's no one to pass to.");
        }
    }

    [PunRPC]
    IEnumerator Text(int playerPosition, int requestingPosition)
    {
        Player thisPlayer = Manager.instance.playerordergameobject[playerPosition];
        Player requestingPlayer = Manager.instance.playerordergameobject[requestingPosition];

        if (thisPlayer.listOfPlacard.Count == 1)
        {
            this.pv.RPC("GetPlacard", requestingPlayer.pv.Controller, thisPlayer.playerposition, thisPlayer.listOfPlacard[0].pv.ViewID);
        }
        else
        {
            for (int i = 0; i < thisPlayer.listOfPlacard.Count; i++)
                thisPlayer.listOfPlacard[i].choicescript.EnableButton(thisPlayer, true);

            thisPlayer.choice = "";
            thisPlayer.chosenPlacard = null;
            Manager.instance.instructions.text = $"Pass one of your Placards to {thisPlayer.GetPreviousPlayer().name}.";
            while (thisPlayer.choice == "")
                yield return null;

            for (int i = 0; i < thisPlayer.listOfPlacard.Count; i++)
                thisPlayer.listOfPlacard[i].choicescript.DisableButton();

            this.pv.RPC("GetPlacard", requestingPlayer.pv.Controller, thisPlayer.playerposition, thisPlayer.chosenPlacard.pv.ViewID);
        }
        Manager.instance.instructions.text = $"Waiting...";
    }

    [PunRPC]
    void GetPlacard(int position, int placard)
    {
        passPlacards[position] = placard;
        waiting--;
    }
}
