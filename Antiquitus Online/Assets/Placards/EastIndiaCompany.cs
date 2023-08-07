using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EastIndiaCompany : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        int weapons = 0;
        int coins = 0;

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Weapon && (submittedtiles[i].rank == 5 || submittedtiles[i].rank == 6))
                weapons++;
            if (submittedtiles[i].type == Card.CardType.Coin && (submittedtiles[i].rank == 1 || submittedtiles[i].rank == 2))
                coins++;
        }

        return (weapons >= 2 && coins >= 2);
    }
}
