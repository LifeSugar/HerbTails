using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT
{
    public class Cangku : MonoBehaviour
    {
        public GameObject prefab;
        public int createCount = 10;
        
        public List<InventorySlot> CangkuSlots;

        private static string herbpath = "Assets/Resources/HT.HerbScriptableObject.asset";

        public void FillCangkus()
        {
            // 清空所有子物体
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            CangkuSlots.Clear();

            var herbSO = AssetDatabase.LoadAssetAtPath<HerbScriptableObject>(herbpath) as HerbScriptableObject;
            var herbs = herbSO.herbs;

            foreach (var herb in herbs)
            {
                var slotinstance = GameObject.Instantiate(prefab);
                var slot = slotinstance.GetComponent<InventorySlot>();
                var slotItem = Utility.CreateUISlotFromItem(herb, createCount, GridTypes.HERBS);
                slot.slotItem = new UISlot()
                {
                    Name = slotItem.Name,
                    Icon = slotItem.Icon,
                    Count = createCount,
                    GridType = GridTypes.HERBS,
                };
                slot.UpdateSlot();
                slotinstance.transform.SetParent(transform);
                slotinstance.transform.localScale = Vector3.one;
                CangkuSlots.Add(slotinstance.GetComponent<InventorySlot>());
            }
        }
        
        public InventorySlot GetFirstHerbsSlotByName(string name)
        {
            foreach (var a in CangkuSlots)
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
            foreach (var a in CangkuSlots)
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