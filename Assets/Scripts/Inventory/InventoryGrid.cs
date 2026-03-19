using UnityEngine;
public class InventoryGrid
{
    public int width;
    public int height;

    private Item[,] _grid;

    public InventoryGrid(int w, int h)
    {
        width = w;
        height = h;
        _grid = new Item[w, h];
    }
    /// <summary>
    /// Return true if there enough space and all cells under dragged item are not occupied with another item.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <returns></returns>
    public bool CanPlaceHere(Item item, int startX, int startY)
    {
        if (startX < 0 || startY < 0) return false;
        if (startX + item.Width > width) return false;
        if (startY + item.Height > height) return false;

        for (int x = 0; x < item.Width; x++)
        {
            for (int y = 0; y < item.Height; y++)
            {
                if (_grid[startX + x, startY + y] != null)
                    return false;
            }
        }

        return true;
    }

    public void Place(Item item, int startX, int startY)
    {
        for (int x = 0; x < item.Width; x++)
        {
            for (int y = 0; y < item.Height; y++)
            {
                _grid[startX + x, startY + y] = item;
            }
        }

        item.x = startX;
        item.y = startY;

        UIManager.instance.UpdateRect(UIManager.instance.icons[item].GetComponent<RectTransform>(), item.x, item.y, item.Width, item.Height);
    }

    public void Remove(Item item)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] == item) _grid[x, y] = null;
            }
        }
    }

    public Item GetItem(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;

        return _grid[x, y];
    }
}