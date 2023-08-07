using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapon : Card
{
    // Start is called before the first frame update
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

        for (int i = 0; i < player.chosenbox.groupoftiles.Count; i++)
        {
            storecards.Add(player.chosenbox.groupoftiles[i].mycard);
            player.chosenbox.groupoftiles[i].pv.RPC("NullCard", RpcTarget.All);
        }

        for (int i = 0; i < storebox.groupoftiles.Count; i++)
        {
            storebox.groupoftiles[i].choicescript.enableBorder = true;
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
            storebox.groupoftiles[i].choicescript.enableBorder = false;

            Manager.instance.listoftiles[storebox.groupoftiles[i].position].pv.RPC
            ("NewCard", RpcTarget.All, nextCard.pv.ViewID, player.choice == "Face Up");
        }

    }
}