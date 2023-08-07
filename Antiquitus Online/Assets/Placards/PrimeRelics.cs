using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimeRelics : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool hastwo = false;
        bool hasthree = false;
        bool hasfive = false;

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].rank == 2)
                hastwo = true;
            if (submittedtiles[i].rank == 3)
                hasthree = true;
            if (submittedtiles[i].rank == 5)
                hasfive = true;
        }

        return (hastwo && hasthree && hasfive);
    }
}
