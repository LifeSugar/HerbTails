using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace HT
{
    public class PrescriptionPanel : MonoBehaviour
    {
        public Image MedicineIcon;
        public TextMeshProUGUI MedicineName;
        public TextMeshProUGUI MedicineDescription;
        public RectTransform Materials;
        public RectTransform FirePeriod;
        
        public List<Sprite> FirePeriodSprites;

        public GameObject iconText;
        
        public static PrescriptionPanel Instance;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // var pres = ResourceManager.instance.GetPrescription("TestPrescription");
            // SetUpPrescriptionPanel(pres);
            // var cr = ResourceManager.instance.GetCraftMaterial("GrindedRedHerb");
            // Debug.Log(cr.Name);
        }

        public void SetUpPrescriptionPanel(Prescription pres)
        {
            for (int i = Materials.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(Materials.GetChild(i).gameObject);
            }

            for (int i = FirePeriod.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(FirePeriod.GetChild(i).gameObject);
            }
            
            MedicineIcon.sprite = pres.ResultMedicine.Icon;
            MedicineName.text = pres.ResultMedicine.Name;
            MedicineDescription.text = pres.ResultMedicine.Description;

            foreach (var f in pres.FirePeriods)
            {
                var a = Instantiate(iconText);
                var image = a.GetComponentInChildren<Image>();
                var textMeshProUGUI = a.GetComponentInChildren<TextMeshProUGUI>();
                if (f.FirePower == FirePower.SMALL)
                {
                    image.sprite = FirePeriodSprites[0];
                }
                if (f.FirePower == FirePower.MIDDLE)
                {
                    image.sprite = FirePeriodSprites[1];
                }

                if (f.FirePower == FirePower.LARGE)
                {
                    image.sprite = FirePeriodSprites[2];
                }

                if (f.FirePower == FirePower.NONE)
                {
                    image.enabled = false;
                }
                textMeshProUGUI.text = f.FirePower.ToString() + ": " + f.Duration.ToString() + "secs";
                a.transform.SetParent(FirePeriod.transform, false);
            }

            for (int i = 0; i < pres.CraftMaterials.Count; i++)
            {
                var a = Instantiate(iconText);
                var image = a.GetComponentInChildren<Image>();
                var textMeshProUGUI = a.GetComponentInChildren<TextMeshProUGUI>();
                image.sprite = pres.CraftMaterials[i].Icon;
                textMeshProUGUI.text = pres.CraftMaterials[i].Name + ": " + pres.Weights[i].ToString() + "g";
                a.transform.SetParent(Materials.transform, false);
            }
        }

    }
}