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

    private GameManager gameManager;

    private void Start()
    {
        AllDotsInTheBoard = new GameObject[width, height];
        SetUpBoard();
        SetUpCamera();

        // cache GameManager to report score/time when matches happen
        gameManager = FindObjectOfType<GameManager>();
    }

    #region Setup

    private void SetUpBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                // Avoid initial matches by re-rolling until safe
                int randomDot;
                int attempts = 0;
                do
                {
                    randomDot = Random.Range(0, dots.Length);
                    attempts++;
                    // safety to avoid infinite loop if dots.Length < required variety
                    if (attempts > 50) break;
                } while (WouldCreateInitialMatch(x, y, randomDot));

                GameObject dot = Instantiate(dots[randomDot], pos, Quaternion.identity, transform);
                Dot dotScript = dot.GetComponent<Dot>();
                dotScript.column = x;
                dotScript.row = y;
                dotScript.board = this;
                dotScript.dotType = randomDot;

                AllDotsInTheBoard[x, y] = dot;
            }
        }
    }

    private bool WouldCreateInitialMatch(int x, int y, int candidateDotType)
    {
        // Check horizontally: two left
        if (x >= 2)
        {
            var left1 = AllDotsInTheBoard[x - 1, y];
            var left2 = AllDotsInTheBoard[x - 2, y];
            if (left1 != null && left2 != null)
            {
                var dt1 = left1.GetComponent<Dot>().dotType;
                var dt2 = left2.GetComponent<Dot>().dotType;
                if (dt1 == candidateDotType && dt2 == candidateDotType)
                    return true;
            }
        }

        // Check vertically: two down
        if (y >= 2)
        {
            var down1 = AllDotsInTheBoard[x, y - 1];
            var down2 = AllDotsInTheBoard[x, y - 2];
            if (down1 != null && down2 != null)
            {
                var dt1 = down1.GetComponent<Dot>().dotType;
                var dt2 = down2.GetComponent<Dot>().dotType;
                if (dt1 == candidateDotType && dt2 == candidateDotType)
                    return true;
            }
        }

        return false;
    }

    private void SetUpCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position =
                new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10);
        }
    }

    #endregion

    #region Swap

    public void TrySwap(int col1, int row1, int col2, int row2)
    {
        // Validate move state
        if (!CanMove) return;

        // Validate both positions inside board
        if (!IsInsideBoard(col1, row1) || !IsInsideBoard(col2, row2))
            return;

        // Validate adjacency (Manhattan distance == 1)
        int dx = Mathf.Abs(col1 - col2);
        int dy = Mathf.Abs(row1 - row2);
        if (dx + dy != 1) return;

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

        // Guard against nulls
        if (dot1 == null || dot2 == null)
        {
            CanMove = true;
            yield break;
        }

        Vector2 pos1 = dot1.transform.position;
        Vector2 pos2 = dot2.transform.position;

        float duration = 0.25f;

        // Animate swap
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

        // Check if this swap created any matches
        yield return new WaitForSeconds(0.05f); // brief wait to let state settle
        bool anyMatches = FindMatchRuns().Count > 0;

        if (!anyMatches)
        {
            // Revert the swap visually and logically
            Sequence revertSeq = DOTween.Sequence();
            revertSeq.Join(dot1.transform.DOMove(pos1, duration).SetEase(Ease.OutQuad));
            revertSeq.Join(dot2.transform.DOMove(pos2, duration).SetEase(Ease.OutQuad));
            yield return revertSeq.WaitForCompletion();

            // Revert in array
            AllDotsInTheBoard[col1, row1] = dot1;
            AllDotsInTheBoard[col2, row2] = dot2;

            // Revert logical coordinates
            dot1Script.column = col1;
            dot1Script.row = row1;

            dot2Script.column = col2;
            dot2Script.row = row2;

            CanMove = true;
            yield break;
        }

        // If there are matches, process them
        yield return StartCoroutine(CheckAndProcessMatches());
        CanMove = true;
    }

    private IEnumerator CheckAndProcessMatches()
    {
        yield return new WaitForSeconds(0.1f);

        var runs = FindMatchRuns(); // list of runs (each run is a horizontal or vertical match)
        if (runs.Count > 0)
        {
            // Calculate points: match of size 3 => 1 point, size 4 => 2 points, etc.
            int totalPoints = 0;
            foreach (var run in runs)
            {
                int runSize = run.Count;
                int pointsForRun = Mathf.Max(0, runSize - 2); // 3->1, 4->2, 5->3...
                totalPoints += pointsForRun;
            }

            if (totalPoints > 0 && gameManager != null)
                gameManager.AddPoints(totalPoints);

            // collect unique dots to destroy
            HashSet<GameObject> uniqueMatches = new HashSet<GameObject>();
            foreach (var run in runs)
                foreach (var dot in run)
                    if (dot != null)
                        uniqueMatches.Add(dot);

            List<GameObject> matchedDots = uniqueMatches.ToList();

            yield return StartCoroutine(DestroyMatches(matchedDots));
            yield return StartCoroutine(CollapseBoard());
            yield return StartCoroutine(RefillBoard());

            // combo chain
            yield return StartCoroutine(CheckAndProcessMatches());
        }
    }

    /// <summary>
    /// Scans the board and returns a list of match "runs".
    /// Each run is a contiguous horizontal or vertical sequence of >= 3 same-type dots.
    /// Overlapping tiles may appear in multiple runs (so T/L shapes count as multiple runs).
    /// </summary>
    private List<List<GameObject>> FindMatchRuns()
    {
        var runs = new List<List<GameObject>>();

        // Horizontal runs
        for (int y = 0; y < height; y++)
        {
            int runStartX = 0;
            int runType = -1;
            int runCount = 0;

            for (int x = 0; x < width; x++)
            {
                var cell = AllDotsInTheBoard[x, y];
                if (cell == null)
                {
                    // end current run
                    if (runCount >= 3)
                    {
                        var run = new List<GameObject>();
                        for (int k = 0; k < runCount; k++)
                            run.Add(AllDotsInTheBoard[runStartX + k, y]);
                        runs.Add(run);
                    }
                    runCount = 0;
                    runType = -1;
                }
                else
                {
                    int type = cell.GetComponent<Dot>().dotType;
                    if (type == runType)
                    {
                        runCount++;
                    }
                    else
                    {
                        if (runCount >= 3)
                        {
                            var run = new List<GameObject>();
                            for (int k = 0; k < runCount; k++)
                                run.Add(AllDotsInTheBoard[runStartX + k, y]);
                            runs.Add(run);
                        }
                        // start new run
                        runType = type;
                        runCount = 1;
                        runStartX = x;
                    }
                }
            }

            if (runCount >= 3)
            {
                var run = new List<GameObject>();
                for (int k = 0; k < runCount; k++)
                    run.Add(AllDotsInTheBoard[runStartX + k, y]);
                runs.Add(run);
            }
        }

        // Vertical runs
        for (int x = 0; x < width; x++)
        {
            int runStartY = 0;
            int runType = -1;
            int runCount = 0;

            for (int y = 0; y < height; y++)
            {
                var cell = AllDotsInTheBoard[x, y];
                if (cell == null)
                {
                    if (runCount >= 3)
                    {
                        var run = new List<GameObject>();
                        for (int k = 0; k < runCount; k++)
                            run.Add(AllDotsInTheBoard[x, runStartY + k]);
                        runs.Add(run);
                    }
                    runCount = 0;
                    runType = -1;
                }
                else
                {
                    int type = cell.GetComponent<Dot>().dotType;
                    if (type == runType)
                    {
                        runCount++;
                    }
                    else
                    {
                        if (runCount >= 3)
                        {
                            var run = new List<GameObject>();
                            for (int k = 0; k < runCount; k++)
                                run.Add(AllDotsInTheBoard[x, runStartY + k]);
                            runs.Add(run);
                        }
                        runType = type;
                        runCount = 1;
                        runStartY = y;
                    }
                }
            }

            if (runCount >= 3)
            {
                var run = new List<GameObject>();
                for (int k = 0; k < runCount; k++)
                    run.Add(AllDotsInTheBoard[x, runStartY + k]);
                runs.Add(run);
            }
        }

        return runs;
    }

    private List<GameObject> FindAllMatches()
    {
        // kept for compatibility: return unique set of matched tiles (flat)
        var runs = FindMatchRuns();
        var unique = new HashSet<GameObject>();
        foreach (var run in runs)
            foreach (var d in run)
                if (d != null)
                    unique.Add(d);
        return unique.ToList();
    }

    private IEnumerator DestroyMatches(List<GameObject> matches)
    {
        foreach (GameObject dot in matches)
        {
            if (dot != null)
            {
                int col = dot.GetComponent<Dot>().column;
                int row = dot.GetComponent<Dot>().row;

                if (col >= 0 && col < width && row >= 0 && row < height)
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

    // Exposed method so GameManager can enable/disable input
    public void SetCanMove(bool value)
    {
        CanMove = value;
    }
}
