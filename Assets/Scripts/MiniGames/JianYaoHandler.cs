using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Herbs
{
    public class JianYaoHandler : MonoBehaviour
    {
        [Header("正在煎药")] public bool isJianYao = false;
        [Header("扇子")] public ShanZi shanzi;

        void Start()
        {
            
        }

        public void InitializeJianyao()
        {
            isJianYao = false;
            shanzi = this.gameObject.GetComponentInChildren<ShanZi>();
        }
        
        
        public static JianYaoHandler instance { get; private set; }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("there is more than one JianYaoHandler in the scene");
            }
            instance = this;
        }

    }

}