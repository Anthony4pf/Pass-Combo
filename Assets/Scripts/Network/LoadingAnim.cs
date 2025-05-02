using UnityEngine;
using DG.Tweening;

public class LoadingAnim : MonoBehaviour
{
    private Tween rotateTween;

    private void Start()
    {
        // Start rotating the arrow infinitely
        rotateTween = transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDisable()
    {
        // Stop the animation when disabled
        if (rotateTween != null && rotateTween.IsActive())
            rotateTween.Kill();
    }

    private void OnEnable()
    {
        // Resume rotation when enabled
        if (rotateTween == null || !rotateTween.IsActive())
        {
            rotateTween = transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}