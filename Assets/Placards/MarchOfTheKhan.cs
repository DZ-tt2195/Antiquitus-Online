using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchOfTheKhan : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[] weaponssubmitted = new bool[6];

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Weapon)
                weaponssubmitted[submittedtiles[i].rank] = true;
        }

        for (int i = 0; i < weaponssubmitted.Length - 2; i++)
        {
            if (weaponssubmitted[i] && weaponssubmitted[i + 1] && weaponssubmitted[i + 2])
                return true;
        }

        return false;
    }
}
