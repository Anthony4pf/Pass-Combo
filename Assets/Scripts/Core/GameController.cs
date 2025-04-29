using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

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

    [SerializeField] private int targetPoints = 10;
    [SerializeField] private float gameDuration = 30f;

    private float timeRemaining;
    private bool gameActive = false;
    private bool gamePaused = false;

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

    private void Start()
    {
        ballOriginalPosition = ball.transform.position;
        SpawnTeammates();
        OnScoreChanged?.Invoke(score);

        timeRemaining = gameDuration;
        gameActive = true;
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

        //Reset the previous Target
        if (currentTarget != null)
        {
            currentTarget.ResetHighlight();
        }

        int randomIndex;
        do {
            randomIndex = Random.Range(0, teammates.Count);
        } while (randomIndex == lastTargetIndex && teammates.Count > 1);
        lastTargetIndex = randomIndex;
        currentTarget = teammates[randomIndex];
        currentTarget.Highlight();
        isWaitingForNextTarget = false;
        reactionStartTime = Time.time;

        //update UI with latest score
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

        if (tappedTeammate != null && closestDistance <= maxTapDistance)
        {
            // Tapped near a teammate
            if (tappedTeammate == currentTarget)
            {
                float reactionTime = Time.time - reactionStartTime;
                Debug.Log("Correct pass! Reaction time: " + reactionTime + " seconds");
                score += difficultySO.ScoreIncrement;

                // Track reaction time
                totalReactionTime += reactionTime;
                reactionCount++;
                averageReactionTime = totalReactionTime / reactionCount;
            }
            else
            {
                score -= difficultySO.Penalty;
                if (score < 0) score = 0;
                Debug.Log("Incorrect pass! Score: " + score);
            }
        }
        else
        {
            // Tapped empty space
            score -= difficultySO.Penalty;
            if (score < 0) score = 0;
            Debug.Log("Missed! No teammate at tapped position. Score: " + score);
        }
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