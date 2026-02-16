using UnityEngine;

public class Dot : MonoBehaviour
{
    [HideInInspector] public int column;
    [HideInInspector] public int row;
    [HideInInspector] public Board board;
    public int dotType;

    private Vector2 firstTouch;
    private Vector2 lastTouch;

    private void OnMouseDown()
    {
        if (board == null || !board.CanMove) return;
        if (Camera.main == null) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        firstTouch = world;
    }

    private void OnMouseUp()
    {
        if (board == null || !board.CanMove) return;
        if (Camera.main == null) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        lastTouch = world;
        CalculateSwipe();
    }

    private void CalculateSwipe()
    {
        if (Vector2.Distance(firstTouch, lastTouch) < 0.3f)
            return;

        float angle = Mathf.Atan2(
            lastTouch.y - firstTouch.y,
            lastTouch.x - firstTouch.x) * Mathf.Rad2Deg;

        int targetColumn = column;
        int targetRow = row;

        if (angle > -45 && angle <= 45)          // RIGHT
            targetColumn++;
        else if (angle > 45 && angle <= 135)     // UP
            targetRow++;
        else if (angle > -135 && angle <= -45)   // DOWN
            targetRow--;
        else                                     // LEFT
            targetColumn--;

        board.TrySwap(column, row, targetColumn, targetRow);
    }
}
