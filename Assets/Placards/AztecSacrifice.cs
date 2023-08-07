using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AztecSacrifice : Placard
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
            List<Card> bones = new List<Card>();
            int sumofothers = 0;

            for (int i = 0; i<submittedtiles.Count; i++)
            {
                if (submittedtiles[i].type == Card.CardType.Bone)
                    bones.Add(submittedtiles[i]);
                else
                    sumofothers += submittedtiles[i].rank;
            }

            int sumofbones = 0;
            for (int i = 0; i < bones.Count; i++)
                sumofbones += (bones[i].rank);

            return (bones.Count <= 2 && sumofbones > sumofothers);
        }
    }
}
