using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiseOfEmpires : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        int fiveandsix = 0;
        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].rank == 5 || submittedtiles[i].rank == 6)
                fiveandsix++;
        }
        return fiveandsix >= 2;
    }
}
