using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT (new input system)

public class HexInputController : MonoBehaviour
{
    public Camera cam;
    public PlayerMover player;
    public PlayerMover player1;
    public PlayerMover player2;
    public GameObject Player1Obj;
    public GameObject Player2Obj;
    public TurnManager turnManager;

    private HexTile hovered;
    private HexTile selected;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        player1 = Player1Obj.GetComponent<PlayerMover>();
        player2 = Player2Obj.GetComponent<PlayerMover>();
    }

    private void Update()
    {
        if (turnManager.currentTurn == TurnState.Player1Turn)
        {
            player = player1;
        }
        else
        {
            player = player2;
        }
        UpdateHover();

        // Left click (new input system)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            LeftClickSelect();
        //right click
        }else if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            RightClickSelect();
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

        bool validMove = player != null && player.CanMoveTo(hovered);
        bool validSpell = player!=null && player.CanCastOn(hovered);
        if (validMove) hovered.SetHoverValid();
        else if (validSpell) hovered.SetHoverSpellValid();
        else hovered.SetHoverInvalid();

    }

    private void LeftClickSelect()
    {
        if (hovered == null || player == null) return;

        if (!player.CanMoveTo(hovered))
            return;

        if (selected != null)
            selected.SetSelected(false);

        selected = hovered;
        selected.SetSelected(true);

        player.MoveTo(selected);
        player.gameObject.GetComponent<PlayerController>().hasMoved = true;
    }

    private void RightClickSelect()
    {
        if (hovered == null || player == null) return;

        if (!player.CanCastOn(hovered))
            return;
        player.GetComponent<PlayerController>().selectedHex = hovered;
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
