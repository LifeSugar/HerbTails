using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Herbs
{
    public class JianYaoHandler : MonoBehaviour
    {
        [Header("扇子")] public ShanZi shanzi;
        public bool shanziInHand;
        [Header ("UI")] public RectTransform JianYaoPanel;
        
        [Header ("控火参数")]
        // 在多少时间窗口内统计点击（单位：秒）
        public float timeWindow = 1f;

        // 当点击频率低于这个值时，返回 0
        public float thresholdFreq = 2f;

        // 点击频率的最大参考值，超出后就直接返回 maxValue
        public float maxFreq = 10f;

        // 映射之后的返回值上限
        public float maxValue = 100f;

        // 用于存储点击时间点
        private List<float> clickTimes = new List<float>();

        // 用于调试或在其它脚本中访问
        public float currentValue = 0f;

        void Start()
        {
            shanzi = this.gameObject.GetComponentInChildren<ShanZi>();
            InputHandler.instance.OnStateChange += SwitchJianYao;
        }

        public void SwitchJianYao(GameState gameState, GameState previousGameState)
        {
            Debug.Log($"从{previousGameState}切换到{gameState}");
            
            if (gameState == GameState.JIANYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                if (previousGameState == GameState.TOPDOWN)
                {
                    RectTransform craftUI = InputHandler.instance.craftUICanvas;
                    Vector2 targetPos = new Vector2(706, 37);
                    craftUI.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            InputHandler.instance.craftUIOn = true;
                        });
                }
                Vector2 targetPos_FireBar = new Vector2(-283.5f, 400f);
                JianYaoPanel.DOAnchorPos(targetPos_FireBar, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {

                    });
            }
            else if (previousGameState == GameState.JIANYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                Vector2 targetPos_FireBar = new Vector2(-283.5f, -1600f);
                JianYaoPanel.DOAnchorPos(targetPos_FireBar, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        JianYaoPanel.localPosition = new Vector2(-283.5f, 600f);

                    });
            }
        }

        //用于InputHandler的Update
        public void Tick()
        {
            shanzi.SwitchShanzi();
            if (shanziInHand)
            {
                ControlFire();
                if (currentValue > 0)
                {
                    shanzi.Wave(currentValue);
                }
            }
        }

        void ControlFire()
        {
            // 检测鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                // 记录点击时刻
                clickTimes.Add(Time.time);
            }

            // 移除在 timeWindow 之外的旧点击
            while (clickTimes.Count > 0 && (Time.time - clickTimes[0] > timeWindow))
            {
                clickTimes.RemoveAt(0);
            }

            // 计算当前点击频率（单位：次/秒）
            float freq = clickTimes.Count / timeWindow;

            // 如果低于阈值，直接返回 0
            if (freq < thresholdFreq)
            {
                currentValue = 0f;
            }
            else
            {
                // freq 超过了 thresholdFreq ~ maxFreq 之间，做一个线性映射
                // 例如 thresholdFreq=5, maxFreq=10
                // 当 freq=5 -> t=0； freq=10 -> t=1
                float t = (freq - thresholdFreq) / (maxFreq - thresholdFreq);

                // 夹到 [0,1] 区间
                t = Mathf.Clamp01(t);

                // 映射到 [0, maxValue] 区间
                currentValue = t * maxValue;
            }
        }
        
        
        public static JianYaoHandler instance { get; private set; }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("there is more than one JianYaoHandler in the scene");
            }
            instance = this;
        }

    }

}