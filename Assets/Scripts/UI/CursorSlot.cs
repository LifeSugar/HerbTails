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
        public static CursorSlot instance{get; private set;}

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
                    if(slot.slotItem.Name == cursorItem.Name || slot.isEmpty)
                        ReturnOneItem(slot);
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
                cursorItem.Name = "_Empty";
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
    }
}