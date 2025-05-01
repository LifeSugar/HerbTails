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
        
        public PrescriptionPanel prescriptionPanel;
        
        public Canvas LibraryCanvas;
        public Button Button;
        

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            
        }

        void HandleTakePrescription()
        {
            var presname = TodaysPrescription.instance.prescriptions[TodaysPrescription.instance.currentPrescription];
            var pres = ResourceManager.instance.GetPrescription(presname);
            prescriptionPanel.gameObject.SetActive(true);
            prescriptionPanel.SetUpPrescriptionPanel(pres);
            JianYaoHandler.instance.testPrescription = pres;
            
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
                            
                            HandleTakePrescription();
                            
                        }
                        else if (canvasPopIn == libraryInteraction)
                        {
                            LibraryCanvas.gameObject.SetActive(true);
                            Button.gameObject.SetActive(false);
                            InputHandler.instance.inDialogue = true;
                        }
                        else if (canvasPopIn == makeMedicineInteraction)
                        { 
                            if (InputHandler.instance.firstZhiyao)
                            {
                                InputHandler.instance.inDialogue = true;
                                DialogueManager.instance.currentData = InputHandler.instance.dialogueData1;
                                DialogueManager.instance.StartDialogue();
                                InputHandler.instance.inDialogue = true;
                            }
                            InputHandler.instance.SwitchGameState(GameState.ZHUAYAO);
                            this.gameObject.SetActive(false);
                            InputHandler.instance.firstZhiyao = false;
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