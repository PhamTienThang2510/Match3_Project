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
    [SerializeField] private float HintDelay = 5f;
    [SerializeField] private int maxShuffleAttempts = 10;

    [Header("Hint Settings")]
    [Tooltip("Strength (world units) of the shake hint on X axis")]
    [SerializeField] private float hintShakeStrength = 0.2f;
    [Tooltip("Vibrato (number of shakes)")]
    [SerializeField] private int hintVibrato = 10;
    [Tooltip("Duration for one shake cycle in seconds")]
    [SerializeField] private float hintCycleDuration = 0.5f;
    [Tooltip("Scale pulse amount while hinting")]
    [SerializeField] private float hintScale = 1.15f;
    [Tooltip("Scale pulse duration")]
    [SerializeField] private float hintScaleDuration = 0.25f;

    private readonly Vector3 baseScale = new Vector3(0.8f, 0.8f, 0.8f);

    private float hintTimer = 0f;

    public int Width => width;
    public int Height => height;

    public bool CanMove { get; private set; } = true;

    public GameObject[,] AllDotsInTheBoard;

    // hint runtime state
    private Tween hintTweenA;
    private Tween hintTweenB;
    private Tween hintScaleTweenA;
    private Tween hintScaleTweenB;
    private GameObject hintA;
    private GameObject hintB;

    private void Start()
    {
        AllDotsInTheBoard = new GameObject[width, height];
        SetUpBoard();
        SetUpCamera();
    }

    private void Update()
    {
        if (!CanMove) return;
        hintTimer += Time.deltaTime;
        if (hintTimer >= HintDelay)
        {
            hintTimer = 0f;
            ShowHint();
        }
    }

    #region Setup
    // this method initializes the board with random dots, ensuring no initial matches of 3 or more in a row/column.
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

                // ensure consistent base scale
                dot.transform.localScale = baseScale;

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

        // stop any hint while player is about to move
        StopHint();

        StartCoroutine(SwapWithAnimation(col1, row1, col2, row2));
    }

    // Helper to check if given coordinates are within board bounds
    private bool IsInsideBoard(int col, int row)
    {
        return col >= 0 && col < width &&
               row >= 0 && row < height;
    }

    // This coroutine handles the visual swap animation, checks for matches, and reverts if no matches are created.
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

        // play swap sound when animation begins (if AudioManager present)
        AudioManager.Instance?.PlaySwap();

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
        hintTimer = 0f; // reset hint timer after a successful move
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

            if (totalPoints > 0)
                GameManager.Instance.AddPoints(totalPoints);

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
    #endregion

    #region Match Processing
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

                    dot.transform.localScale = baseScale;

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

    #region Hints
    private void ShowHint()
    {
        // stop any previous hint first
        StopHint();

        // find possible moves
        var possible = FindPossibleMoves();

        if (possible == null || possible.Count == 0)
        {
            // no possible moves -> shuffle/reshuffle board
            StartCoroutine(ShuffleBoard());
            return;
        }

        // show a shaking hint for the first possible move (two tiles)
        var firstMove = possible[0];
        if (firstMove == null || firstMove.Count < 2) return;

        GameObject a = firstMove[0];
        GameObject b = firstMove[1];
        if (a == null || b == null) return;

        hintA = a;
        hintB = b;

        // DOShakePosition with X-axis strength only; loops until StopHint is called
        hintTweenA = a.transform.DOShakePosition(hintCycleDuration, new Vector3(hintShakeStrength, 0f, 0f), hintVibrato, 0f, false, true)
            .SetLoops(-1, LoopType.Restart);
        hintTweenB = b.transform.DOShakePosition(hintCycleDuration, new Vector3(hintShakeStrength, 0f, 0f), hintVibrato, 0f, false, true)
            .SetLoops(-1, LoopType.Restart);

        // optional small scale pulse in parallel
        hintScaleTweenA = a.transform.DOScale(hintScale, hintScaleDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        hintScaleTweenB = b.transform.DOScale(hintScale, hintScaleDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void StopHint()
    {
        // kill tweens (if any)
        if (hintTweenA != null) { hintTweenA.Kill(); hintTweenA = null; }
        if (hintTweenB != null) { hintTweenB.Kill(); hintTweenB = null; }
        if (hintScaleTweenA != null) { hintScaleTweenA.Kill(); hintScaleTweenA = null; }
        if (hintScaleTweenB != null) { hintScaleTweenB.Kill(); hintScaleTweenB = null; }

        // Reset ONLY the two hinted tiles to their grid positions and base scale.
        if (hintA != null)
        {
            var da = hintA.GetComponent<Dot>();
            if (da != null)
            {
                hintA.transform.position = new Vector3(da.column, da.row, 0f);
            }
            hintA.transform.localScale = baseScale;
            hintA = null;
        }

        if (hintB != null)
        {
            var db = hintB.GetComponent<Dot>();
            if (db != null)
            {
                hintB.transform.position = new Vector3(db.column, db.row, 0f);
            }
            hintB.transform.localScale = baseScale;
            hintB = null;
        }
    }

    private List<List<GameObject>> FindPossibleMoves()
    {
        List<List<GameObject>> possibleMoves = new List<List<GameObject>>();

        // iterate every cell and try swapping with right and up neighbor (avoids duplicates)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject current = AllDotsInTheBoard[x, y];
                if (current == null) continue;

                // right neighbor
                if (x + 1 < width && AllDotsInTheBoard[x + 1, y] != null)
                {
                    SwapDotTypes(x, y, x + 1, y);
                    var runs = FindMatchRuns();
                    if (runs.Count > 0)
                        possibleMoves.Add(new List<GameObject> { AllDotsInTheBoard[x, y], AllDotsInTheBoard[x + 1, y] });
                    SwapDotTypes(x, y, x + 1, y); // swap back
                }

                // up neighbor
                if (y + 1 < height && AllDotsInTheBoard[x, y + 1] != null)
                {
                    SwapDotTypes(x, y, x, y + 1);
                    var runs = FindMatchRuns();
                    if (runs.Count > 0)
                        possibleMoves.Add(new List<GameObject> { AllDotsInTheBoard[x, y], AllDotsInTheBoard[x, y + 1] });
                    SwapDotTypes(x, y, x, y + 1); // swap back
                }
            }
        }

        return possibleMoves;
    }

    // swap only the dotType values for a quick simulation (keeps GameObjects in-place)
    private void SwapDotTypes(int x1, int y1, int x2, int y2)
    {
        var a = AllDotsInTheBoard[x1, y1];
        var b = AllDotsInTheBoard[x2, y2];
        if (a == null || b == null) return;

        var da = a.GetComponent<Dot>();
        var db = b.GetComponent<Dot>();
        int tmp = da.dotType;
        da.dotType = db.dotType;
        db.dotType = tmp;
    }

    private IEnumerator ShuffleBoard()
    {
        // ensure hint stopped while shuffling
        StopHint();

        // prevent input while shuffling
        CanMove = false;

        int attempts = 0;
        bool foundMove = false;

        while (attempts < maxShuffleAttempts && !foundMove)
        {
            attempts++;

            // Collect all existing GameObjects
            List<GameObject> all = new List<GameObject>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (AllDotsInTheBoard[x, y] != null)
                        all.Add(AllDotsInTheBoard[x, y]);

            // Fisher-Yates shuffle the list
            for (int i = all.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = all[i];
                all[i] = all[j];
                all[j] = tmp;
            }

            // Reassign shuffled objects back to board and animate to new positions
            int index = 0;
            Sequence seq = DOTween.Sequence();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var obj = all[index++];
                    AllDotsInTheBoard[x, y] = obj;
                    Dot ds = obj.GetComponent<Dot>();
                    ds.column = x;
                    ds.row = y;
                    seq.Join(obj.transform.DOMove(new Vector2(x, y), 0.25f).SetEase(Ease.OutQuad));
                }
            }

            yield return seq.WaitForCompletion();

            // give a frame to settle then check for possible moves
            yield return null;
            var possible = FindPossibleMoves();
            if (possible != null && possible.Count > 0)
                foundMove = true;
        }

        // if we still didn't find a move after attempts, as a fallback reinitialize board
        if (!foundMove)
        {
            // destroy existing and rebuild
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (AllDotsInTheBoard[x, y] != null)
                        Destroy(AllDotsInTheBoard[x, y]);

            AllDotsInTheBoard = new GameObject[width, height];
            SetUpBoard();
        }

        CanMove = true;
        hintTimer = 0f;
    }
    #endregion

    // Exposed method so GameManager can enable/disable input
    public void SetCanMove(bool value)
    {
        CanMove = value;
    }
}
