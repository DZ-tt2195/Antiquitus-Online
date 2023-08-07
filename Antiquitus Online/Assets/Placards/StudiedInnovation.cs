using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudiedInnovation : Placard
{
    public override void Setup()
    {
        rep = 1;
    }

    public override bool CanSubmit(Player player, List<Card> submittedtiles)
    {
        bool[][] listofbools = new bool[4][];

        for (int i = 0; i < listofbools.Length; i++)
            listofbools[i] = new bool[6];

        for (int i = 0; i < submittedtiles.Count; i++)
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
            listofbools[typenumber][submittedtiles[i].rank-1] = true;
        }

        for (int i = 0; i < listofbools.Length; i++)
        {
            for (int j = 0; j < listofbools[i].Length-1; j++)
            {
                if (listofbools[i][j] && listofbools[i][j + 1])
                    return true;
            }
        }

        return false;
    }
}
