using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace HT
{
    public class MoYaoHandler :MonoBehaviour
    {

        public RectTransform MoYaoPanel;
        public bool Ready = false;
        public Chu chu;
        public Collider selectCollider;

        [FormerlySerializedAs("herbsIn")] public InventorySlot herbsInSlot;
        [FormerlySerializedAs("GrindedOut")] public InventorySlot GrindedOutSlot;
        
        public static MoYaoHandler instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InputHandler.instance.OnStateChange += SwitchMoYao;
        }

        public void Tick()
        {
            if (herbsInSlot.isEmpty) 
                HandleSelectHerb();
            else
            {
                UpdateMarkerPosition();
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
            LayerMask mask = 1 << 11;
            if (Physics.Raycast(ray, out hit, mask))
            {
                if (Input.GetMouseButtonDown(0)
                    && !CursorSlot.instance.isEmpty 
                    && hit.collider == selectCollider
                    && herbsInSlot.isEmpty && GrindedOutSlot.isEmpty)
                {
                    var cursorItem = CursorSlot.instance.cursorItem;
                    if (cursorItem.GridType == GridTypes.HERBS)
                    {
                        chu.ResetPound();
                        Utility.DeepCopyUISlot(CursorSlot.instance.cursorItem, herbsInSlot.slotItem, true ,false);
                        CursorSlot.instance.cursorItem.Count = 0;
                        CursorSlot.instance.UpdateCursorSlot();
                        herbsInSlot.UpdateSlot();
                        return;
                    }
                }
            }
        }


        // Marker活动范围
        public float minX = 30f;
        public float maxX = 300f;
        public RectTransform Marker;
        public int targetCount = 12;
        void UpdateMarkerPosition()
        {
            if (Marker == null || chu == null)
                return;

            float clampedPound = Mathf.Clamp(chu.PoundCount, 0f, targetCount);
            float targetX = Mathf.Lerp(minX, maxX, clampedPound / targetCount);

            // 平滑移动
            Vector2 anchoredPos = Marker.anchoredPosition;
            anchoredPos.x = Mathf.Lerp(anchoredPos.x, targetX, Time.deltaTime * 10f); 
            Marker.anchoredPosition = anchoredPos;
            if (chu.PoundCount == targetCount)
            {
                var herbin = ResourceManager.instance.GetHerb(herbsInSlot.slotItem.Name);
                var grindedherbs = ResourceManager.instance.GetGrindedHerb(herbin.GrindedHerb);

                GrindedOutSlot.slotItem = new UISlot()
                {
                    GridType = GridTypes.GRINDEDHERBS,
                    Name = grindedherbs.Name,
                    Count = herbsInSlot.slotItem.Count,
                    Icon = grindedherbs.Icon,
                };
                
                GrindedOutSlot.UpdateSlot();
                herbsInSlot.slotItem.Count = 0;
                herbsInSlot.UpdateSlot();
                herbsInSlot.isEmpty = true;
                
                

            }
        }

    }
}