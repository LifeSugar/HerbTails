using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class PrescriptionScriptableObject : ScriptableObject
    {
        public List<Prescription> prescriptions = new List<Prescription>();
    }
}