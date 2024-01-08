using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TextTile : Card
{
    public override void Setup()
    {
        type = CardType.Text;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        Log.instance.AddTextRPC($"");
        yield return TextManager.instance.WaitingTime(player);
    }
}

