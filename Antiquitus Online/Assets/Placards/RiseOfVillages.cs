using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiseOfVillages : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        int oneandtwo = 0;
        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].rank == 1 || submittedtiles[i].rank == 2)
                oneandtwo++;
        }
        return oneandtwo >= 2;
    }

}
