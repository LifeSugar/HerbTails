using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Herbs
{
    public class InventorySlot : MonoBehaviour
    {
        public GridTypes gridType;
        [FormerlySerializedAs("item")] public Item slotItem;
        public TextMeshProUGUI count;
        public bool isEmpty;
        public Button slotButton;
        public Image slotImage;

        public void UpdateSlot()
        {
            if (slotItem.Count <= 0)
            {
                slotItem.Name = "_EmptySlot";
                isEmpty = true;
                count.gameObject.SetActive(false);
                slotImage.gameObject.SetActive(false);
            }
            else
            {
                isEmpty = false;
                count.gameObject.SetActive(true);
                slotImage.gameObject.SetActive(true);
                slotImage.sprite = slotItem.Icon;
                count.text = slotItem.Count.ToString();
            }
        }

        public void OnClickSlot()
        {
            if (isEmpty)
            {
                // if (!CursorSlot.instance.isEmpty)
                // {
                //     GlobalFunctions.DeepCopyItem(CursorSlot.instance.cursorItem, slotItem, false);
                //     CursorSlot.instance.cursorItem.Count -= 1;
                //     CursorSlot.instance.UpdateCursorSlot();
                //     UpdateSlot();
                //     return;
                // }
            }
            else
            {
                if (CursorSlot.instance.isEmpty)
                {
                    GlobalFunctions.DeepCopyItem(slotItem, CursorSlot.instance.cursorItem, false);
                    slotItem.Count -= 1;
                    CursorSlot.instance.previousInventorySlot = this;
                    CursorSlot.instance.UpdateCursorSlot();
                    UpdateSlot();
                    return;
                }
                else
                {
                    if (CursorSlot.instance.cursorItem.Name == slotItem.Name)
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
                        GlobalFunctions.DeepCopyItem(slotItem, CursorSlot.instance.cursorItem, false);
                        slotItem.Count -= 1;
                        CursorSlot.instance.previousInventorySlot = this;
                        CursorSlot.instance.UpdateCursorSlot();
                        UpdateSlot();
                        return;
                    }
                }
            }
        }

        void OnEnable()
        {
            slotButton = GetComponent<Button>();
            count = GetComponentInChildren<TextMeshProUGUI>();
            UpdateSlot();
            slotButton.onClick.AddListener(() => OnClickSlot());
        }
        
        

    }

    public enum GridTypes
    {
        HERBS = 0,
        GRINDEDHERBS = 1,
        MEDICINES = 2,
        HERBSINVENTORY = 3
    }

}