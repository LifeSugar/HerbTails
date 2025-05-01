using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class TodaysPrescription : MonoBehaviour
    {
        public List<string> prescriptions;
        public int currentPrescription = 0;
        
        public static TodaysPrescription instance;

        void Awake()
        {
            instance = this;
        }
    }
}