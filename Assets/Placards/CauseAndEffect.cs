using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauseAndEffect : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        return player.eventactivated;
    }
}
