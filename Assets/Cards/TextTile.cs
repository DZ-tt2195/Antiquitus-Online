using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TextTile : Card
{
    // Start is called before the first frame update
    public override void Setup()
    {
        type = CardType.Text;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        Log.instance.pv.RPC("AddText", RpcTarget.All, $"");
        yield return TextManager.instance.WaitingTime(player);
    }
}

