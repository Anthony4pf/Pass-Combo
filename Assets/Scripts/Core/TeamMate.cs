using UnityEngine;

public class TeamMate : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Highlight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }

    public void ResetHighlight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
}
