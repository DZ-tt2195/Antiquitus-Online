using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpicOfGilgamesh : Placard
{
    public override void Setup()
    {
        rep = 4;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[] textssubmitted = new bool[6];

        for (int i = 0; i < submittedtiles.Count; i++)
        {
            if (submittedtiles[i].type == Card.CardType.Text)
                textssubmitted[submittedtiles[i].rank] = true;
        }

        for (int i = 0; i < textssubmitted.Length - 2; i++)
        {
            if (textssubmitted[i] && textssubmitted[i + 1] && textssubmitted[i + 2])
                return true;
        }

        return false;
    }
}
