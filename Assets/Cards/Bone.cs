using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bone : Card
{
    // Start is called before the first frame update
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

        for (int i = 4; i>0; i--)
        {
            TileData currentTile = Manager.instance.listoftiles[myArrow.groupoftiles[i].position];
            TileData previousTile = Manager.instance.listoftiles[myArrow.groupoftiles[i-1].position];
            currentTile.pv.RPC("NewCard", RpcTarget.All, previousTile.mycard.pv.ViewID, previousTile.faceup);
            yield return new WaitForSeconds(0.05f);
        }
        myArrow.groupoftiles[0].pv.RPC("NullCard", RpcTarget.All);
        Manager.instance.AddCardButton(player, finalCard, false);

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

        myArrow.groupoftiles[0].pv.RPC("NewCard", RpcTarget.All, player.chosencard.pv.ViewID, true);
        if (finalCard.eventtile)
        {
            yield return finalCard.OnDiscardEffect(player);
        }
        else
        {
            int[] cardIDs = new int[1];
            cardIDs[0] = finalCard.pv.ViewID;
            player.pv.RPC("SendDraw", RpcTarget.All, cardIDs);
        }
    }
}
