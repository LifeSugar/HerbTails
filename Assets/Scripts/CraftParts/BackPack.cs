using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class BackPack : MonoBehaviour
    {
        public List<InventorySlot> herbSlots;
        public GameObject herbsBag;
        
        public static BackPack instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            for (int i = herbsBag.transform.childCount - 1; i >= 0; i--)
            {
                InventorySlot slot = herbsBag.transform.GetChild(i).GetComponent<InventorySlot>();
                herbSlots.Add(slot);
            }
        }

        public InventorySlot GetFirstHerbsSlotByName(string name)
        {
            foreach (var a in herbSlots)
            {
                if (a.slotItem.Name == name)
                {
                    return a;
                }
            }
            
            return null;
        }

        public InventorySlot GetFirstEmptySlot()
        {
            foreach (var a in herbSlots)
            {
                if (a.slotItem == null)
                {
                    return a;
                }
            }
            return null;
        }
    }
}