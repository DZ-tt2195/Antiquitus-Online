using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletsOfUruk : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Weapon || submittedtiles[i].type == Card.CardType.Bone)
                return false;
        }
        return true;
    }

}
