using UnityEngine;

[CreateAssetMenu(fileName = "SFXManager", menuName = "Scriptable Objects/SFXManager")]
public class SFXManager : ScriptableObject
{
    public void Click()
    {
        AudioManager.Instance?.PlaySFX("Click");
    }
}
