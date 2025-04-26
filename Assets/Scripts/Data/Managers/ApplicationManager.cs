using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "Application", menuName = "Managers/Application Manager")]

public class ApplicationManager : ScriptableObject
{

    [FancyHeader("APPLICATION MANAGER", 3f, "#D4AF37", 8.5f, order = 0)]
    [Space]
    public GameEvent OnSceneLoad;
   

    public GameEvent OnPauseGame;
    public GameEvent OnResumeGame;
    public void PauseGame()
    {
        Time.timeScale = 0f;
        
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
       
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    
}
