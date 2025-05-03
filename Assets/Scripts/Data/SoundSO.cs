using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/SoundSO")]
public class SoundSO : ScriptableObject
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;
}