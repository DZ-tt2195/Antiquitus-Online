using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IstanbulWas : Placard
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
            List<Card> weapons = new List<Card>();
            int sumofothers = 0;

            for (int i = 0; i < submittedtiles.Count; i++)
            {
                if (submittedtiles[i].type == Card.CardType.Weapon)
                    weapons.Add(submittedtiles[i]);
                else
                    sumofothers += submittedtiles[i].rank;
            }

            int sumofweapons = 0;
            for (int i = 0; i < weapons.Count; i++)
                sumofweapons += (weapons[i].rank);

            return (weapons.Count <= 2 && sumofweapons > sumofothers);
        }
    }

}
