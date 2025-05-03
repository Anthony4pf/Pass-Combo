using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject m_tutorialPanel;
    [SerializeField] private GameObject tutorialBox;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject scoreboardObj;
    [SerializeField] private GameObject brainBallObj;

    public string howToPlayMessage;
    public string pointsMessage;
    public string brainBallMessage;
    public IEnumerator HowToPlayTutorial()
    {
        m_tutorialPanel.SetActive(true);
        tutorialText.text = howToPlayMessage;

        yield return new WaitForSeconds(2);

        tutorialBox.SetActive(false);
    }

    public IEnumerator PointsTutorial()
    {
        yield return new WaitForSeconds(1);

        scoreboardObj.transform.SetParent(m_tutorialPanel.transform);
        tutorialBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 400);
        tutorialBox.SetActive(true);
        tutorialText.text = pointsMessage;

        yield return new WaitForSeconds(2f);

        m_tutorialPanel.SetActive(false);
        scoreboardObj.transform.SetParent(m_tutorialPanel.transform.parent);
        scoreboardObj.transform.SetAsFirstSibling();
    }

    public IEnumerator BrainBallTutorial()
    {
        Transform originalParent = brainBallObj.transform.parent;
        m_tutorialPanel.SetActive(true);
        brainBallObj.transform.SetParent(m_tutorialPanel.transform);
        tutorialText.text = brainBallMessage;
        
        yield return new WaitForSeconds(2f);

        brainBallObj.transform.SetParent(originalParent);
        m_tutorialPanel.SetActive(false);
    }
}