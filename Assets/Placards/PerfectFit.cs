using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectFit : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        return ((player.listOfHand.Count - submittedtiles.Count) == 0);
    }
}
