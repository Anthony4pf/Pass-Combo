using UnityEngine;

public class SingletonReference : MonoBehaviour
{
    public GameStateManager GameStateManager;

    bool _firstLoadup = false;
    int isItFirstTimePlaying;

    private void Awake()
    {
        GameStateManager.Init();
    } 
}
