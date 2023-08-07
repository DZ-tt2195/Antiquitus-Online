using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalvagedBlade : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        return player.cardthisturn.type == Card.CardType.Weapon;
    }

}
