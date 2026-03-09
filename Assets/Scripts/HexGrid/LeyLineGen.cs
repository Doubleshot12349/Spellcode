using System.Collections.Generic;
using UnityEngine;

public class LeyLineGen : MonoBehaviour
{
    public GameObject hexGrid;
    public bool generated = false;
    private float min;
    private float max;
    public class LeyLine
    {
        public HexTile tile1;
        public HexTile tile2;
        public float weight;
        public LeyLine(HexTile t1,HexTile t2)
        {
            tile1 = t1;
            tile2 = t2;
            weight = 0;
        }
    }
    private Dictionary<(HexTile,HexTile),LeyLine> leyLines = new Dictionary<(HexTile, HexTile), LeyLine>();

    public bool LeyLineExists(HexTile tile1, HexTile tile2)
    {
        if (leyLines.Count==0) {
            Debug.Log("LeyLines have not been generated yet");
            return false;
        }
        //Using a 2 element key where the order doesn't matter, have to check both combos
        return leyLines.ContainsKey((tile2, tile1)) || leyLines.ContainsKey((tile1, tile2));

    }
    
    public LeyLine GetLeyLine(HexTile tile1, HexTile tile2)
    {
        if (LeyLineExists(tile1, tile2))
        {
            return leyLines[(tile1, tile2)];
        }
        else
        {
            return null;
        }
    }

    public void Awake()
    {
        min = 1;
        max = 20;
    }

    public void GenerateLeyLines()
    {
        foreach (Transform tile in hexGrid.transform)
        {
            var prevTile = tile.gameObject.GetComponent<HexTile>();
            foreach (var neighbor in HexGridManager.GetNeighbors(tile.gameObject.GetComponent<HexTile>().coords))
            {
                if (!LeyLineExists(prevTile, neighbor))
                {
                    //make a new leyline
                    var leyline = new LeyLine(prevTile, neighbor);
                    leyLines.Add((prevTile, neighbor), leyline);
                }
            }
        }
        WeightLeyLines();
        generated = true;
    }

    public void WeightLeyLines()
    {
        foreach (var (_,line) in leyLines)
        {
            line.weight = Random.Range(min,max);
        }
    }


}
