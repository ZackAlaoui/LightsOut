using UnityEngine;
using UnityEngine.Tilemaps;

public class Tilemap3DColliderGenerator : MonoBehaviour
{
    public Tilemap tilemap;

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3Int cellPos = new Vector3Int(x + bounds.x, y + bounds.y, 0);
                    Vector3 worldPos = tilemap.CellToWorld(cellPos);
                    
                    // Offset to center of the tile
                    Vector3 offset = tilemap.cellSize / 2;
                    worldPos += offset;

                    // Create box at tile position
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = worldPos;
                    cube.transform.localScale = tilemap.cellSize;

                    // Optional: organize in hierarchy
                    cube.transform.parent = this.transform;
                }
            }
        }
    }
}
