using System.Collections;
using System.Collections.Generic;
using NPOI.SS.Formula.Atp;
using UnityEngine;
using UnityEngine.UI;


namespace HT
{
    public class ClickInteractions : MonoBehaviour
    {
        [Header("拿取药方的按钮")] public CanvasPopIn takePrescriptionInteraction;
        [Header("ZhuoYao")] public CanvasPopIn makeMedicineInteraction;
        [Header("kanshu")] public CanvasPopIn libraryInteraction;
        
        public static ClickInteractions instance;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            
        }

        
        CanvasPopIn lastPopIn = null;
        public void Tick()
        {
            Ray ray = Utility.GetRayFromRealCamScreenPos(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.TryGetComponent<CanvasPopIn>(out var canvasPopIn))
                {
                    if (lastPopIn != null && lastPopIn != canvasPopIn)
                    {
                        lastPopIn.image.color = Color.white;
                    }
                    canvasPopIn.image.color = new Color(1.0f, 0.95f, 0.8f, 1.0f);
                    lastPopIn = canvasPopIn;
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (canvasPopIn == takePrescriptionInteraction)
                        {
                            
                        }
                        else if (canvasPopIn == libraryInteraction)
                        {
                            
                        }
                        else if (canvasPopIn == makeMedicineInteraction)
                        { 
                            InputHandler.instance.SwitchGameState(GameState.ZHUAYAO);
                        }
                    }
                }
            }
            else
            {
                if (lastPopIn != null)
                    lastPopIn.image.color = Color.white;
            }
        }

        public void HideInteractions()
        {
            this.gameObject.SetActive(false);
        }

        public void RevealInteractions()
        {
            this.gameObject.SetActive(true);
        }
    }

}