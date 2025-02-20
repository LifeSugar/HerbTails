using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace HT
{
    
    //这是一个秤
    public class Balance : MonoBehaviour
    {
        public float maxWeight = 100f; //可测量的最大重量
        public float minWeight = 1f; //可测量的最小重量
        
        [FormerlySerializedAs("weight")] public float weightLeft = 10f;
        [FormerlySerializedAs("weightMeature")] public float weightRight = 10f;
        public float maxAngle = 15f;
        public float tweenDuration = 0.3f;
        public float interpolationSpeed = 5f;

        private Tween currentTween;
        // 当前天平角度（绕局部 x 轴）
        private float currentAngle = 0f;
        

        

        public void UpdateBalance()
        {
            float diff = weightRight - weightLeft;
            float normalized = diff / weightLeft;
            float targetAngle = Mathf.Clamp(-normalized * maxAngle, -maxAngle, maxAngle);
            //
            // // 如果已有 Tween，则判断目标角度是否发生变化
            // if (currentTween != null && currentTween.IsActive())
            // {
            //     currentTween.Kill(); // 干掉之前的 Tween
            // }
            //
            // // 启动一个新的 Tween 进行平滑过渡
            // currentTween = transform.DOLocalRotate(new Vector3(targetAngle, 0, 0), tweenDuration);
            
            // 手动插值
            // Time.deltaTime * interpolationSpeed 控制平滑速度
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * interpolationSpeed);

            // 更新天平的局部旋转，绕 x 轴旋转
            transform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
        }

        public void ResetBalance()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        
        
    }

}