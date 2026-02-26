using UnityEngine;
using System.Collections;

public class PlayerMover : MonoBehaviour
{
    public HexGridManager grid;

    public float moveSpeed = 6f;

    public HexTile currentTile;
    public int startHexQ;
    public int startHexR;

    private bool isMoving;

    public void SnapToTile(HexTile tile)
    {
        currentTile = tile;
        transform.position = tile.transform.position;
    }

    public bool CanMoveTo(HexTile target)
    {
        if (target == null) return false;
        if (!target.walkable) return false;
        if (currentTile == null) return false;

        return grid != null && grid.AreAdjacent(currentTile.coords, target.coords);
    }

    public void MoveTo(HexTile target)
    {
        if (isMoving) return;
        if (!CanMoveTo(target)) return;

        StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(HexTile target)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        float t = 0f;
        float dist = Vector3.Distance(start, end);
        float duration = dist / Mathf.Max(0.01f, moveSpeed);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        currentTile = target;

        isMoving = false;
    }

    /*public void AutoSnapToNearestTile()
    {
        if (grid == null) return;

        HexTile nearest = null;
        float best = float.PositiveInfinity;

        foreach (Transform child in grid.tilesParent)
        {
            var t = child.GetComponent<HexTile>();
            if (t == null) continue;

            float d = Vector3.SqrMagnitude(transform.position - t.transform.position);
            if (d < best)
            {
                best = d;
                nearest = t;
            }
        }

        if (nearest != null)
            SnapToTile(nearest);
    }*/

    private void Start()
    {
        //AutoSnapToNearestTile();
        currentTile = HexGridManager.GetHex(startHexQ, startHexR).GetComponent<HexTile>();
        SnapToTile(currentTile);
    }
}
