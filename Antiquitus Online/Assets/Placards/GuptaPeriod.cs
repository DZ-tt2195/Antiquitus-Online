using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuptaPeriod : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool hasbone = false;
        bool hasweapon = false;

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Bone)
                hasbone = true;
            if (submittedtiles[i].type == Card.CardType.Weapon)
                hasweapon = true;
            if (submittedtiles[i].rank >= 4)
                return false;
        }

        return (hasbone && hasweapon);
    }
}
