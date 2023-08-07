using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneArrow : MonoBehaviour
{
    [HideInInspector] public SendChoice choicescript;
    public List<TileData> groupoftiles = new List<TileData>();
    public enum Direction { up, down, right, left};
    public Direction direction;

    private void Awake()
    {
        choicescript = this.GetComponent<SendChoice>();
    }

    private void Start()
    {
        switch (direction)
        {
            case Direction.up:
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 5]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 10]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 15]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 20]);
                break;
            case Direction.down:
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 5]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 10]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 15]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 20]);
                break;
            case Direction.right:
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 1]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 2]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 3]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position + 4]);
                break;
            case Direction.left:
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 1]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 2]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 3]);
                groupoftiles.Add(Manager.instance.listoftiles[groupoftiles[0].position - 4]);
                break;
        }
    }
}
