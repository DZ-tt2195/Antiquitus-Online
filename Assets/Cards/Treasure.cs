using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Treasure : Card
{
    // Start is called before the first frame update
    public override void Setup()
    {
        type = CardType.Treasure;
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
            nextplayer.pv.RPC("RequestDraw", RpcTarget.MasterClient, 1);
            yield return new WaitForSeconds(0.5f);
            playertracker = (playertracker == Manager.instance.playerordergameobject.Count - 1) ? 0 : playertracker + 1;
        }
    }

}
