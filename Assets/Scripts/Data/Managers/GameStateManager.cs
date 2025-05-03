using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/GameState Manager")]
public class GameStateManager : SingletonScriptableObject<GameStateManager>
{
    [FancyHeader("GAMESTATE MANAGER", 3f, "#D4AF37", 8.5f, order = 0)]
    [Space(order = 1)]
    [CustomProgressBar(hideWhenZero = true, label = "m_loadingTxt"), SerializeField] public float m_loadingBar;
    [HideInInspector] public string m_loadingTxt;
    [HideInInspector] public bool m_loadingDone = false;
    [Space]



    [HeaderAttribute("Managers")]
    [SerializeField] private ApplicationManager m_applicationManager;

    [SerializeField] private EconomyManager m_economyManager;

    [SerializeField] private LevelManager m_levelManager;
    [SerializeField] private SFXManager m_sfxManager;



    public static EconomyManager EconomyManager
    {
        get { return Instance.m_economyManager; }

    }
    public static LevelManager LevelManager
    {
        get { return Instance.m_levelManager; }

    }
    public static ApplicationManager ApplicationManager
    {
        get { return Instance.m_applicationManager; }
    }

    public static SFXManager SFXManager
    {
        get { return Instance.m_sfxManager; }

    }

    public void Init()
    {
        EconomyManager.InitializeValues();
    }
}