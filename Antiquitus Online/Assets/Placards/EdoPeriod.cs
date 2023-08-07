using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdoPeriod : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool hasweapon = false;
        bool hastext = false;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Weapon)
                hasweapon = true;
            if (submittedtiles[i].type == Card.CardType.Text)
                hastext = true;
            if (submittedtiles[i].rank == 6 || submittedtiles[i].rank <= 2)
                return false;
        }

        return (hasweapon && hastext);
    }
}
