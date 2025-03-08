using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgeOfExploration : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool coinrequirement = false;
        bool weaponrequirement = false;
        bool textrequirement = false;

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Coin && (submittedtiles[i].rank == 1 || submittedtiles[i].rank == 2))
                coinrequirement = true;
            if (submittedtiles[i].type == Card.CardType.Weapon && (submittedtiles[i].rank == 3 || submittedtiles[i].rank == 4))
                weaponrequirement = true;
            if (submittedtiles[i].type == Card.CardType.Text && (submittedtiles[i].rank == 5 || submittedtiles[i].rank == 6))
                textrequirement = true;
        }

        return (coinrequirement && weaponrequirement && textrequirement);
    }
}
