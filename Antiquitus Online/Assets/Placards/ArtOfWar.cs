using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtOfWar : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool textrequirement = false;
        bool bonerequirement = false;
        bool weaponrequirement = false;

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Text && (submittedtiles[i].rank == 1 || submittedtiles[i].rank == 2))
                textrequirement = true;
            if (submittedtiles[i].type == Card.CardType.Bone && (submittedtiles[i].rank == 3 || submittedtiles[i].rank == 4))
                bonerequirement = true;
            if (submittedtiles[i].type == Card.CardType.Weapon && (submittedtiles[i].rank == 5 || submittedtiles[i].rank == 6))
                weaponrequirement = true;
        }

        return (textrequirement && bonerequirement && weaponrequirement);
    }

}
