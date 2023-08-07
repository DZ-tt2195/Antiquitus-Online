using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnneccessarySurplus : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        return ((player.cardhand.childCount - submittedtiles.Count) >= 3);
    }

}
