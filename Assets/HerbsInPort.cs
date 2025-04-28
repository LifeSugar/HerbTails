using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class HerbsInPort : MonoBehaviour
    {
        public List<InventorySlot> slots = new List<InventorySlot>();


        public void ClearSlots()
        {
            foreach (var slot in slots)
            {
                slot.slotItem.Count = 0;
                slot.UpdateSlot();
            }
        }

        public bool GetSlotFullState()
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty)
                    return false;
            }
            return true;
        }

        public InventorySlot GetFirstEmptySlot()
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty)
                    return slot;
            }
            return null;
        }
        
    }
}