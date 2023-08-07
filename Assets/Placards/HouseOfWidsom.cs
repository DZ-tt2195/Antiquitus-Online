using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseOfWidsom : Placard
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
            List<Card> texts = new List<Card>();
            int sumofothers = 0;

            for (int i = 0; i < submittedtiles.Count; i++)
            {
                if (submittedtiles[i].type == Card.CardType.Text)
                    texts.Add(submittedtiles[i]);
                else
                    sumofothers += submittedtiles[i].rank;
            }

            int sumoftexts = 0;
            for (int i = 0; i < texts.Count; i++)
                sumoftexts += (texts[i].rank);

            return (texts.Count <= 2 && sumoftexts > sumofothers);
        }
    }
}
