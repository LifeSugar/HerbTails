using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class HerbsInPort : MonoBehaviour
    {
        public List<InventorySlot> slots = new List<InventorySlot>();


        void Start()
        {
            UpdateSlots();
            ClearSlots();
            
        }
        public void ClearSlots()
        {
            foreach (var slot in slots)
            {
                slot.slotItem.Count = 0;
                slot.UpdateSlot();
            }
        }

        public void UpdateSlots()
        {
            slots.Clear();
            foreach (Transform child in transform)
            {
                InventorySlot slot = child.GetComponent<InventorySlot>();
                if (slot)
                {
                    slots.Add(slot);
                }
            }
        }

        public bool GetSlotEmptyState()
        {
            foreach (var slot in slots)
            {
                if (slot.isEmpty)
                    return true;
            }
            return false;
        }

        public InventorySlot GetFirstEmptySlot()
        {
            foreach (var slot in slots)
            {
                if (slot.isEmpty)
                    return slot;
            }
            return null;
        }

        
        public List<CraftMaterial> materialsIn = new List<CraftMaterial>();
        public void ReFreshMaterialsInPort()
        {
            materialsIn.Clear();
            foreach (var slot in slots)
            {
                
                if (slot.isEmpty)
                    continue;
                var slotItem = slot.slotItem;
                var cm = ResourceManager.instance.GetCraftMaterial(slotItem.Name);
                cm.Count = slotItem.Count;
                materialsIn.Add(cm);

            }
        }
        
    }
}