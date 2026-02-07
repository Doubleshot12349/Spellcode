using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT (new input system)

public class HexInputController : MonoBehaviour
{
    public Camera cam;
    public PlayerMover player;

    private HexTile hovered;
    private HexTile selected;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        UpdateHover();

        // Left click (new input system)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ClickSelect();
        }
    }

    private void UpdateHover()
    {
        HexTile tileUnderMouse = RaycastTileUnderMouse();

        // hover changed -> reset old hover (unless selected)
        if (tileUnderMouse != hovered)
        {
            if (hovered != null && hovered != selected)
                hovered.ClearHighlight();

            hovered = tileUnderMouse;
        }

        if (hovered == null) return;
        if (hovered == selected) return;

        bool valid = player != null && player.CanMoveTo(hovered);
        if (valid) hovered.SetHoverValid();
        else hovered.SetHoverInvalid();
    }

    private void ClickSelect()
    {
        if (hovered == null || player == null) return;

        if (!player.CanMoveTo(hovered))
            return;

        if (selected != null)
            selected.SetSelected(false);

        selected = hovered;
        selected.SetSelected(true);

        player.MoveTo(selected);
    }

    private HexTile RaycastTileUnderMouse()
    {
        if (Mouse.current == null) return null;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        Vector2 pos2D = new Vector2(world.x, world.y);

        // Raycast "at a point"
        RaycastHit2D hit = Physics2D.Raycast(pos2D, Vector2.zero);
        if (!hit) return null;

        return hit.collider.GetComponent<HexTile>();
    }
}
