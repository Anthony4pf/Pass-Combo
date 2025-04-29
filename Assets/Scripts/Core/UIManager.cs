using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    [Header("Score Board")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Win Screen")]
    [SerializeField] private TextMeshProUGUI finalPointsText;
    [SerializeField] private TextMeshProUGUI averageReactionTimeText;
    [SerializeField] private TextMeshProUGUI comboBonusText;
    [SerializeField] private TextMeshProUGUI totalReward;

    [Header("Lose Window")]
    [SerializeField]private TextMeshProUGUI targetText;

    [Header("Events")]
    [SerializeField] private GameEvent OnGameWin;
    [SerializeField] private GameEvent onGameLose;


    private void OnEnable()
    {
        GameController.OnGameEnd += UpdateEndGameUI;
        GameController.OnScoreChanged += UpdateScoreText;
        GameController.OnTimeChanged += UpdateTimerText;
    }

    private void OnDisable()
    {
        GameController.OnGameEnd -= UpdateEndGameUI;
        GameController.OnScoreChanged -= UpdateScoreText;
        GameController.OnTimeChanged -= UpdateTimerText;
    }

    private void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString();
    }
    
    private void UpdateTimerText(float curDuration)
    {
        int minutes = Mathf.FloorToInt(curDuration / 60f);
        int seconds = Mathf.FloorToInt(curDuration % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateEndGameUI(int finalPoints, int comboBonus, float averageReactionTime, int targetPoints, bool isWin)
    {
        finalPointsText.text = finalPoints.ToString();
        comboBonusText.text = comboBonus.ToString();
        averageReactionTimeText.text = averageReactionTime.ToString("F2") + "s";
        targetText.text = targetPoints.ToString();

        int timeReward = Mathf.RoundToInt(1f / Mathf.Max(averageReactionTime, 0.1f));
        int total = finalPoints + comboBonus + timeReward;
        totalReward.text = total.ToString();

        if (isWin)
        {
            OnGameWin?.Raise();
        }
        else
        {
            onGameLose?.Raise();
        }
    }
}
