using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelManager", menuName = "Managers/Level Manager")]
public class LevelManager : ScriptableObject
{
    public DifficultySO EasyDifficulty;
    public DifficultySO MediumDifficulty;
    public DifficultySO HardDifficulty;

    public int CurrentLevel
    {
        get => PlayerPrefs.GetInt("CurrentLevel", 1);
        set => PlayerPrefs.SetInt("CurrentLevel", value);
    }

    public DifficultySO GetLevelDifficultyData()
    {
        int level = CurrentLevel;

        // First 10 levels are always easy
        if (level <= 10)
            return EasyDifficulty;

            // For every group of 10 levels after the first 10
            int groupIndex = (level - 11) / 10;
            int groupLevel = (level - 11) % 10;

            // Build a pattern for this group: 3 medium, 2 hard, 5 easy (randomized, but no consecutive hard)
            List<DifficultySO> pool = new List<DifficultySO>();
            pool.AddRange(new[] { MediumDifficulty, MediumDifficulty, MediumDifficulty });
            pool.AddRange(new[] { HardDifficulty, HardDifficulty });
            pool.AddRange(new[] { EasyDifficulty, EasyDifficulty, EasyDifficulty, EasyDifficulty, EasyDifficulty });

            // Shuffle, but ensure no consecutive hard
            List<DifficultySO> pattern = new List<DifficultySO>();
            System.Random rng = new System.Random(groupIndex); // Seed for repeatability
            while (pool.Count > 0)
            {
                // If last was hard, filter out hard for this pick
                List<DifficultySO> candidates = (pattern.Count > 0 && pattern[pattern.Count - 1] == HardDifficulty)
                    ? pool.FindAll(d => d != HardDifficulty)
                    : pool;

                int pick = rng.Next(candidates.Count);
                DifficultySO chosen = candidates[pick];
                pattern.Add(chosen);
                pool.Remove(chosen);
            }

            // Pick the difficulty for this level in the group
            return pattern[groupLevel];
        }
}