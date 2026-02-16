using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Game References")]
    [SerializeField] private Board board;

    [Header("Timer / Scoring")]
    [Tooltip("Starting time in seconds")]
    [SerializeField] private float initialTime = 60f;
    [Tooltip("Maximum allowed time (cap)")]
    [SerializeField] private float maxTime = 120f;
    [Tooltip("Time bonus awarded per point")]
    [SerializeField] private float timeBonusPerPoint = 0.5f;

    private int score;
    private bool isPaused;
    private float timeRemaining;

    private void Awake()
    {
        if (board == null)
            board = FindObjectOfType<Board>();
    }

    void Start()
    {
        score = 0;
        UpdatePointsText();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // initialize timer
        timeRemaining = initialTime;
        UpdateTimeText();

        // ensure normal time at start
        Time.timeScale = 1f;
        isPaused = false;
        if (board != null)
            board.SetCanMove(true);
    }

    void Update()
    {
        // simple keyboard shortcut to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        // countdown timer runs only when not paused and game not over
        if (!isPaused)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                UpdateTimeText();
                GameOver();
            }
            else
            {
                UpdateTimeText();
            }
        }
    }

    public void AddPoints(int amount)
    {
        if (amount == 0) return;
        score += amount;
        UpdatePointsText();

        // grant time bonus when scoring
        float bonus = amount * timeBonusPerPoint;
        timeRemaining += bonus;
        if (timeRemaining > maxTime) timeRemaining = maxTime;
        UpdateTimeText();
    }

    private void UpdatePointsText()
    {
        if (pointsText != null)
            pointsText.text = score.ToString();
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
        {
            // Display as whole seconds
            timeText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        if (board != null) board.SetCanMove(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        if (board != null) board.SetCanMove(true);
    }

    public void GameOver()
    {
        isPaused = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        if (board != null) board.SetCanMove(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetScore() => score;
}
