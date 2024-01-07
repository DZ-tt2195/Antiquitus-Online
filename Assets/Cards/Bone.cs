using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bone : Card
{
    public override void Setup()
    {
        type = CardType.Bone;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        Manager.instance.instructions.text = $"Choose a row / direction of tiles.";
        for (int i = 0; i < Manager.instance.listofarrows.Count; i++)
            Manager.instance.listofarrows[i].choicescript.EnableButton(player, true);

        player.choice = "";
        player.chosencard = null;
        player.chosentile = null;
        player.chosenbox = null;
        player.chosenarrow = null;

        while (player.choice == "")
            yield return null;
        for (int i = 0; i < Manager.instance.listofarrows.Count; i++)
            Manager.instance.listofarrows[i].choicescript.DisableButton();

        BoneArrow myArrow = player.chosenarrow;
        Card finalCard = myArrow.groupoftiles[4].mycard;

        Log.instance.AddTextRPC($"");
        switch (myArrow.direction)
        {
            case BoneArrow.Direction.up:
                Log.instance.AddTextRPC( $"{player.name} shifts column {myArrow.groupoftiles[0].column+1} upwards.");
                break;
            case BoneArrow.Direction.down:
                Log.instance.AddTextRPC( $"{player.name} shifts column {myArrow.groupoftiles[0].column+1} downwards.");
                break;
            case BoneArrow.Direction.left:
                Log.instance.AddTextRPC( $"{player.name} shifts row {myArrow.groupoftiles[0].row+1} to the left.");
                break;
            case BoneArrow.Direction.right:
                Log.instance.AddTextRPC( $"{player.name} shifts row {myArrow.groupoftiles[0].row+1} to the right.");
                break;
        }

        for (int i = 4; i>0; i--)
        {
            TileData currentTile = Manager.instance.listoftiles[myArrow.groupoftiles[i].position];
            TileData previousTile = Manager.instance.listoftiles[myArrow.groupoftiles[i-1].position];
            Debug.Log($"{previousTile.mycard.name} moved");
            currentTile.NewCardRPC(previousTile.mycard, previousTile.mycard.IsPublic(), false);
            yield return new WaitForSeconds(0.05f);
        }

        finalCard.transform.SetParent(GameObject.Find("Canvas").transform);
        finalCard.MoveCardRPC(new float[] { -750, 0 }, new float[] { 0, 0, 0 }, 0.3f);
        Log.instance.AddTextRPC($"{player.name} knocks away {finalCard.logName}.");

        Manager.instance.instructions.text = $"Put a card from your hand in the site.";
        myArrow.groupoftiles[0].choicescript.enableBorder = true;

        for (int i = 0; i < player.listOfHand.Count; i++)
            player.listOfHand[i].choicescript.EnableButton(player, true);
        player.choice = "";
        player.chosencard = null;
        player.chosenarrow = null;
        while (player.choice == "")
            yield return null;

        myArrow.groupoftiles[0].choicescript.enableBorder = false;
        for (int i = 0; i < player.listOfHand.Count; i++)
            player.listOfHand[i].choicescript.DisableButton();
        Manager.instance.ClearButtons();

        player.listOfHand.Remove(player.chosencard);
        myArrow.groupoftiles[0].NewCardRPC(player.chosencard, true, false);
        Log.instance.AddTextRPC($"{player.name} puts {finalCard.logName} into the Site.");

        if (finalCard.eventtile)
        {
            yield return finalCard.OnDiscardEffect(player);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                int[] cardIDs = new int[1];
                cardIDs[0] = finalCard.pv.ViewID;
                player.pv.RPC("SendDraw", RpcTarget.All, cardIDs);
            }
            else
            {
                Card[] gainCard = new Card[1];
                gainCard[0] = finalCard;
                player.AddToHand(gainCard);
            }
        }
    }
}
