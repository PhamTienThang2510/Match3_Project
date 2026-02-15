using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] dots;

    public int Width => width;
    public int Height => height;

    public bool CanMove { get; private set; } = true;

    public GameObject[,] AllDotsInTheMatch;

    private void Start()
    {
        AllDotsInTheMatch = new GameObject[width, height];
        SetUpBoard();
        SetUpCamera();
    }

    #region Setup

    private void SetUpBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                int randomDot = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[randomDot], pos, Quaternion.identity, transform);

                Dot dotScript = dot.GetComponent<Dot>();
                dotScript.column = x;
                dotScript.row = y;
                dotScript.board = this;

                AllDotsInTheMatch[x, y] = dot;
            }
        }
    }

    private void SetUpCamera()
    {
        Camera.main.transform.position =
            new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10);
    }

    #endregion

    #region Swap

    public void TrySwap(int col1, int row1, int col2, int row2)
    {
        if (!IsInsideBoard(col2, row2))
            return;

        StartCoroutine(SwapWithAnimation(col1, row1, col2, row2));
    }

    private bool IsInsideBoard(int col, int row)
    {
        return col >= 0 && col < width &&
               row >= 0 && row < height;
    }

    private IEnumerator SwapWithAnimation(int col1, int row1, int col2, int row2)
    {
        CanMove = false;

        GameObject dot1 = AllDotsInTheMatch[col1, row1];
        GameObject dot2 = AllDotsInTheMatch[col2, row2];

        Vector2 pos1 = dot1.transform.position;
        Vector2 pos2 = dot2.transform.position;

        float duration = 0.25f;

        Sequence seq = DOTween.Sequence();

        seq.Join(dot1.transform.DOMove(pos2, duration).SetEase(Ease.OutQuad));
        seq.Join(dot2.transform.DOMove(pos1, duration).SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        // Swap in array
        AllDotsInTheMatch[col1, row1] = dot2;
        AllDotsInTheMatch[col2, row2] = dot1;

        // Update logical coordinates
        Dot dot1Script = dot1.GetComponent<Dot>();
        Dot dot2Script = dot2.GetComponent<Dot>();

        dot1Script.column = col2;
        dot1Script.row = row2;

        dot2Script.column = col1;
        dot2Script.row = row1;

        CanMove = true;
    }

    #endregion
}
