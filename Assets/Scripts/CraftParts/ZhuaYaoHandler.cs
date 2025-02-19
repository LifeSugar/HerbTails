using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Herbs
{
    public class ZhuaYaoHandler : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform ZhuaYaoPanel;

        private void Start()
        {
            InputHandler.instance.OnStateChange += SwitchZhuaYao;
        }


        public void Tick()
        {
            
        }
        /// <summary>
        /// 切换到/退出“抓药”界面的UI动画
        /// </summary>
        public void SwitchZhuaYao(GameState gameState, GameState previousGameState)
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