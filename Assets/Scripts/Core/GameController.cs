using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject teammatePrefab;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float waitForFeedbackDelay = 0.2f;
    private Vector3 ballOriginalPosition; 

    private List<TeamMate> teammates = new List<TeamMate>();
    private TeamMate currentTarget;

    private int numTeammates = 6;
    private float targetDuration = 1f;
    private float reactionStartTime;
    private float targetTime = 0f;
    private int lastTargetIndex = -1;
    private float maxTapDistance = 1.5f;
    private bool isWaitingForNextTarget = false;

    private int score = 0;
    private int penalty = 0;

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
    }

    private void Update()
    {
        if (currentTarget != null && !isWaitingForNextTarget)
        {
            targetTime += Time.deltaTime;
            if (targetTime >= targetDuration)
            {
                currentTarget.ResetHighlight();
                targetTime = 0f;
                SelectRandomTarget();
            }
        }
    }

    private void SpawnTeammates()
    {
        for (int i = 0; i < numTeammates; i++)
        {
            float angle = i * Mathf.PI * 2 / numTeammates;
            Vector3 teammatePosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            GameObject teammate = Instantiate(teammatePrefab, transform.position + teammatePosition, Quaternion.identity);
            teammates.Add(teammate.GetComponent<TeamMate>());
        }

        Timer.Register(1f, ()=>{
            SelectRandomTarget();
        });
    }

    private void SelectRandomTarget()
    {
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
    }

    private void MoveBallToPosition(Vector2 screenPosition)
    {
        if (isWaitingForNextTarget) return;
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

        // Only register a pass if the tap is close enough to a teammate
        if (tappedTeammate != null && closestDistance <= maxTapDistance)
        {
            StartCoroutine(MoveBallToTarget(tappedTeammate.transform.position));

            // Check if the tapped teammate is the current target
            if (tappedTeammate == currentTarget)
            {
                float reactionTime = Time.time - reactionStartTime;
                Debug.Log("Correct pass! Reaction time: " + reactionTime + " seconds");
                score++;
            }
            else
            {
                score -= penalty;
                Debug.Log("Incorrect pass! Score: " + score);
            }
        }
    }

    private IEnumerator MoveBallToTarget(Vector3 targetPosition)
    {
        isWaitingForNextTarget = true;
        
        float duration = 0.3f;
        Vector3 startPosition = ball.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ball.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ball.transform.position = targetPosition;
        Debug.Log("Pass completed!");
        
        yield return new WaitForSeconds(waitForFeedbackDelay); // Wait for a moment before resetting the ball position
        ball.transform.position = ballOriginalPosition;

        targetTime = 0f;
        SelectRandomTarget();
    }
}