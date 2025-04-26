using UnityEngine;

public class SingletonReference : MonoBehaviour
{
    public GameStateManager GameStateManager;

    bool _firstLoadup = false;
    int isItFirstTimePlaying;

    private void Awake()
    {
        isItFirstTimePlaying = PlayerPrefs.GetInt("isItFirstTimePlaying");
        SetDefaultValues();

        GameStateManager.Init();
    } 


    void SetDefaultValues()
    {
        _firstLoadup = (isItFirstTimePlaying == 0) ? true : false;
        if (_firstLoadup)
        {
          
            UnlockCharacters();
           
        }
        else
        {
            return;
        }
    }

    void UnlockCharacters()
    {
        
    }
}
