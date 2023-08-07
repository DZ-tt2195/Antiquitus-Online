using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OddsAndEvens : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[] rankssubmitted = new bool[6];

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            rankssubmitted[submittedtiles[i].rank] = true;
        }

        int odds = 0;
        int evens = 0;

        for (int i = 0; i<rankssubmitted.Length; i++)
        {
            if (i % 2 == 0)
                odds += (rankssubmitted[i]) ? 1 : 0;
            else
                evens += (rankssubmitted[i]) ? 1 : 0;
        }

        return ((odds >= 2) && (evens >= 2));
    }
}
