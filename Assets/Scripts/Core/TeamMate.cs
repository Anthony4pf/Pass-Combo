using UnityEngine;

public class TeamMate : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    [SerializeField] private Material glowMaterial;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
    }

    public void Highlight()
    {
        if (spriteRenderer != null)
        {
            //spriteRenderer.color = Color.red;
            if (glowMaterial != null)
                spriteRenderer.material = glowMaterial;
        }
    }

    public void ResetHighlight()
    {
        if (spriteRenderer != null)
        {
            //spriteRenderer.color = Color.white;
            if (originalMaterial != null)
                spriteRenderer.material = originalMaterial;
        }
    }
}