using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhengYisFleet : Placard
{
    public override void Setup()
    {
        rep = 2;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool submissionrequirement = true;
        bool weaponrequirement = false;
        bool coinrequirement = false;

        for (int i = 0; i<Manager.instance.playerordergameobject.Count; i++)
        {
            if (i != player.playerposition &&
            Manager.instance.playerordergameobject[i].personalsubmissions <= player.personalsubmissions)
                submissionrequirement = false;
        }

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Weapon)
                weaponrequirement = true;
            if (submittedtiles[i].type == Card.CardType.Coin)
                coinrequirement = true;
        }

        return (submissionrequirement && weaponrequirement && coinrequirement);
    }
}
