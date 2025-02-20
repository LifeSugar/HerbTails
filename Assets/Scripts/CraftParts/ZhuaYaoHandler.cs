using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PixelPerfectURP;
using TMPro;

namespace HT
{
    public class ZhuaYaoHandler : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform ZhuaYaoPanel;

        public TextMeshProUGUI weightText;
        public Button measureButton;

        public float weightTextPosLeft;
        public float weightTextPosRight;

        
        public FaMa faMa;
        private Balance balance;
        public Collider chengPan;
        public Transform spawnPoint;
        
        public bool isMeasuring = false;

        [SerializeField] private List<RuntimeHerb> herbsIn = new List<RuntimeHerb>();
        private void Start()
        {
            faMa = GetComponentInChildren<FaMa>();
            balance = GetComponentInChildren<Balance>();
            InputHandler.instance.OnStateChange += SwitchZhuaYao;
            measureButton.onClick.AddListener(() => StatMeasuring());
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
            GetMeasureItem();
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

        float CalculateMeasureWeight()
        {
            var weight = Mathf.Clamp(Mathf.Floor( faMa.currentRatio * (balance.maxWeight - balance.minWeight)), balance.minWeight, balance.maxWeight); ;
            return weight;
        }

        private void StatMeasuring()
        {
            isMeasuring = true;
            balance.weightRight = CalculateMeasureWeight();
        }

        private void GetMeasureItem()
        {
            Vector2 mousePos = Input.mousePosition;
            Ray ray = GlobalFunctions.GetRayFromRealCamScreenPos(mousePos);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.red);
            if (Input.GetMouseButtonDown(0) && !CursorSlot.instance.isEmpty)
            {
                var cursorItem = CursorSlot.instance.cursorItem;
                if (cursorItem.GridType == GridTypes.HERBS)
                {
                    
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider == chengPan)
                        {
                            Debug.Log("HerbsGot");
                            var herbGot = new Herb();
                            herbGot = ResourceManager.instance.GetHerb(cursorItem.Name);
                            var pos = new Vector3(hit.point.x, spawnPoint.position.y, spawnPoint.position.z);
                            GameObject herbPrefab = Instantiate(herbGot.Prefab, pos, Quaternion.identity);
                            herbPrefab.transform.parent = this.transform;
                            var rtHerb = herbPrefab.GetComponent<RuntimeHerb>();
                            GlobalFunctions.DeepCopyHerb(herbGot, rtHerb.herb, false);
                            herbsIn.Add(rtHerb);
                            cursorItem.Count -= 1;
                            CursorSlot.instance.UpdateCursorSlot();
                            balance.weightLeft += herbGot.Weight;
                        }
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (CursorSlot.instance.isEmpty && herbsIn.Count > 0)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider == chengPan)
                        {
                            CursorSlot.instance.cursorItem= new UISlot()
                            {
                                GridType = GridTypes.HERBS,
                                Name = herbsIn[herbsIn.Count - 1].herb.Name,
                                Count = 1,
                                Icon = herbsIn[herbsIn.Count - 1].herb.Icon
                            };
                            balance.weightLeft -= herbsIn[herbsIn.Count - 1].herb.Weight;
                            Destroy( herbsIn[herbsIn.Count - 1].gameObject);
                            herbsIn.RemoveAt(herbsIn.Count - 1);
                            
                            CursorSlot.instance.UpdateCursorSlot();
                        }
                    }
                }
                
                if (!CursorSlot.instance.isEmpty && herbsIn.Count > 0)
                {
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider == chengPan)
                        {
                            var cursorItem = CursorSlot.instance.cursorItem;
                            var matchRTHerb = herbsIn.LastOrDefault(rtherb => rtherb.herb.Name == cursorItem.Name);
                            if (matchRTHerb != null)
                            {
                                cursorItem.Count += 1;
                                balance.weightLeft -= matchRTHerb.herb.Weight;
                                Destroy(matchRTHerb.gameObject);
                                herbsIn.Remove(matchRTHerb);
                                CursorSlot.instance.UpdateCursorSlot();
                            }

                            
                        }
                    }
                }
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