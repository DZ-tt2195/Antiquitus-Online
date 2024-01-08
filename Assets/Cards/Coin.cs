using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Coin : Card
{
    public override void Setup()
    {
        type = CardType.Coin;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        int playertracker = player.playerposition;
        Log.instance.AddTextRPC($"");

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player nextplayer = Manager.instance.playerordergameobject[playertracker];

            if (PhotonNetwork.IsConnected)
            {
                for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                    Manager.instance.playerordergameobject[j].pv.RPC("WaitForPlayer", RpcTarget.All, nextplayer.name);
            }

            if (PhotonNetwork.IsConnected)
            {
                this.pv.RPC("CoinEffect", nextplayer.pv.Controller, nextplayer.playerposition, player.playerposition);
                player.waiting = true;
                while (player.waiting)
                    yield return null;
                playertracker = (playertracker == Manager.instance.playerordergameobject.Count - 1) ? 0 : playertracker + 1;

                for (int j = 0; j < Manager.instance.playerordergameobject.Count; j++)
                    Manager.instance.playerordergameobject[j].pv.RPC("WaitDone", RpcTarget.All);
            }
            else
            {
                yield return CoinEffect(player.playerposition, player.playerposition);
            }
        }
    }

    [PunRPC]
    IEnumerator CoinEffect(int playerPosition, int requestingPosition)
    {
        Player thisPlayer = Manager.instance.playerordergameobject[playerPosition];
        Player requestingPlayer = Manager.instance.playerordergameobject[requestingPosition];

        yield return thisPlayer.ChoosePlacard();
        if (PhotonNetwork.IsConnected)
            requestingPlayer.pv.RPC("WaitDone", RpcTarget.All);
    }
}
