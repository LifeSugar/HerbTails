using UnityEngine;
using DG.Tweening;
using HT;
using PixelPerfectURP;
using UnityEngine.UI;

namespace HT
{
    public class CanvasPopIn : MonoBehaviour
    {
        [Header("缩放动画设置")] public float startScale = 0.8f;
        public float endScale = 1.0f;
        public float duration = 0.3f;
        public int loopCount = -1; // -1代表无限循环
        public LoopType loopType = LoopType.Yoyo;
        public Image image;


        private Tween scaleTween;

        private void OnEnable()
        {
            // this.transform.forward = PixelCameraManager.Instance.transform.forward;
            transform.localScale = Vector3.one * startScale;
            scaleTween = transform.DOScale(endScale, duration)
                .SetEase(Ease.OutQuad)
                .SetLoops(loopCount, loopType);

        }

        private void OnDisable()
        {

            if (scaleTween != null && scaleTween.IsActive())
            {
                scaleTween.Kill();
            }
        }
    }
}