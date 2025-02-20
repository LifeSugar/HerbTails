using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace HT
{
    public class CursorSlot : MonoBehaviour
    {
        public UISlot cursorItem;
        private RectTransform cursorRect;
        private Image cursorImage;
        private TextMeshProUGUI cursorCount;
        public Vector2 offset = new Vector2(10f, -10f);
        public bool isEmpty = false;
        public InventorySlot previousInventorySlot; // 记录上次点击的 InventorySlot
        public static CursorSlot instance { get; private set; }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogError("More than one instance of SlotManager");
            }
            else
            {
                instance = this;
            }
        }

        void Start()
        {
            cursorRect = GetComponent<RectTransform>();
            cursorImage = GetComponentInChildren<Image>();
            cursorCount = GetComponentInChildren<TextMeshProUGUI>();
            UpdateCursorSlot();
        }

        void Update()
        {
            // 始终保持在最上层显示
            cursorRect.SetAsLastSibling();
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                Input.mousePosition,
                null,
                out mousePosition
            );

            mousePosition.x = Mathf.Clamp(mousePosition.x, -Screen.width / 2f, Screen.width / 2f);
            mousePosition.y = Mathf.Clamp(mousePosition.y, -Screen.height / 2f, Screen.height / 2f);
            cursorRect.anchoredPosition = mousePosition + offset;

            if (Input.GetMouseButtonDown(1) && !isEmpty)
            {
                GameObject clickedUI = GetClickedUI();
                if (clickedUI != null && clickedUI.GetComponent<InventorySlot>())
                {
                    var slot = clickedUI.GetComponent<InventorySlot>();
                    if ((cursorItem.GridType == slot.slotItem.GridType) && (slot.isEmpty || slot.slotItem.Name == cursorItem.Name))
                    {
                        ReturnOneItem(slot);
                    }
                }
                else
                {
                    ReturnItems();
                }
            }
        }

        public void UpdateCursorSlot()
        {
            if (cursorItem.Count <= 0)
            {
                cursorItem = new UISlot
                {
                    Name = "_Empty",
                    Icon = null,
                    Count = 0,
                    GridType = GridTypes.HERBS // 默认值
                };

                cursorImage.gameObject.SetActive(false);
                cursorCount.gameObject.SetActive(false);
                isEmpty = true;
            }
            else
            {
                cursorImage.sprite = cursorItem.Icon;
                cursorCount.text = cursorItem.Count.ToString();
                cursorImage.gameObject.SetActive(true);
                cursorCount.gameObject.SetActive(true);
                isEmpty = false;
            }
        }

        public void ReturnItems()
        {
            if (!isEmpty)
            {
                ReturnItemsVisual();
                if (!previousInventorySlot.isEmpty)
                {
                    previousInventorySlot.slotItem.Count += cursorItem.Count;
                    previousInventorySlot.UpdateSlot();
                }
                else
                {
                    GlobalFunctions.DeepCopyUISlot(cursorItem, previousInventorySlot.slotItem, true, false);
                    previousInventorySlot.UpdateSlot();
                }
                cursorItem.Count = 0;
                UpdateCursorSlot();
            }
        }

        void ReturnOneItem(InventorySlot slot)
        {
            if (slot.isEmpty)
            {
                GlobalFunctions.DeepCopyUISlot(cursorItem, slot.slotItem, true, false);
                cursorItem.Count = 0;
                slot.UpdateSlot();
                UpdateCursorSlot();
            }
            else
            {
                slot.slotItem.Count += 1;
                cursorItem.Count -= 1;
                slot.UpdateSlot();
                UpdateCursorSlot();
            }
        }

        void ReturnItemsVisual()
        {
            GameObject visual = new GameObject("returnicon");

            // 设置为当前物体的子级，保持局部坐标不变
            visual.transform.SetParent(transform, false);

            // 添加 RectTransform 组件，并获取当前物体的 RectTransform
            RectTransform newRect = visual.AddComponent<RectTransform>();
            RectTransform selfRect = GetComponent<RectTransform>();

            // 同步一些布局属性，但不直接复制 anchoredPosition
            // newRect.anchorMin = selfRect.anchorMin;
            // newRect.anchorMax = selfRect.anchorMax;
            // newRect.sizeDelta = selfRect.sizeDelta;
            // newRect.pivot = selfRect.pivot;

            // 直接将局部位置设为零，确保新物体起始位置与父物体重合
            newRect.localPosition = Vector3.zero;

            Image image = visual.AddComponent<Image>();
            image.sprite = cursorItem.Icon;
            image.preserveAspect = true;
            image = cursorImage;

            // 目标 UI 元素的转换：从世界坐标转换为 newRect 所在父级的局部坐标
            RectTransform targetRect = previousInventorySlot.GetComponent<RectTransform>();
            Vector3 targetWorldPos = targetRect.position;
            Vector3 targetLocalPos3D = newRect.parent.InverseTransformPoint(targetWorldPos);
            Vector2 targetLocalPos = new Vector2(targetLocalPos3D.x, targetLocalPos3D.y);

            // 执行动画：将 anchoredPosition 从初始 0 动画到目标位置
            newRect.DOAnchorPos(targetLocalPos, 0.1f)
                .SetEase(Ease.Linear)
                .OnComplete(() => Destroy(visual));
            
        }

        GameObject GetClickedUI()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                return results[0].gameObject;
            }
            return null;
        }
        /*
         符合要求的slot
         鼠标上没有物品，左键点击一个非空的 Slot
        从该格子拿 1 个物品到鼠标上（Slot数量减 1，鼠标数量增 1），并将该格子记录为“previousInventorySlot”。

        鼠标上有物品，左键点击一个非空的 Slot
        如果与鼠标上物品同种，叠加拿 1 个（Slot 数量减 1，鼠标数量加 1）。
        如果不同种，则先把鼠标上的物品还给“previousInventorySlot”，再从这个新点击的格子中拿 1 个到鼠标。

        鼠标上有物品，左键点击一个空的 Slot
        默认啥也不做。

        鼠标上有物品，右键点击
        如果点击到的 UI 是一个 InventorySlot，并且这个格子是空或相同物品，则只放入 1 个（或整堆，取决于逻辑）；
        如果没点到合适的格子或点到其它地方，就把整堆都还回“previousInventorySlot”。

        鼠标上没有物品，右键点击
        不会进入上述逻辑
        。
         */
    }
}


