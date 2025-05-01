using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HT
{
    public class ResetZhuaYao :MonoBehaviour
    {
        public GameObject zhuaYao;
        public Button resetZhuaYaoBtn;
        public Button StartZhuaYaoBtn;
        
        public RectTransform ZhuaYaoPanel;

        public TextMeshProUGUI weightText;
        
        public static ResetZhuaYao Instance;

        void Awake()
        {
            Instance = this;
        }

        public void RessetZhuaYao()
        {
            var oldZhuaYao = transform.GetChild(0).GetComponent<ZhuaYaoHandler>();

            if (oldZhuaYao.isEmpty())
            {
                oldZhuaYao.isMeasuring = false;
                DestroyImmediate(oldZhuaYao.gameObject);
                GameObject newZhuaYao =  Instantiate(this.zhuaYao);
                newZhuaYao.name = "ZhuaYao";
                newZhuaYao.transform.SetParent(this.transform);
                newZhuaYao.transform.localPosition = Vector3.zero;
                
            }
            
        }

        private void Start()
        {
            resetZhuaYaoBtn.gameObject.SetActive(false);
            resetZhuaYaoBtn.onClick.AddListener(RessetZhuaYao);
        }

        void Update()
        {
            var a = ZhuaYaoHandler.instance.isEmpty();
            if (a)
            {
                resetZhuaYaoBtn.gameObject.SetActive(true);
            }
            else
            {
                resetZhuaYaoBtn.gameObject.SetActive(false);
            }
        }
    }
}