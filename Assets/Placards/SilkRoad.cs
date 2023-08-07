using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilkRoad : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        if (submittedtiles.Count < 4)
            return false;

        else
        {
            List<Card> coins = new List<Card>();
            int sumofothers = 0;

            for (int i = 0; i < submittedtiles.Count; i++)
            {
                if (submittedtiles[i].type == Card.CardType.Coin)
                    coins.Add(submittedtiles[i]);
                else
                    sumofothers += submittedtiles[i].rank;
            }

            int sumofcoins = 0;
            for (int i = 0; i < coins.Count; i++)
                sumofcoins += (coins[i].rank);

            return (coins.Count <= 2 && sumofcoins > sumofothers);
        }
    }
}
