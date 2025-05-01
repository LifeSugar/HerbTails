using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HT
{
    public class InventorySlot : MonoBehaviour,  IPointerEnterHandler, IPointerExitHandler
    {
        
        // 移除了原来的 GridTypes slotGridType 字段
        public UISlot slotItem;
        public TextMeshProUGUI count;
        public bool isEmpty;
        public Button slotButton;
        public Image slotImage;
        
        public RectTransform description;
        public TextMeshProUGUI descriptionText;

        public void UpdateSlot()
        {
            if (slotItem.Count <= 0)
            {
                slotItem.Name = "_EmptySlot";
                isEmpty = true;
                count.gameObject.SetActive(false);
                slotImage.gameObject.SetActive(false);
                descriptionText.text = "";
            }
            else
            {
                isEmpty = false;
                count.gameObject.SetActive(true);
                slotImage.gameObject.SetActive(true);
                slotImage.sprite = slotItem.Icon;
                count.text = slotItem.Count.ToString();
                descriptionText.text = slotItem.Name;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (description != null && isEmpty == false)
            {
                // this.GetComponent<RectTransform>().SetAsLastSibling();
                description.gameObject.SetActive(true);
                // description.SetAsLastSibling(); // 置顶
            }
                
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (description != null)
                description.gameObject.SetActive(false);
        }


        public bool readOnly = false;
        public void OnClickSlot()
        {
            if (isEmpty)
            {
                if (CursorSlot.instance.isEmpty)
                {
                    return;
                }
                else if (CursorSlot.instance.cursorItem.GridType == slotItem.GridType && !readOnly)
                {
                    Utility.DeepCopyUISlot(CursorSlot.instance.cursorItem, slotItem, true ,false);
                    CursorSlot.instance.cursorItem.Count = 0;
                    CursorSlot.instance.UpdateCursorSlot();
                    UpdateSlot();
                    return;
                }
            }
            else
            {
                if (CursorSlot.instance.isEmpty)
                {
                    // 使用新的深拷贝方法 DeepCopyUISlot 复制当前 slotItem 到鼠标上
                    CursorSlot.instance.cursorItem = Utility.DeepCopyUISlot(slotItem, false);
                    slotItem.Count -= 1;
                    CursorSlot.instance.previousInventorySlot = this;
                    CursorSlot.instance.UpdateCursorSlot();
                    UpdateSlot();
                    return;
                }
                else if (!readOnly)
                {
                    // 通过比较 UISlot 内的 GridType 判断两者是否匹配
                    bool match = CheckCursorItemType(CursorSlot.instance.cursorItem);

                    if (CursorSlot.instance.cursorItem.Name == slotItem.Name && match)
                    {
                        CursorSlot.instance.cursorItem.Count += 1;
                        slotItem.Count -= 1;
                        CursorSlot.instance.previousInventorySlot = this;
                        CursorSlot.instance.UpdateCursorSlot();
                        UpdateSlot();
                        return;
                    }
                    else
                    {
                        CursorSlot.instance.ReturnItems();
                        CursorSlot.instance.cursorItem = Utility.DeepCopyUISlot(slotItem, false);
                        slotItem.Count -= 1;
                        CursorSlot.instance.previousInventorySlot = this;
                        CursorSlot.instance.UpdateCursorSlot();
                        UpdateSlot();
                        return;
                    }
                }
            }
        }

        // 优化后的类型匹配：直接比较 cursorItem.GridType 与 slotItem.GridType 是否一致
        bool CheckCursorItemType(UISlot cursorItem)
        {
            return cursorItem.GridType == slotItem.GridType;
        }

        void OnEnable()
        {

            if (!readOnly)
            {
                slotButton = GetComponent<Button>();
                count = GetComponentInChildren<TextMeshProUGUI>();
                UpdateSlot();
                slotButton.onClick.AddListener(() => OnClickSlot());
            }
        }

        void Start()
        {
            if (slotItem.Count > 0)
            {
                var item = ResourceManager.instance.GetItem(slotItem.Name);
                if (item != null)
                {
                    slotItem.Name = item.Name;
                    slotItem.Icon = item.Icon;
                }
                UpdateSlot();
            }
        }
    }

    // UISlot 类只保留了 Name、Icon、Count 及 GridType 四个属性
    [System.Serializable]
    public class UISlot
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Sprite Icon { get; set; }
        [field: SerializeField] public int Count { get; set; }
        [field: SerializeField] public GridTypes GridType { get; set; }
    }

    public enum GridTypes
    {
        HERBS = 0,
        GRINDEDHERBS = 1,
        MEDICINES = 2,
        HERBSINVENTORY = 3
    }
}
