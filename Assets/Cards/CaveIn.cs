using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CaveIn : Card
{
    public override void Setup()
    {
        type = CardType.CaveIn;
        eventtile = true;
    }

    public override IEnumerator OnDiscardEffect(Player player)
    {
        player.eventactivated = true;
        TrashRPC(-1);
        Log.instance.AddTextRPC($"{player.name} trashes {this.logName}.");

        int playertracker = player.playerposition;
        Log.instance.AddTextRPC($"");

        for (int i = 0; i < Manager.instance.playerordergameobject.Count; i++)
        {
            Player nextplayer = Manager.instance.playerordergameobject[playertracker];

            if (PhotonNetwork.IsConnected)
            {
                for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                    Manager.instance.playerordergameobject[j].pv.RPC("WaitForPlayer", RpcTarget.All, nextplayer.name);
            }

            if (PhotonNetwork.IsConnected)
            {
                this.pv.RPC("CaveInEffect", nextplayer.pv.Controller, nextplayer.playerposition, player.playerposition);
                player.waiting = true;
                while (player.waiting)
                    yield return null;
                playertracker = (playertracker == Manager.instance.playerordergameobject.Count - 1) ? 0 : playertracker + 1;

                for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                    Manager.instance.playerordergameobject[j].pv.RPC("WaitDone", RpcTarget.All);
            }
            else
            {
                yield return CaveInEffect(player.playerposition, player.playerposition);
            }
        }
    }

    [PunRPC]
    IEnumerator CaveInEffect(int playerPosition, int requestingPosition)
    {
        Player thisPlayer = Manager.instance.playerordergameobject[playerPosition];
        Player requestingPlayer = Manager.instance.playerordergameobject[requestingPosition];

        if (thisPlayer.listOfPlacard.Count >= 2)
        {
            foreach (Placard placard in thisPlayer.listOfPlacard)
                placard.choicescript.EnableButton(thisPlayer, true);

            thisPlayer.choice = "";
            thisPlayer.chosenPlacard = null;
            Manager.instance.instructions.text = $"Trash one of your Placards.";
            while (thisPlayer.choice == "")
                yield return null;

            Log.instance.AddTextRPC($"{thisPlayer.name} trashes {thisPlayer.chosenPlacard.logName}.");
            thisPlayer.chosenPlacard.TrashRPC(thisPlayer.playerposition);
            foreach (Placard placard in thisPlayer.listOfPlacard)
                placard.choicescript.DisableButton();
        }

        if (PhotonNetwork.IsConnected)
            requestingPlayer.pv.RPC("WaitDone", RpcTarget.All);
    }
}
