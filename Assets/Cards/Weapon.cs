using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapon : Card
{
    public override void Setup()
    {
        type = CardType.Weapon;
        eventtile = false;
    }

    public override IEnumerator OnTakeEffect(Player player)
    {
        Manager.instance.instructions.text = $"Choose a 2x2 square of tiles in the Site.";
        for (int i = 0; i < Manager.instance.listofboxes.Count; i++)
            Manager.instance.listofboxes[i].choicescript.EnableButton(player, true);

        player.choice = "";
        player.chosencard = null;
        player.chosentile = null;
        player.chosenbox = null;
        player.chosenarrow = null;

        while (player.choice == "")
            yield return null;
        for (int i = 0; i < Manager.instance.listofboxes.Count; i++)
            Manager.instance.listofboxes[i].choicescript.DisableButton();

        List<Card> storecards = new List<Card>();
        WeaponBox storebox = player.chosenbox;

        for (int i = 0; i < player.chosenbox.groupofTiles.Count; i++)
        {
            Card nextCard = player.chosenbox.groupofTiles[i].mycard;
            if (nextCard != null)
            {
                storecards.Add(nextCard);
                player.chosenbox.groupofTiles[i].pv.RPC("NullCard", RpcTarget.All);
            }
        }

        Log.instance.pv.RPC("AddText", RpcTarget.All, $"");

        for (int i = 0; i < storebox.groupofTiles.Count; i++)
        {
            storebox.groupofTiles[i].choicescript.enableBorder = true;
            Manager.instance.instructions.text = $"Put a card on this tile.";

            for (int j = 0; j < storecards.Count; j++)
                Manager.instance.AddCardButton(player, storecards[j], true);

            player.choice = "";
            player.chosencard = null;
            player.chosenbox = null;

            while (player.choice == "")
                yield return null;

            Card nextCard = player.chosencard;
            storecards.Remove(nextCard);
            Manager.instance.ClearButtons();

            Manager.instance.AddCardButton(player, nextCard, false);
            Manager.instance.AddTextButton(player, "Face Up", true);
            Manager.instance.AddTextButton(player, "Face Down", true);
            Manager.instance.instructions.text = $"Face up or down?";

            player.choice = "";
            while (player.choice == "")
                yield return null;

            Manager.instance.ClearButtons();
            storebox.groupofTiles[i].choicescript.enableBorder = false;

            if (player.choice == "Face Up")
            {
                Manager.instance.listoftiles[storebox.groupofTiles[i].position].pv.RPC("NewCard", RpcTarget.All, nextCard.pv.ViewID, true);
                Log.instance.pv.RPC("AddText", RpcTarget.All, $"{player.name} puts {nextCard.logName} into the Site.");
            }
            else
            {
                Manager.instance.listoftiles[storebox.groupofTiles[i].position].pv.RPC("NewCard", RpcTarget.All, nextCard.pv.ViewID, false);
                Log.instance.pv.RPC("AddText", RpcTarget.All, $"{player.name} puts a card into the Site.");
            }
        }

    }
}