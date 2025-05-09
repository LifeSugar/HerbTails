﻿using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace HT
{
    public class MoYaoHandler :MonoBehaviour
    {

        public RectTransform MoYaoPanel;
        public bool Ready = false;
        public Chu chu;
        public Collider selectCollider;
        
        public List<Herb> herbsInBox = new List<Herb>();
        
        public static MoYaoHandler instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InputHandler.instance.OnStateChange += SwitchMoYao;
            chu.enabled = false;
        }

        public void Tick()
        {
            if (!Ready)
            {
                selectCollider.gameObject.SetActive(true);
                HandleSelectHerb();
            }
            else
            {
                selectCollider.gameObject.SetActive(false);
                chu.enabled = true;
            }
        }

        public void SwitchMoYao(GameState gameState, GameState previousGameState)
        {
            if (gameState == GameState.MOYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                // 如果是从 TOPDOWN 切换过来，打开制作UI
                if (previousGameState == GameState.INSCENE)
                {
                    RectTransform craftUI = InputHandler.instance.craftUICanvas;
                    Vector2 targetPos = new Vector2(706, 37);
                    craftUI.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad)
                        .OnComplete(() => { InputHandler.instance.craftUIOn = true; });
                }
                
                // 把抓药面板移动到可视区域
                Vector2 targetPos_ZhuaYaoPanel = new Vector2(-283.5f, 400f);
                MoYaoPanel.DOAnchorPos(targetPos_ZhuaYaoPanel, duration).SetEase(Ease.OutQuad);
            }
            else if (previousGameState == GameState.MOYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                Vector2 targetPos_MoYaoPanel = new Vector2(-283.5f, -1600f);
                MoYaoPanel.DOAnchorPos(targetPos_MoYaoPanel, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() => {MoYaoPanel.localPosition = new Vector2(-283.5f, 800f); });
            }
        }

        void HandleSelectHerb()
        {
            Ray ray = Utility.GetRayFromRealCamScreenPos(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (Input.GetMouseButtonDown(0)
                    && !CursorSlot.instance.isEmpty 
                    && hit.collider == selectCollider)
                {
                    var cursorItem = CursorSlot.instance.cursorItem;
                    if (cursorItem.GridType == GridTypes.HERBS)
                    {
                        var herb = ResourceManager.instance.GetHerb(cursorItem.Name) as Herb;
                        herbsInBox.Add(herb);
                        cursorItem.Count -= 1;
                        CursorSlot.instance.UpdateCursorSlot();
                    }
                }
            }
        }
    }
}