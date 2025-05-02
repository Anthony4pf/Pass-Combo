using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private DifficultySO difficultySO;
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject teammatePrefab;

    [SerializeField] private float waitForFeedbackDelay = 0.2f;
    private Vector3 ballOriginalPosition; 

    private List<TeamMate> teammates = new List<TeamMate>();
    private TeamMate currentTarget;

    private float reactionStartTime;
    private float targetTime = 0f;
    private int lastTargetIndex = -1;
    private float maxTapDistance = 1.5f;
    private bool isWaitingForNextTarget = false;

    private int score = 0;
    private int comboBonus;
    private float averageReactionTime;
    private float totalReactionTime = 0f;
    private int reactionCount = 0;
    private int streak = 0;

    [SerializeField] private int targetPoints = 10;
    [SerializeField] private float gameDuration = 30f;

    private float timeRemaining;
    private bool gameActive = false;
    private bool gamePaused = false;

    private List<int[]> hardComboPatterns = new List<int[]>
    {
        new int[] {0, 1, 0, 2},
        new int[] {1, 2, 3, 1},
        new int[] {2, 0, 3, 2}
    };
    private int[] currentComboPattern = null;
    private int comboPatternStep = 0;
    private int comboPatternAttempts = 0;
    private bool isComboPatternActive = false;
    private const int maxComboPatternAttempts = 4;
    private bool comboPatternUsedThisLevel = false;
    private Coroutine comboTimerCoroutine;
    [SerializeField] private GameEvent OnStartBrainBall;
    [SerializeField] private GameEvent OnEndBrainBall;
    [SerializeField] private TextMeshProUGUI brainBallTimerText;
    [SerializeField] private TextMeshProUGUI captionText;


    // Static events for UI updates
    public delegate void EndGameDataHandler(int finalPoints, int comboBonus, float averageReactionTime, int targetPoints, bool isWin);
    public static event EndGameDataHandler OnGameEnd;
    public static event Action<int> OnScoreChanged;
    public static event Action<float> OnTimeChanged;

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnTapEvent += MoveBallToPosition;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnTapEvent -= MoveBallToPosition;
        }
    }

    private void Awake()
    {
        //difficultySO = GameStateManager.LevelManager.GetLevelDifficultyData();
    }

    private void Start()
    {
        ballOriginalPosition = ball.transform.position;
        SpawnTeammates();
        OnScoreChanged?.Invoke(score);

        timeRemaining = gameDuration;
        gameActive = true;
        comboPatternUsedThisLevel = false;
        StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            OnTimeChanged?.Invoke(timeRemaining);
            if (gamePaused == true) yield return new WaitUntil(() => gamePaused == false); 
            yield return null;
        }
        
        OnTimeChanged?.Invoke(0);
        if (currentTarget != null)
            currentTarget.ResetHighlight();
        gameActive = false;

        //play full time whistle sfx
        yield return new WaitForSeconds(1f);

        EndGame();
    }

    private void Update()
    {
        if (currentTarget != null && !isWaitingForNextTarget && gameActive)
        {
            targetTime += Time.deltaTime;
            if (targetTime >= difficultySO.TargetDuration)
            {
                currentTarget.ResetHighlight();
                targetTime = 0f;
                SelectRandomTarget();
            }
        }
    }

    private void EndGame()
    {
        if (score >= targetPoints)
        {
            GameStateManager.LevelManager.CurrentLevel++;
            if (LeaderboardManager.Instance != null)
                LeaderboardManager.Instance.SubmitScore(GameStateManager.LevelManager.CurrentLevel);
            OnGameEnd?.Invoke(score, comboBonus, averageReactionTime, targetPoints, true);
        }
        else
        {
            OnGameEnd?.Invoke(score, comboBonus, averageReactionTime, targetPoints, false);
        }
    }

    private void SpawnTeammates()
    {
        Vector3 center = transform.position;
        for (int i = 0; i < difficultySO.NumberOfTeammates; i++)
        {
            float angle = i * Mathf.PI * 2 / difficultySO.NumberOfTeammates;
            Vector3 teammatePosition = new Vector3(Mathf.Cos(angle) * difficultySO.Radius, Mathf.Sin(angle) * difficultySO.Radius, 0f);
            GameObject teammate = Instantiate(teammatePrefab, center + teammatePosition, Quaternion.identity, transform.parent);

            // Rotate to face the center
            Vector3 directionToCenter = (center - teammate.transform.position).normalized;
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
            teammate.transform.rotation = Quaternion.AngleAxis(angleToCenter, Vector3.forward);

            teammates.Add(teammate.GetComponent<TeamMate>());
        }

        
        Timer.Register(1f, () => {
            SelectRandomTarget();
        });
    }

    private void SelectRandomTarget()
    {
        if (!gameActive) return;

        // Reset the previous Target
        if (currentTarget != null)
            currentTarget.ResetHighlight();

        if (difficultySO.HasComboPatterns && !isComboPatternActive && !comboPatternUsedThisLevel
            && Random.value < 0.3f && timeRemaining > 8f)
        {
            comboPatternUsedThisLevel = true;
            int[] pattern = hardComboPatterns[Random.Range(0, hardComboPatterns.Count)];
            StartCoroutine(PreviewComboPattern(pattern));
            return;
        }

        int randomIndex;
        do {
            randomIndex = Random.Range(0, teammates.Count);
        } while (randomIndex == lastTargetIndex && teammates.Count > 1);
        lastTargetIndex = randomIndex;
        SetCurrentTarget(randomIndex);
    }

    private IEnumerator PreviewComboPattern(int[] pattern)
    {
        Debug.Log("Combo Pattern Active: " + string.Join(", ", pattern));
        isComboPatternActive = true;
        currentComboPattern = pattern;
        comboPatternStep = 0;
        comboPatternAttempts = 0;

        OnStartBrainBall?.Raise();

        // Preview each teammate in the pattern
        yield return new WaitForSeconds(0.2f);
        foreach (int idx in pattern)
        {
            teammates[idx].Highlight();
            yield return new WaitForSeconds(0.5f);
            teammates[idx].ResetHighlight();
            yield return new WaitForSeconds(0.2f);
        }

        // Start combo timer
        if (comboTimerCoroutine != null)
            StopCoroutine(comboTimerCoroutine);
        comboTimerCoroutine = StartCoroutine(ComboPatternTimer(5f));
    }

    private IEnumerator ComboPatternTimer(float timeLimit)
    {
        brainBallTimerText.gameObject.SetActive(true);
        Debug.Log("Start Deadzone Timer");
        float timeLeft = timeLimit;
        while (timeLeft > 0 && isComboPatternActive)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            brainBallTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
            timeLeft -= Time.deltaTime;
        }
        brainBallTimerText.text = "00:00";
        if (isComboPatternActive)
        {
            // Time ran out
            EndComboPattern(false);
        }
    }

    private void SetCurrentTarget(int index)
    {
        if (currentTarget != null)
            currentTarget.ResetHighlight();

        currentTarget = teammates[index];
        currentTarget.Highlight();
        isWaitingForNextTarget = false;
        reactionStartTime = Time.time;
        OnScoreChanged?.Invoke(score);
    }

    private void MoveBallToPosition(Vector2 screenPosition)
    {
        // Prevent ball movement if tap is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!gameActive || isWaitingForNextTarget) return;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        worldPosition.z = ball.transform.position.z;

        // Find the closest teammate to the tapped position
        TeamMate tappedTeammate = null;
        float closestDistance = float.MaxValue;

        foreach (TeamMate teammate in teammates)
        {
            float distance = Vector3.Distance(worldPosition, teammate.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                tappedTeammate = teammate;
            }
        }

        StartCoroutine(MoveBallToTarget(worldPosition));

        // --- Combo Pattern Input ---
        if (isComboPatternActive && currentComboPattern != null)
        {
            int tappedIndex = teammates.IndexOf(tappedTeammate);
            comboPatternAttempts++;

            if (tappedTeammate != null && closestDistance <= maxTapDistance &&
                tappedIndex == currentComboPattern[comboPatternStep])
            {
                // Correct tap for this step
                float reactionTime = Time.time - reactionStartTime;
                totalReactionTime += reactionTime;
                reactionCount++;
                averageReactionTime = totalReactionTime / reactionCount;

                comboPatternStep++;
                if (comboPatternStep >= currentComboPattern.Length)
                {
                    // Pattern complete!
                    Debug.Log("Pattern complete!");
                    score += difficultySO.ComboBonus * 3;
                    EndComboPattern(true);
                }
            }
            else
            {
                // Incorrect tap or missed
                Debug.Log("Incorrect tap or missed! Pattern Over");
                EndComboPattern(false);
            }

            if (comboPatternAttempts >= maxComboPatternAttempts && comboPatternStep < currentComboPattern.Length)
            {
                // Out of attempts
                EndComboPattern(false);
            }
            return;
        }

        // --- Normal pass logic ---
        if (tappedTeammate != null && closestDistance <= maxTapDistance)
        {
            // Tapped near a teammate
            if (tappedTeammate == currentTarget)
            {
                float reactionTime = Time.time - reactionStartTime;
                score += difficultySO.ScoreIncrement;
                totalReactionTime += reactionTime;
                reactionCount++;
                averageReactionTime = totalReactionTime / reactionCount;
                streak++;

                if (difficultySO.HasComboBonus && streak >= difficultySO.ComboPasses)
                {
                    comboBonus += difficultySO.ComboBonus;
                    streak = 0;
                }
            }
            else
            {
                score -= difficultySO.Penalty;
                if (score < 0) score = 0;
                streak = 0;
            }
        }
        else
        {
            // Tapped empty space
            score -= difficultySO.Penalty;
            if (score < 0) score = 0;
            streak = 0;
        }
        OnScoreChanged?.Invoke(score);
    }

    private void EndComboPattern(bool success)
    {
        Debug.Log("End Combo Pattern: " + success);
        if (comboTimerCoroutine != null)
            StopCoroutine(comboTimerCoroutine);
        comboTimerCoroutine = null;
        brainBallTimerText.gameObject.SetActive(false);
        isComboPatternActive = false;
        currentComboPattern = null;
        comboPatternStep = 0;
        comboPatternAttempts = 0;

        if (success)
        {
            score += difficultySO.ScoreIncrement * 5;
            captionText.text = "Tiki-Taka King!";
        }
        else
        {
            score -= difficultySO.Penalty * 2;
            if (score < 0) score = 0;
            captionText.text = "Captain Clueless!";
        }

        Timer.Register(.3f, () =>
        {
            OnScoreChanged?.Invoke(score);

            // Resume normal play
            OnEndBrainBall?.Raise();
            SelectRandomTarget();
        });
    }

    private IEnumerator MoveBallToTarget(Vector3 targetPosition)
    {
        isWaitingForNextTarget = true;

        bool isLeftPass = targetPosition.x < ballOriginalPosition.x;

        // Flip the player to face the direction of the pass
        StartCoroutine(FlipPlayer(isLeftPass));

        // Set ball start position based on direction
        Vector3 startPosition = isLeftPass
            ? new Vector3(-Mathf.Abs(ballOriginalPosition.x), ballOriginalPosition.y, ballOriginalPosition.z)
            : ballOriginalPosition;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ball.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ball.transform.position = targetPosition;
        Debug.Log("Pass completed!");

        yield return new WaitForSeconds(waitForFeedbackDelay);
        ball.transform.position = startPosition;

        targetTime = 0f;
        SelectRandomTarget();
    }

    private IEnumerator FlipPlayer(bool faceLeft)
    {
        float duration = 0.1f;
        float elapsed = 0f;
        float startScaleX = transform.localScale.x;
        float endScaleX = faceLeft ? -Mathf.Abs(startScaleX) : Mathf.Abs(startScaleX);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float newScaleX = Mathf.Lerp(startScaleX, endScaleX, t);
            transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(endScaleX, transform.localScale.y, transform.localScale.z);
    }

    public void PauseGame()
    {
        gameActive = false;
        gamePaused = true;
    }

    public void ResumeGame()
    {
        gameActive = true;
        gamePaused = false;
    }
}