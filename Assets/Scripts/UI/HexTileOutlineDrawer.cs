using UnityEngine;
using System.Collections.Generic;

public class HexTileOutlineDrawer : MonoBehaviour
{
    // Where the tiles live
    public Transform tilesRoot;

    // Hex settings
    public bool pointyTop = true;
    public float radius = 0.5f;
    public Vector2 centerOffset = Vector2.zero;

    // Line settings
    public Color lineColor = Color.gray;
    public float lineWidth = 0.02f;
    public Material lineMaterial;
    public string sortingLayerName = "Default";
    public int sortingOrder = 1;

    // Lifecycle
    public bool drawOnStart = true;
    public bool clearBeforeRedraw = true;

    private readonly List<GameObject> spawned = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (drawOnStart)
            Redraw();
    }

    public void Redraw()
    {
        if (tilesRoot == null)
        {
            Debug.LogError("GeneratedHexOutlineDrawer: tilesRoot is not assigned");
            return;
        }

        if (clearBeforeRedraw)
            ClearOutlines();

        // Outline every child tile under tilesRoot
        for (int i = 0; i < tilesRoot.childCount; i++)
        {
            Transform tile = tilesRoot.GetChild(i);
            DrawOutlinesForTile(tile);
        }
    }

    public void ClearOutlines()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();
    }

    private void DrawOutlinesForTile(Transform tile)
    {
        Vector3 center = tile.position + (Vector3)centerOffset;

        GameObject go = new GameObject($"HexOutline_{tile.name}");
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.position = center;

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;

        lr.positionCount = 6;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.material = lineMaterial != null
            ? lineMaterial
            : new Material(Shader.Find("Sprites/Default"));

        lr.startColor = lineColor;
        lr.endColor = lineColor;

        lr.sortingLayerName = sortingLayerName;
        lr.sortingOrder = sortingOrder;

        for (int k = 0; k < 6; k++)
        {
            Vector3 corner = center + (Vector3)HexCornerOffset(k, radius, pointyTop);
            corner.z = center.z;
            lr.SetPosition(k, corner);
        }
        spawned.Add(go);

    }

    private static Vector2 HexCornerOffset(int cornerIndex, float r, bool pointy)
    {
        float angleDeg = (pointy ? 30f : 0f) + 60f * cornerIndex;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(r * Mathf.Cos(angleRad), r * Mathf.Sin(angleRad));
    }

}
