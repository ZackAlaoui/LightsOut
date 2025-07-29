using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class BuildPlane : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildNavMeshPlane();
    }

    void BuildNavMeshPlane()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("BuildPlane script requires a Tilemap component on the same GameObject.");
            return;
        }

        var grid = tilemap.layoutGrid;
        if (grid == null)
        {
            grid = tilemap.GetComponentInParent<Grid>();
        }
        if (grid == null)
        {
            Debug.LogError("BuildPlane could not find a Grid component.");
            return;
        }

        var mesh = new Mesh();
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();

        BoundsInt bounds = tilemap.cellBounds;
        int vertexIndex = 0;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                var cellPosition = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cellPosition))
                {
                    var worldPos = grid.CellToWorld(cellPosition);

                    vertices.Add(worldPos);
                    vertices.Add(worldPos + new Vector3(grid.cellSize.x, 0, 0));
                    vertices.Add(worldPos + new Vector3(0, grid.cellSize.y, 0));
                    vertices.Add(worldPos + new Vector3(grid.cellSize.x, grid.cellSize.y, 0));

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);

                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 3);
                    triangles.Add(vertexIndex + 1);

                    vertexIndex += 4;
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (meshRenderer.sharedMaterial == null)
        {
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }

        NavMeshSurface navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        }
        
        // Optionally build the NavMesh at runtime
        navMeshSurface.BuildNavMesh();
    }
}
