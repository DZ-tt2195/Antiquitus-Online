using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [HideInInspector] public SendChoice choicescript;
    [HideInInspector] public List<TileData> groupofTiles = new List<TileData>();
    public int firstTile;

    private void Awake()
    {
        choicescript = this.GetComponent<SendChoice>();
    }

    private void Start()
    {
        TileData tileOne = Manager.instance.listoftiles[firstTile];

        groupofTiles.Add(Manager.instance.FindTile(tileOne.row, tileOne.column));
        groupofTiles.Add(Manager.instance.FindTile(tileOne.row, tileOne.column+1));
        groupofTiles.Add(Manager.instance.FindTile(tileOne.row+1, tileOne.column));
        groupofTiles.Add(Manager.instance.FindTile(tileOne.row+1, tileOne.column+1));
    }
}
