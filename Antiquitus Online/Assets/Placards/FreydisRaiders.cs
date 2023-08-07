using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreydisRaiders : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Coin || submittedtiles[i].type == Card.CardType.Text)
                return false;
        }
        return true;
    }

}
