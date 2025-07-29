// Attach this script to your tilemap GameObject
using UnityEngine;
using UnityEngine.Tilemaps;

public class Walls : MonoBehaviour
{
    public Tilemap tilemap;

    void Start()
    {
        GameObject wallColliders = new GameObject("WallColliders");
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Vector3Int tilePos = new Vector3Int(x + bounds.x, y + bounds.y, 0);
                    Vector3 worldPos = tilemap.CellToWorld(tilePos);
                    GameObject colliderObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    colliderObj.transform.position = worldPos + tilemap.cellSize / 2 - new Vector3(0.5f, 0f, 0.5f);
                    colliderObj.transform.localScale = tilemap.cellSize * 3f;
                    colliderObj.transform.parent = wallColliders.transform;
                    // Make the collider invisible by removing the renderer
                    //DestroyImmediate(colliderObj.GetComponent<Renderer>());
                }
            }
        }
    }
}
