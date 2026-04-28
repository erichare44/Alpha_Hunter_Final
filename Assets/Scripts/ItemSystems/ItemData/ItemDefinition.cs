using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Inventory/Item Definition")]

public class ItemDefinition : ScriptableObject
{
    [Header("Basic")]
    public string itemID;
    public string itemName;

    [Header("Grid Size")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;
    public bool canRotate = true;

    [Header("Stacks")]
    [Min(1)] public int maxStack = 1;
}
