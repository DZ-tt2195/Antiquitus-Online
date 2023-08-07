using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [HideInInspector]public SendChoice choicescript;
    public List<TileData> groupoftiles = new List<TileData>();

    private void Awake()
    {
        choicescript = this.GetComponent<SendChoice>();
    }

}
