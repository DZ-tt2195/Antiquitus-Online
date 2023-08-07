using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RivalNations : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[][] listofbools = new bool[6][];

        for (int i = 0; i < listofbools.Length; i++)
            listofbools[i] = new bool[4];

        for (int i = 0; i<submittedtiles.Count; i++)
        {
            int typenumber = -1;
            switch (submittedtiles[i].type)
            {
                case Card.CardType.Coin:
                    typenumber = 0;
                    break;
                case Card.CardType.Bone:
                    typenumber = 1;
                    break;
                case Card.CardType.Weapon:
                    typenumber = 2;
                    break;
                case Card.CardType.Text:
                    typenumber = 3;
                    break;
            }
            listofbools[submittedtiles[i].rank-1][typenumber] = true;
        }

        for (int i = 0; i<listofbools.Length; i++)
        {
            int counter = 0;
            for (int j = 0; j<listofbools[i].Length; j++)
            {
                counter += listofbools[i][j] ? 1 : 0;
            }
            if (counter >= 2)
                return true;
        }
        return false;
    }
}
