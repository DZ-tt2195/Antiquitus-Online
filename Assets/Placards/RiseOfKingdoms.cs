using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiseOfKingdoms : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        int threeandfour = 0;
        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].rank == 3 || submittedtiles[i].rank == 4)
                threeandfour++;
        }
        return threeandfour >= 2;
    }
}
