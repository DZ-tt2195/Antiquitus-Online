using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renaissance : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool hascoin = false;
        bool hastext = false;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Coin)
                hascoin = true;
            if (submittedtiles[i].type == Card.CardType.Text)
                hastext = true;
            if (submittedtiles[i].rank == 1 || submittedtiles[i].rank >= 5)
                return false;
        }

        return (hascoin && hastext);
    }
}
