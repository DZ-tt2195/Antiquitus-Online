using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        yield return TextManager.instance.WaitingTime(player);
    }
}

