using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackDeath : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[] bonessubmitted = new bool[6];

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Bone)
                bonessubmitted[submittedtiles[i].rank] = true;
        }

        for (int i = 0; i<bonessubmitted.Length-2; i++)
        {
            if (bonessubmitted[i] && bonessubmitted[i + 1] && bonessubmitted[i + 2])
                return true;
        }

        return false;
    }
}
