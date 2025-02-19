using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Herbs
{
    public class CursorSlot : MonoBehaviour
    {
        public Item cursorItem;
        private RectTransform cursorRect;
        private Image cursorImage;
        private TextMeshProUGUI cursorCount;
        public Vector2 offset = new Vector2(10f, -10f);
        public bool isEmpty = false;
        public InventorySlot previousInventorySlot; //上次点击的slot
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
            cursorRect.SetAsLastSibling(); // 确保在最上层
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, // canvas
                Input.mousePosition,
                null, // Screen Space - Overlay 模式传 null,否则传入摄像机
                out mousePosition
            );

            // 限制在屏幕范围内
            mousePosition.x = Mathf.Clamp(mousePosition.x, -Screen.width / 2f, Screen.width / 2f);
            mousePosition.y = Mathf.Clamp(mousePosition.y, -Screen.height / 2f, Screen.height / 2f);

            cursorRect.anchoredPosition = mousePosition + offset;

            if (Input.GetMouseButtonDown(1) && !isEmpty)
            {
                GameObject clickedUI = GetClickedUI();
                if (clickedUI != null && clickedUI.GetComponent<InventorySlot>())
                {
                    var slot = clickedUI.GetComponent<InventorySlot>();
                    if (slot.slotItem.Name == cursorItem.Name)
                    {
                        ReturnOneItem(slot);
                    }

                    if (slot.slotItem.GetType() == cursorItem.GetType() && slot.isEmpty)
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
                // 重置为一个新的基类 Item
                cursorItem = new Item
                {
                    Name = "_Empty",
                    Icon = null,
                    Description = "",
                    Count = 0
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
                if (!previousInventorySlot.isEmpty)
                {
                    previousInventorySlot.slotItem.Count += cursorItem.Count;
                    previousInventorySlot.UpdateSlot();
                }
                else
                {
                    GlobalFunctions.DeepCopyItem(cursorItem, previousInventorySlot.slotItem);
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
                GlobalFunctions.DeepCopyItem(cursorItem, slot.slotItem);
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
                return results[0].gameObject; // 返回最上层的 UI
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