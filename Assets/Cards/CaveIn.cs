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
        pv.RPC("TrashThis", RpcTarget.All, -1);
        Log.instance.pv.RPC("AddText", RpcTarget.All, $"{player.name} trashes {this.logName}.");

        int playertracker = player.playerposition;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player nextplayer = Manager.instance.playerordergameobject[playertracker];
            nextplayer.pv.RPC("TrashPlacard", nextplayer.pv.Controller, player.pv.Controller);
            player.waiting = true;
            while (player.waiting)
                yield return null;
            playertracker = (playertracker == Manager.instance.playerordergameobject.Count - 1) ? 0 : playertracker + 1;
        }
    }
}
