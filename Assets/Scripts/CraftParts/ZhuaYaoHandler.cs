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
            }
        }
    }

}