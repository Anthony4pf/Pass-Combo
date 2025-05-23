using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "DifficultySO", menuName = "Scriptable Objects/DifficultySO")]
public class DifficultySO : ScriptableObject
{
    public enum DifficultyTier
    {
        Easy,
        Medium,
        Hard
    }
    [SerializeField] private DifficultyTier difficultyTier;
    [SerializeField] private float targetDuration;
    [SerializeField] private int targetPoints;
    [SerializeField] private int numberOfTeammates;
    [SerializeField] private int penalty;
    [SerializeField] private int scoreIncrement;
    [SerializeField] private int radius;
    [SerializeField] private bool hasComboBonus;
    [SerializeField] private bool hasComboPatterns;
    [ShowIf("hasComboBonus")] [SerializeField] private int comboPasses;
    [ShowIf("hasComboBonus")] [SerializeField] private int comboBonus;

    public float TargetDuration => targetDuration;
    public int TargetPoints => targetPoints;
    public int Penalty => penalty;
    public int ScoreIncrement => scoreIncrement;
    public bool HasComboBonus => hasComboBonus;
    public bool HasComboPatterns => hasComboPatterns;
    public int ComboPasses => comboPasses;
    public int ComboBonus => comboBonus;
    public int NumberOfTeammates => numberOfTeammates;
    public int Radius => radius;
    public DifficultyTier m_difficultyTier => difficultyTier;
}