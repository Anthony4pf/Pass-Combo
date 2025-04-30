using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();   
    }
    void Start()
    {
        if (name == "LevelText")
            text.text = $"Level {GameStateManager.LevelManager.CurrentLevel}";

        if (name == "TargetText")
            text.text = GameStateManager.LevelManager.GetLevelDifficultyData().TargetPoints.ToString();

        if (name == "TierText")
            text.text = GameStateManager.LevelManager.GetLevelDifficultyData().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
