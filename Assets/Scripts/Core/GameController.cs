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
    public enum GameState
    {
        Inactive,
        Tutorial,
        Playing,
        Paused,
        PreviewingCombo,
        ComboActive,
        Ended
    }

    [SerializeField] private InputReader inputReader;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private DifficultySO difficultySO;
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject teammatePrefab;
    [SerializeField] private List<Sprite> teammateSprites = new List<Sprite>();
    [SerializeField]private GameState gameState = GameState.Inactive;
    [SerializeField] bool playTutorial;
    
    [SerializeField] private RectTransform levelBanner;

    [SerializeField] private float waitForFeedbackDelay = 0.2f;
    private Vector3 ballOriginalPosition; 

    private List<TeamMate> teammates = new List<TeamMate>();
    private TeamMate currentTarget;

    private float reactionStartTime;
    private float targetTime = 0f;
    private int lastTargetIndex = -1;
    private float maxTapDistance = 1.5f;

    private int score = 0;
    private int comboBonus;
    private float averageReactionTime;
    private float totalReactionTime = 0f;
    private int reactionCount = 0;
    private int streak = 0;

    [SerializeField] private int targetPoints = 10;
    [SerializeField] private float gameDuration = 30f;

    private float timeRemaining;

    private List<int[]> hardComboPatterns = new List<int[]>
    {
        new int[] {0, 1, 0, 2},
        new int[] {1, 2, 3, 1},
        new int[] {2, 0, 3, 2}
    };
    private int[] currentComboPattern = null;
    private int comboPatternStep = 0;
    private int comboPatternAttempts = 0;
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
        difficultySO = GameStateManager.LevelManager.GetLevelDifficultyData();
    }

    private void Start()
    {
        ballOriginalPosition = ball.transform.position;
        OnScoreChanged?.Invoke(score);

        timeRemaining = gameDuration;
        comboPatternUsedThisLevel = false;

        SpawnTeammates();
    }

    private IEnumerator GameTimer()
    {
        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            OnTimeChanged?.Invoke(timeRemaining);
            if (gameState == GameState.Paused) yield return new WaitUntil(() => gameState == GameState.Playing);
            if (gameState == GameState.Tutorial)
                yield return new WaitUntil(() => gameState == GameState.Playing || gameState == GameState.PreviewingCombo);
            yield return null;
        }
        
        OnTimeChanged?.Invoke(0);
        if (currentTarget != null)
            currentTarget.ResetHighlight();

        gameState = GameState.Ended;

        //play full time whistle sfx
        AudioManager.Instance?.PlaySFX("Whistle");
        yield return new WaitForSeconds(1f);

        EndGame();
    }

    private void Update()
    {
        if (currentTarget != null && gameState == GameState.Playing)
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

   private void SpawnTeammates()
    {
        Vector3 center = transform.position;
        for (int i = 0; i < difficultySO.NumberOfTeammates; i++)
        {
            float angle = i * Mathf.PI * 2 / difficultySO.NumberOfTeammates;
            Vector3 teammatePosition = new Vector3(Mathf.Cos(angle) * difficultySO.Radius, Mathf.Sin(angle) * difficultySO.Radius, 0f);
            GameObject teammate = Instantiate(teammatePrefab, center + teammatePosition, Quaternion.identity, transform.parent);

            SpriteRenderer sr = teammate.GetComponent<SpriteRenderer>();
            if (sr != null && teammateSprites != null && teammateSprites.Count > 0)
            {
                sr.sprite = teammateSprites[Random.Range(0, teammateSprites.Count)];
            }

            // Rotate to face the center
            Vector3 directionToCenter = (center - teammate.transform.position).normalized;
            float angleToCenter = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
            teammate.transform.rotation = Quaternion.AngleAxis(angleToCenter, Vector3.forward);

            teammates.Add(teammate.GetComponent<TeamMate>());
        }

        ShowLevelBannerAndSelectTarget();
    }

    private void ShowLevelBannerAndSelectTarget()
    {
        levelBanner.gameObject.SetActive(true);

        LeanTween.moveY(levelBanner, 0f, 0.4f).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            // Wait for a moment, then move it back up
            LeanTween.delayedCall(0.6f, () =>
            {
                LeanTween.moveY(levelBanner, 700f, 0.4f).setEase(LeanTweenType.easeInCubic).setOnComplete(() =>
                {
                    levelBanner.gameObject.SetActive(false);
                    AudioManager.Instance?.PlaySFX("Whistle");

                    StartCoroutine(PrepareGame());
                });
            });
        });
    }

    public IEnumerator PrepareGame()
    {
        if (!PlayerPrefs.HasKey("HowToPlayTut"))
        {
            gameState = GameState.Tutorial;

            // Highlight the first target for the tutorial
            int tutorialTargetIndex = 0;
            SetCurrentTarget(tutorialTargetIndex);
            
            yield return StartCoroutine(tutorialManager.HowToPlayTutorial());

            // Wait until the player makes the first correct pass
            yield return new WaitUntil(() => score > 0);

            PlayerPrefs.SetInt("HowToPlayTut", 1);

            yield return StartCoroutine(tutorialManager.PointsTutorial());
        }

        yield return new WaitForSeconds(0.5f);
        gameState = GameState.Playing;
        StartCoroutine(GameTimer());
        SelectRandomTarget();
    }

    private void SelectRandomTarget()
    {
        if (gameState != GameState.Playing) return;

        // Reset the previous Target
        if (currentTarget != null)
            currentTarget.ResetHighlight();

        if (difficultySO.HasComboPatterns && gameState != GameState.ComboActive && !comboPatternUsedThisLevel
            && timeRemaining > 8f && Random.value < 0.3)
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

        currentComboPattern = pattern;
        comboPatternStep = 0;
        comboPatternAttempts = 0;

        gameState = GameState.Tutorial;
        OnStartBrainBall?.Raise();

        if (!PlayerPrefs.HasKey("BrainBallTut"))
        {
            yield return StartCoroutine(tutorialManager.BrainBallTutorial());
            PlayerPrefs.SetInt("BrainBallTut", 1);
        }

        gameState = GameState.PreviewingCombo;
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
        Debug.Log("Start BrainBall Timer");
        float timeLeft = timeLimit;
        gameState = GameState.ComboActive;
        while (timeLeft > 0)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            brainBallTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
            timeLeft -= Time.deltaTime;
        }
        brainBallTimerText.text = "00:00";
        if (gameState == GameState.ComboActive)
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
        if (gameState != GameState.Tutorial)
            gameState = GameState.Playing;
        reactionStartTime = Time.time;
        OnScoreChanged?.Invoke(score);
    }

    private void MoveBallToPosition(Vector2 screenPosition)
    {
        // Prevent ball movement if tap is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Raycast all UI under the pointer
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // Allow if any UI under pointer has the allowed tag
            if (!results.Exists(r => r.gameObject.CompareTag("AllowUIClick")))
                return;
        }

        if (gameState != GameState.Playing && gameState != GameState.ComboActive && gameState != GameState.Tutorial)
            return;

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

        GameState currentState = gameState;
        StartCoroutine(MoveBallToTarget(worldPosition));

        // --- Combo Pattern Input ---
        if (currentState == GameState.ComboActive && currentComboPattern != null)
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
                AudioManager.Instance?.PlaySFX("CorrectPass");
                if (comboPatternStep >= currentComboPattern.Length)
                {
                    // Pattern complete!
                    Debug.Log("Pattern complete!");
                    EndComboPattern(true);
                }
            }
            else
            {
                // Incorrect tap or missed
                Debug.Log("Incorrect tap or missed! Pattern Over");
                AudioManager.Instance?.PlaySFX("WrongPass");
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
                AudioManager.Instance?.PlaySFX("CorrectPass");

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
                AudioManager.Instance?.PlaySFX("WrongPass");
            }
        }
        else
        {
            // Tapped empty space
            score -= difficultySO.Penalty;
            if (score < 0) score = 0;
            streak = 0;
            AudioManager.Instance?.PlaySFX("WrongPass");
        }
        OnScoreChanged?.Invoke(score);
    }

     private void EndGame()
    {
        if (score >= targetPoints)
        {
            GameStateManager.LevelManager.CurrentLevel++;
            OnGameEnd?.Invoke(score, comboBonus, averageReactionTime, targetPoints, true);
            if (PlayFabManager.Instance != null)
                PlayFabManager.Instance.SubmitScore(GameStateManager.LevelManager.CurrentLevel);
        }
        else
        {
            OnGameEnd?.Invoke(score, comboBonus, averageReactionTime, targetPoints, false);
        }
    }
    private void EndComboPattern(bool success)
    {
        Debug.Log("End Combo Pattern: " + success);
        if (comboTimerCoroutine != null)
            StopCoroutine(comboTimerCoroutine);
        comboTimerCoroutine = null;
        brainBallTimerText.gameObject.SetActive(false);
        gameState = GameState.Playing;
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

        Timer.Register(.5f, () =>
        {
            OnScoreChanged?.Invoke(score);

            // Resume normal play
            OnEndBrainBall?.Raise();
            SelectRandomTarget();
        });
    }

    private IEnumerator MoveBallToTarget(Vector3 targetPosition)
    {
        GameState currentState = gameState;
        gameState = GameState.Inactive;
        AudioManager.Instance?.PlaySFX("KickBall");

        // Flip the player to face the direction of the pass
        StartCoroutine(FlipPlayer(targetPosition));

        // Set ball start position based on direction
        Vector3 dir = (targetPosition - ballOriginalPosition).normalized;
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        Vector3 startPosition = ballOriginalPosition;

        if (absX >= absY)
        {
            // Horizontal pass
            if (dir.x < 0)
                startPosition = new Vector3(-Mathf.Abs(ballOriginalPosition.x), ballOriginalPosition.y, ballOriginalPosition.z); // Left
            else
                startPosition = new Vector3(Mathf.Abs(ballOriginalPosition.x), ballOriginalPosition.y, ballOriginalPosition.z);  // Right
        }
        else
        {
            // Vertical pass
            if (dir.y > 0)
                startPosition = new Vector3(0, 0.6f, ballOriginalPosition.z);   // Up
            else
                startPosition = new Vector3(0, -0.6f, ballOriginalPosition.z);  // Down
        }

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
        gameState = currentState;

        if (gameState != GameState.ComboActive)
            SelectRandomTarget();
    }

    private IEnumerator FlipPlayer(Vector3 targetPosition)
    {
        float duration = 0.1f;
        float elapsed = 0f;

        Vector3 dir = (targetPosition - ballOriginalPosition).normalized;
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        float startScaleX = transform.localScale.x;
        float endScaleX = Mathf.Abs(startScaleX);
        float startRotZ = transform.eulerAngles.z;
        float endRotZ = 0f;

        if (absX >= absY)
        {
            // Horizontal pass
            if (dir.x < 0)
                endScaleX = -Mathf.Abs(startScaleX); // Left
            else
                endScaleX = Mathf.Abs(startScaleX);  // Right
            endRotZ = 0f;
        }
        else
        {
            // Vertical pass
            endScaleX = Mathf.Abs(startScaleX); // No flip
            if (dir.y > 0)
                endRotZ = 90f;   // Up
            else
                endRotZ = -90f;  // Down
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float newScaleX = Mathf.Lerp(startScaleX, endScaleX, t);
            float newRotZ = Mathf.LerpAngle(startRotZ, endRotZ, t);
            transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
            transform.rotation = Quaternion.Euler(0, 0, newRotZ);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(endScaleX, transform.localScale.y, transform.localScale.z);
        transform.rotation = Quaternion.Euler(0, 0, endRotZ);
    }

    public void PauseGame()
    {
        gameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        gameState = GameState.Playing;
    }
}