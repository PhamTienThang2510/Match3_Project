using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Dot : MonoBehaviour
{
    [HideInInspector] public int column;
    [HideInInspector] public int row;
    [HideInInspector] public Board board;

    private GameObject target;

    private Vector2 firstTouch;
    private Vector2 lastTouch;
    private float swipeAngle;

    private bool isSwapping = false;

    private void OnMouseDown()
    {
        if (isSwapping) return;

        firstTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (isSwapping) return;

        lastTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        if (Vector2.Distance(firstTouch, lastTouch) < 0.2f)
            return;

        swipeAngle = Mathf.Atan2(
            lastTouch.y - firstTouch.y,
            lastTouch.x - firstTouch.x) * Mathf.Rad2Deg;

        SwipePieces();
    }

    void SwipePieces()
    {
        target = null;

        // RIGHT
        if (swipeAngle > -45 && swipeAngle <= 45)
        {
            if (column < board.Width - 1)
                target = board.AllDotsInTheMatch[column + 1, row];
        }
        // UP
        else if (swipeAngle > 45 && swipeAngle <= 135)
        {
            if (row < board.Height - 1)
                target = board.AllDotsInTheMatch[column, row + 1];
        }
        // DOWN
        else if (swipeAngle > -135 && swipeAngle <= -45)
        {
            if (row > 0)
                target = board.AllDotsInTheMatch[column, row - 1];
        }
        // LEFT
        else
        {
            if (column > 0)
                target = board.AllDotsInTheMatch[column - 1, row];
        }

        if (target != null)
        {
            StartCoroutine(SwapWithAnimation());
        }
    }

    IEnumerator SwapWithAnimation()
    {
        isSwapping = true;

        Dot targetDot = target.GetComponent<Dot>();

        Vector2 startPos = transform.position;
        Vector2 targetPos = target.transform.position;

        float duration = 0.25f;

        Sequence seq = DOTween.Sequence();

        seq.Join(transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad));
        seq.Join(target.transform.DOMove(startPos, duration).SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        // --- Update board array ---
        board.AllDotsInTheMatch[column, row] = target;
        board.AllDotsInTheMatch[targetDot.column, targetDot.row] = gameObject;

        // --- Swap logic coordinates ---
        int tempColumn = targetDot.column;
        int tempRow = targetDot.row;

        targetDot.column = column;
        targetDot.row = row;

        column = tempColumn;
        row = tempRow;

        target = null;
        isSwapping = false;
    }
}
