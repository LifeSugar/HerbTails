using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace HT
{
    public class ZhuaYaoHandler : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform ZhuaYaoPanel;

        public TextMeshProUGUI weightText;

        public float weightTextPosLeft;
        public float weightTextPosRight;
        
        public FaMa faMa;
        private Balance balance;
        
        public bool isMeasuring = false;

        private void Start()
        {
            faMa = GetComponentInChildren<FaMa>();
            balance = GetComponentInChildren<Balance>();
            InputHandler.instance.OnStateChange += SwitchZhuaYao;
        }


        public void Tick()
        {
            if (!isMeasuring)
            {
                faMa.Tick();
                weightText.text = CalculateMeasureWeight().ToString();
                Vector2 Textpos = weightText.rectTransform.anchoredPosition;
                Textpos.x = weightTextPosLeft + faMa.currentRatio * (weightTextPosRight - weightTextPosLeft);
                weightText.rectTransform.anchoredPosition = Textpos;
                
            }
            else
            {
                balance.UpdateBalance();
            }
        }
        /// <summary>
        /// 切换到/退出“抓药”界面的UI动画
        /// </summary>
        private void SwitchZhuaYao(GameState gameState, GameState previousGameState)
        {
            Debug.Log($"从{previousGameState} 切换到 {gameState}");

            if (gameState == GameState.ZHUAYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                // 如果是从 TOPDOWN 切换过来，打开制作UI
                if (previousGameState == GameState.TOPDOWN)
                {
                    RectTransform craftUI = InputHandler.instance.craftUICanvas;
                    Vector2 targetPos = new Vector2(706, 37);
                    craftUI.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad)
                        .OnComplete(() => { InputHandler.instance.craftUIOn = true; });
                }
                
                // 把抓药面板移动到可视区域
                Vector2 targetPos_ZhuaYaoPanel = new Vector2(0, 0);
                ZhuaYaoPanel.DOAnchorPos(targetPos_ZhuaYaoPanel, duration).SetEase(Ease.OutQuad);
            }
            else if (previousGameState == GameState.ZHUAYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                Vector2 targetPos_zhuaYaoPanel = new Vector2(0, -1200f);
                ZhuaYaoPanel.DOAnchorPos(targetPos_zhuaYaoPanel, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() => {ZhuaYaoPanel.localPosition = new Vector2(0, 720f); });
            }
        }

        public float CalculateMeasureWeight()
        {
            var weight =Mathf.Floor( faMa.currentRatio * (balance.maxWeight - balance.minWeight));
            return weight;
        }
        
        public static ZhuaYaoHandler instance { get; private set; }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("there is more than one ZhuaYaoHandler");
            }
            else
            {
                instance = this;
            }
        }
    }

}