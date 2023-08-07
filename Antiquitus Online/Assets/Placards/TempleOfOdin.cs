using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleOfOdin : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        int bones = 0;
        int texts = 0;
        int total = 0;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            total += submittedtiles[i].rank;
            if (submittedtiles[i].type == Card.CardType.Bone)
                bones++;
            if (submittedtiles[i].type == Card.CardType.Text)
                texts++;
        }

        return (bones >= 2 && texts >= 1 && total == 10);
    }
}
