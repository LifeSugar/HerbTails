using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR          // Editor APIs only compile here
using UnityEditor;
#endif

namespace HT
{
    public class Cangku : MonoBehaviour
    {
        [Header("Slot prefab & count")]
        public GameObject prefab;
        public int createCount = 10;

        [Header("Runtime-populated")]
        public List<InventorySlot> CangkuSlots = new();

        // Path is editor-only, so keep it inside the guard
#if UNITY_EDITOR
        private static readonly string herbPath =
            "Assets/Resources/HT.HerbScriptableObject.asset";
#endif

        /// <summary>Editor-only helper to populate the warehouse slots.</summary>
        [ContextMenu("Fill Cangkus")]
        public void FillCangkus()
        {
#if UNITY_EDITOR

            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            CangkuSlots.Clear();


            var herbSO = AssetDatabase.LoadAssetAtPath<HerbScriptableObject>(herbPath);
            if (herbSO == null)
            {
                Debug.LogError($"Failed to load HerbScriptableObject at {herbPath}");
                return;
            }


            PopulateSlots(herbSO);
#else
            Debug.LogWarning("FillCangkus is editor-only and was called at runtime.");
#endif
        }

#if UNITY_EDITOR
        private void PopulateSlots(HerbScriptableObject herbSO)
        {
            foreach (var herb in herbSO.herbs)
            {
                var slotInstance = Instantiate(prefab, transform);
                slotInstance.transform.localScale = Vector3.one;

                var slot = slotInstance.GetComponent<InventorySlot>();
                var tempItem = Utility.CreateUISlotFromItem(
                    herb, createCount, GridTypes.HERBS);

                slot.slotItem = new UISlot
                {
                    Name     = tempItem.Name,
                    Icon     = tempItem.Icon,
                    Count    = createCount,
                    GridType = GridTypes.HERBS
                };
                slot.UpdateSlot();
                CangkuSlots.Add(slot);
            }
        }
#endif

        /* ---------- Runtime helpers (unchanged) ---------- */
        public InventorySlot GetFirstHerbsSlotByName(string name)
        {
            foreach (var a in CangkuSlots)
                if (a.slotItem?.Name == name) return a;
            return null;
        }

        public InventorySlot GetFirstEmptySlot()
        {
            foreach (var a in CangkuSlots)
                if (a.slotItem == null) return a;
            return null;
        }
    }
}
