using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidasTouch : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[] coinssubmitted = new bool[6];

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Coin)
                coinssubmitted[submittedtiles[i].rank] = true;
        }

        for (int i = 0; i < coinssubmitted.Length - 2; i++)
        {
            if (coinssubmitted[i] && coinssubmitted[i + 1] && coinssubmitted[i + 2])
                return true;
        }

        return false;
    }
}
