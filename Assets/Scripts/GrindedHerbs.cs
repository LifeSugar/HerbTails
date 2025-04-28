using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class GrindedHerbs : MonoBehaviour
    {
        public List<InventorySlot> grindedHerbsslots = new List<InventorySlot>();

        void Start()
        {
            UpdateGrindedHerbsslots();
        }

        public void UpdateGrindedHerbsslots()
        {
            grindedHerbsslots.Clear();

            foreach (Transform child in transform)
            {
                InventorySlot slot = child.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    grindedHerbsslots.Add(slot);
                }
            }
        }
    }
}