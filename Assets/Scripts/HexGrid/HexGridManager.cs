using UnityEngine;
using System.Collections.Generic;

public class HexGridManager : MonoBehaviour
{
    public HexTile hexTilePrefab;
    public Transform tilesParent;

    public int width = 10; // q range
    public int height = 10; // r range

    public float hexSize = 1f; // "radius" in word units

    private readonly Dictionary<HexCoords, HexTile> tiles = 
        new Dictionary<HexCoords, HexTile>();

    // Axial neighbor directions for pointy-top layout
    public static readonly HexCoords[] NeighborDirs = new HexCoords[]
    {
        new HexCoords(+1, 0),
        new HexCoords(+1, -1),
        new HexCoords(0, -1),
        new HexCoords(-1, 0),
        new HexCoords(-1, +1),
        new HexCoords(0, +1),
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid(); 
    }

    public void GenerateGrid()
    {
        float size = GetHexSizeFromPrefab();

        if (hexTilePrefab == null)
        {
            Debug.LogError("HexGridManager: hexTilePrefab not set");
            return;
        }

        if (tilesParent == null)
            tilesParent = this.transform;

        //Clear old children if regenerate
        foreach (Transform child in tilesParent)
            Destroy(child.gameObject);

        tiles.Clear();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                HexCoords axial = OddRToAxial(col, row);
                Vector3 worldPos = OddRToWorld(col, row, size);

                HexTile tile = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, tilesParent);
                tile.coords = axial;
                tile.name = $"Hex {axial} (col={col},row={row})";

                tiles[axial] = tile;
            }
        }
        CenterCameraOnGrid(Camera.main, 1.2f);
    }

    public static Vector3 AxialToWorld(HexCoords c, float size)
    {
        float x = size * Mathf.Sqrt(3f) * (c.q + c.r * 0.5f);
        float y = size * 1.5f * c.r;
        return new Vector3(x, y, 0f);
    }

    public List<HexTile> GetNeighbors(HexCoords c)
    {
        var result = new List<HexTile>(6);
        foreach (var dir in NeighborDirs)
        {
            HexCoords n = new HexCoords(c.q + dir.q, c.r + dir.r);
            if (tiles.TryGetValue(n, out HexTile t))
                result.Add(t);
        }
        return result;
    }

    public bool AreAdjacent(HexCoords a, HexCoords b)
    {
        foreach (var dir in NeighborDirs)
        {
            if (a.q + dir.q == b.q && a.r + dir.r == b.r)
                return true;
        }
        return false;
    }

    private float GetHexSizeFromPrefab()
    {
        var sr = hexTilePrefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            return hexSize; // fallback to inspector value

        // For a pointy-top hex sprite, "radius" is half its height in world units
        float radius = sr.sprite.bounds.extents.y;

        // account for prefab scale
        radius *= hexTilePrefab.transform.lossyScale.y;

        return radius;
    }

    // ODD-R offset (pointy-top) -> axial
    public static HexCoords OddRToAxial(int col, int row)
    {
        int q = col - (row - (row & 1)) / 2;
        int r = row;
        return new HexCoords(q, r);
    }

    // ODD-R offset -> world position
    public static Vector3 OddRToWorld(int col, int row, float size)
    {
        float x = size * Mathf.Sqrt(3f) * (col + 0.5f * (row & 1));
        float y = size * 1.5f * row;
        return new Vector3(x, y, 0f);
    }

    public void CenterCameraOnGrid(Camera cam, float padding = 1.2f)
    {
        if (cam == null) return;

        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        foreach (Transform child in tilesParent)
        {
            Vector3 p = child.position;
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, cam.transform.position.z);
        cam.transform.position = center;

        if (cam.orthographic)
        {
            float gridWidth = (maxX - minX) * padding;
            float gridHeight = (maxY - minY) * padding;

            float sizeByHeight = gridHeight / 2f;
            float sizeByWidth = gridWidth / (2f * cam.aspect);

            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
        }
    }


}
