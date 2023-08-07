using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisCourtOath : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool hasbone = false;
        bool hastext = false;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Bone)
                hasbone = true;
            if (submittedtiles[i].type == Card.CardType.Text)
                hastext = true;
            if (submittedtiles[i].rank <= 3)
                return false;
        }

        return (hasbone && hastext);
    }
}
