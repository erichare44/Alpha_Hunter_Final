using System;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryGrid 
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    int rows;
    int columns;
    float cellSize;
    Vector3 origin;

    private GameObject[,] gridCellsArray;


    public InventoryGrid(int width, int height, float cellSize, Vector3 origin)
    { 
        this.columns = width;
        this.rows = height;
        this.cellSize = cellSize;
        this.origin = origin;

        gridCellsArray = new GameObject[columns, rows];
    }


    public bool AutoPlaceWeapon(GameObject weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("AutoPlaceWeapon() failed: weapon is null.");
            return false;
        }

        InGameItem weaponInstance = weapon.GetComponent<InGameItem>();

        if (weaponInstance == null)
        {
            Debug.LogError("AutoPlaceWeapon() failed: WeaponInstance missing on " + weapon.name);
            return false;
        }

        if (weaponInstance.itemInstance == null)
        {
            Debug.LogError("AutoPlaceWeapon() failed: itemInstance missing on " + weapon.name);
        }

        float itemWidth = weaponInstance.itemInstance.width;
        float itemHeight = weaponInstance.itemInstance.height;


        for (int startX = 0; startX <= columns - itemWidth; startX++)
        { 
            for (int startY = 0; startY <= rows - itemHeight; startY++)
            {
                if (CanPlaceItem(startX, startY, itemWidth, itemHeight))
                {
                    PlaceItem(startX, startY, weapon, weaponInstance);
                    Debug.Log("Placed " + weapon.name + " at cell (" + startX + ", " + startY + ")");
                    return true;
                }
            }
        }

        Debug.LogWarning("No Valid Space found for " + weapon.name);
        return false;

    }




    private bool CanPlaceItem(int startX, int startY, float itemWidth, float itemHeight)
    {
        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (gridCellsArray[startX + x, startY + y] != null)
                {
                    Debug.Log("Blocked at: (" + (startX + x) + ", " + (startY + y) + ")");
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceItem(int startX, int startY, GameObject weapon, InGameItem weaponInstance)
    {
        float itemWidth = weaponInstance.itemInstance.GetWidth();
        float itemHeight = weaponInstance.itemInstance.GetHeight();

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                gridCellsArray[startX + x, startY + y] = weapon;
                Debug.Log("Occupying cell: (" + (startX + x) + ", " + (startY + y) + ")");
            }
        }

        weaponInstance.itemInstance.x = startX;
        weaponInstance.itemInstance.y = startY;

        TriggerGridObjectChanged(startX, startY);
    }

    public void RemoveItem(GameObject weapon)
    {
        if (weapon == null)
            return;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (gridCellsArray[x, y] == weapon)
                {
                    gridCellsArray[x, y] = null;
                    TriggerGridObjectChanged(x, y);
                }
            }
        }

        Debug.Log("Removed item from grid: " + weapon.name);
    }


    public bool CanPlaceWeaponAt(GameObject weapon, int startX, int startY)
    {
        if (weapon == null)
            return false;

        InGameItem weaponInstance = weapon.GetComponent<InGameItem>();
        if (weaponInstance == null || weaponInstance.itemInstance == null)
            return false;

        int itemWidth = Mathf.RoundToInt(weaponInstance.itemInstance.GetWidth());
        int itemHeight = Mathf.RoundToInt(weaponInstance.itemInstance.GetHeight());

        if (startX < 0 || startY < 0)
            return false;

        if (startX + itemWidth > columns || startY + itemHeight > rows)
            return false;

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                if (gridCellsArray[startX + x, startY + y] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool PlaceWeaponAt(GameObject weapon, int startX, int startY)
    {
        if (!CanPlaceWeaponAt(weapon, startX, startY))
            return false;

        InGameItem weaponInstance = weapon.GetComponent<InGameItem>();
        int itemWidth = Mathf.RoundToInt(weaponInstance.itemInstance.GetWidth());
        int itemHeight = Mathf.RoundToInt(weaponInstance.itemInstance.GetHeight());

        for (int x = 0; x < itemWidth; x++)
        {
            for (int y = 0; y < itemHeight; y++)
            {
                gridCellsArray[startX + x, startY + y] = weapon;
            }
        }

        weaponInstance.itemInstance.x = startX;
        weaponInstance.itemInstance.y = startY;

        TriggerGridObjectChanged(startX, startY);
        return true;
    }

    public GameObject GetValue(int x, int y)
    {
        if (x < 0 || y < 0 || x >= columns || y >= rows)
            return null;

        return gridCellsArray[x, y];
    }

    public int GetColumns()
    {
        return columns;
    }

    public int GetRows()
    {
        return rows;
    }

    public float GetCellSize()
    {
        return cellSize;
    }


    public void SetValue(int x, int y, GameObject value)
    {
        gridCellsArray[x, y] = value;
    }



    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridValueChanged != null)
        {
            OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

}
