using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public RectTransform fader;
    [SerializeField] private float fadeDuration = 0.5f;
    
    public void LoadLevel(int sceneIndex)
    {
        fader.gameObject.SetActive(true);
        CanvasGroup canvasGroup = fader.gameObject.GetComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
            .setOnComplete(() => StartCoroutine(ChangeScene(sceneIndex)));
    }
    
    IEnumerator ChangeScene(int sceneIndex)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadSceneAsync(sceneIndex);
    } 
}
