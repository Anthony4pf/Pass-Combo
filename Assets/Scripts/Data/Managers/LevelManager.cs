using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelManager", menuName = "Managers/Level Manager")]
public class LevelManager : ScriptableObject
{
    public DifficultySO EasyDifficulty;
    public DifficultySO MediumDifficulty;
    public DifficultySO HardDifficulty;
}