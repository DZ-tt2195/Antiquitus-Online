using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Coin : Card
{
    // Start is called before the first frame update
    public override void Setup()
    {
        type = CardType.Coin;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        int playertracker = player.playerposition;
        Log.instance.pv.RPC("AddText", RpcTarget.All, $"");

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player nextplayer = Manager.instance.playerordergameobject[playertracker];

            for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                Manager.instance.playerordergameobject[j].pv.RPC("WaitForPlayer", RpcTarget.All, nextplayer.name);

            nextplayer.pv.RPC("Coin", nextplayer.photonplayer, player.photonplayer);
            player.waiting = true;
            while (player.waiting)
                yield return null;
            playertracker = (playertracker == Manager.instance.playerordergameobject.Count - 1) ? 0 : playertracker + 1;
        }

        for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
            Manager.instance.playerordergameobject[j].pv.RPC("WaitDone", RpcTarget.All);
    }

}
