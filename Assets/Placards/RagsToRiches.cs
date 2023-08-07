using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagsToRiches : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool bonerequirement = false;
        bool textrequirement = false;
        bool coinrequirement = false;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Bone && (submittedtiles[i].rank == 1 || submittedtiles[i].rank == 2))
                bonerequirement = true;
            if (submittedtiles[i].type == Card.CardType.Text && (submittedtiles[i].rank == 3 || submittedtiles[i].rank == 4))
                textrequirement = true;
            if (submittedtiles[i].type == Card.CardType.Coin && (submittedtiles[i].rank == 5 || submittedtiles[i].rank == 6))
                coinrequirement = true;
        }

        return (bonerequirement && textrequirement && coinrequirement);
    }
}
