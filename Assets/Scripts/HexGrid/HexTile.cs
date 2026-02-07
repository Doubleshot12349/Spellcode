using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("Grid Data")]
    public HexCoords coords;
    public bool walkable = true;

    [Header("Highlight Colors")]
    public Color hoverValidColor = new Color(0.4f, 1f, 0.4f, 0.7f);   // green
    public Color hoverInvalidColor = new Color(1f, 0.4f, 0.4f, 0.7f); // red
    public Color selectedColor = new Color(1f, 1f, 0.3f, 0.8f);       // yellow

    [Header("Refs")]
    [SerializeField] private SpriteRenderer highlightSR; // assign Highlight sprite renderer here

    private bool isSelected;

    private void Awake()
    {
        // If you forgot to assign it in Inspector, try to find it automatically.
        if (highlightSR == null)
        {
            var child = transform.Find("Highlight");
            if (child != null) highlightSR = child.GetComponent<SpriteRenderer>();
        }

        ClearHighlight();
    }

    public void ClearHighlight()
    {
        if (highlightSR == null) return;
        if (isSelected) return; // selected stays yellow
        var c = highlightSR.color;
        c.a = 0f;
        highlightSR.color = c;
    }

    public void SetHoverValid()
    {
        if (highlightSR == null || isSelected) return;
        highlightSR.color = hoverValidColor;
    }

    public void SetHoverInvalid()
    {
        if (highlightSR == null || isSelected) return;
        highlightSR.color = hoverInvalidColor;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (highlightSR == null) return;

        if (selected) highlightSR.color = selectedColor;
        else ClearHighlight();
    }
}
