using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class MedicineScriptableObject : ScriptableObject
    {
        public List<Medicine> medicines = new List<Medicine>();
    }
}