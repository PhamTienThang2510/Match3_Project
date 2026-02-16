using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private GameObject[] dots;

    public int Width => width;
    public int Height => height;

    public bool CanMove { get; private set; } = true;

    public GameObject[,] AllDotsInTheBoard;

    private void Start()
    {
        AllDotsInTheBoard = new GameObject[width, height];
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

                int randomDot = Random.Range(0, dots.Length);
                GameObject dot = Instantiate(dots[randomDot], pos, Quaternion.identity, transform);
                Dot dotScript = dot.GetComponent<Dot>();
                dotScript.column = x;
                dotScript.row = y;
                dotScript.board = this;

                AllDotsInTheBoard[x, y] = dot;
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

        GameObject dot1 = AllDotsInTheBoard[col1, row1];
        GameObject dot2 = AllDotsInTheBoard[col2, row2];

        Vector2 pos1 = dot1.transform.position;
        Vector2 pos2 = dot2.transform.position;

        float duration = 0.25f;

        Sequence seq = DOTween.Sequence();

        seq.Join(dot1.transform.DOMove(pos2, duration).SetEase(Ease.OutQuad));
        seq.Join(dot2.transform.DOMove(pos1, duration).SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        // Swap in array
        AllDotsInTheBoard[col1, row1] = dot2;
        AllDotsInTheBoard[col2, row2] = dot1;

        // Update logical coordinates
        Dot dot1Script = dot1.GetComponent<Dot>();
        Dot dot2Script = dot2.GetComponent<Dot>();

        dot1Script.column = col2;
        dot1Script.row = row2;

        dot2Script.column = col1;
        dot2Script.row = row1;
        yield return StartCoroutine(CheckAndProcessMatches());
        CanMove = true;
    }
    private IEnumerator CheckAndProcessMatches()
    {
        yield return new WaitForSeconds(0.1f);

        List<GameObject> matchedDots = FindAllMatches();

        if (matchedDots.Count > 0)
        {
            yield return StartCoroutine(DestroyMatches(matchedDots));
            yield return StartCoroutine(CollapseBoard());
            yield return StartCoroutine(RefillBoard());

            // combo chain
            yield return StartCoroutine(CheckAndProcessMatches());
        }
    }
    private List<GameObject> FindAllMatches()
    {
        List<GameObject> matches = new List<GameObject>();

        // ===== HORIZONTAL CHECK =====
        for (int y = 0; y < height; y++)
        {
            int matchCount = 1;

            for (int x = 0; x < width - 1; x++)
            {
                if (AllDotsInTheBoard[x, y] == null ||
                    AllDotsInTheBoard[x + 1, y] == null)
                    continue;

                Dot current = AllDotsInTheBoard[x, y].GetComponent<Dot>();
                Dot next = AllDotsInTheBoard[x + 1, y].GetComponent<Dot>();

                if (current.dotType == next.dotType)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int k = 0; k < matchCount; k++)
                            matches.Add(AllDotsInTheBoard[x - k, y]);
                    }

                    matchCount = 1;
                }
            }

            if (matchCount >= 3)
            {
                for (int k = 0; k < matchCount; k++)
                    matches.Add(AllDotsInTheBoard[width - 1 - k, y]);
            }
        }

        // ===== VERTICAL CHECK =====
        for (int x = 0; x < width; x++)
        {
            int matchCount = 1;

            for (int y = 0; y < height - 1; y++)
            {
                if (AllDotsInTheBoard[x, y] == null ||
                    AllDotsInTheBoard[x, y + 1] == null)
                    continue;

                Dot current = AllDotsInTheBoard[x, y].GetComponent<Dot>();
                Dot next = AllDotsInTheBoard[x, y + 1].GetComponent<Dot>();

                if (current.dotType == next.dotType)
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= 3)
                    {
                        for (int k = 0; k < matchCount; k++)
                            matches.Add(AllDotsInTheBoard[x, y - k]);
                    }

                    matchCount = 1;
                }
            }

            if (matchCount >= 3)
            {
                for (int k = 0; k < matchCount; k++)
                    matches.Add(AllDotsInTheBoard[x, height - 1 - k]);
            }
        }

        return matches.Distinct().ToList();
    }

    private IEnumerator DestroyMatches(List<GameObject> matches)
    {
        foreach (GameObject dot in matches)
        {
            if (dot != null)
            {
                int col = dot.GetComponent<Dot>().column;
                int row = dot.GetComponent<Dot>().row;

                AllDotsInTheBoard[col, row] = null;

                Destroy(dot);
            }
        }

        yield return new WaitForSeconds(0.2f);
    }
    private IEnumerator CollapseBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (AllDotsInTheBoard[x, y] == null)
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (AllDotsInTheBoard[x, k] != null)
                        {
                            GameObject fallingDot = AllDotsInTheBoard[x, k];

                            AllDotsInTheBoard[x, y] = fallingDot;
                            AllDotsInTheBoard[x, k] = null;

                            Dot dotScript = fallingDot.GetComponent<Dot>();
                            dotScript.row = y;

                            fallingDot.transform
                                .DOMove(new Vector2(x, y), 0.2f)
                                .SetEase(Ease.OutQuad);

                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
    }
    private IEnumerator RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (AllDotsInTheBoard[x, y] == null)
                {
                    Vector2 spawnPos = new Vector2(x, height + 1);

                    int randomDot = Random.Range(0, dots.Length);
                    GameObject dot = Instantiate(dots[randomDot], spawnPos, Quaternion.identity, transform);

                    Dot dotScript = dot.GetComponent<Dot>();
                    dotScript.column = x;
                    dotScript.row = y;
                    dotScript.board = this;
                    dotScript.dotType = randomDot;

                    AllDotsInTheBoard[x, y] = dot;

                    dot.transform
                        .DOMove(new Vector2(x, y), 0.3f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }

        yield return new WaitForSeconds(0.4f);
    }

    #endregion
}
