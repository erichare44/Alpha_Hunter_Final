using UnityEngine;
using System;

[System.Serializable]
public class ItemInstance 
{

    public ItemDefinition definition;
    public int x, y;
    public float width, height;
    public Sprite icon;
    public int stackCount = 1;
    public bool rotated;


    public ItemInstance(ItemDefinition definition, int x, int y, float width, float height, Sprite icon, int stackCount, bool rotated)
    {
        this.definition = definition;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.icon = icon;
        this.stackCount = stackCount;
        this.rotated = rotated;
    }

    public float GetWidth()
    {
        return width;
    }


    public float GetHeight()
    {
        return height;
    }


    public float GetWidth(bool useRotated)
    {
        if (definition == null) 
            return 0;
        if (useRotated)
        {
            return definition.height;
        }
        else 
            return definition.width;
    }

    public int GetHeight(bool useRotated)
    {
        if (definition == null) 
            return 0;
        if (useRotated)
        {
            return definition.width;
        }
        else
            return definition.height;
    }

}
