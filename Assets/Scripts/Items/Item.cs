using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HT
{
    [System.Serializable]
    public class Item
    {
        [field: SerializeField] public string Name { get; set; } = "Empty";
        [field: SerializeField] public Sprite Icon { get; set; } = null;
        [field: TextArea(3, 5)]
        [field: SerializeField] public string Description { get; set; } = "No Description";

        [field: SerializeField] public int Count { get; set; } = 0;
        
        
    }

    [System.Serializable]
    public class CraftMaterial : Item
    {
        [field: SerializeField] public float Weight { get; set; } = 0;
    }

    // Herb 类，每个 Herb 可以创建两个不同的 GrindedHerb
    [System.Serializable]
    public class Herb : CraftMaterial
    {

        [field: SerializeField] public GameObject Prefab { get; set; } = null;
        [field: SerializeField] public Color Color { get; set; } = Color.white; //color tint
        [field: SerializeField] public String GrindedHerb { get; set; } = null;
        [field: SerializeField] public String SlicedHerb { get; set; } = null;
    }

    // GrindedHerb 只能由特定的 Herb 生成
    [System.Serializable]
    public class GrindedHerb : CraftMaterial
    {
        [field: SerializeField] public string OringingHerb { get; set; } = null;
        
        // 无参构造函数留给 Unity 序列化用
        public GrindedHerb() { }
    }

    [System.Serializable]
    public class SlicedHerb : CraftMaterial
    {
        
    }

    // 药品
    [System.Serializable]
    public class Medicine : Item
    {
        [field: SerializeField] public Quality Quality { get; set; }
    }

    // BoilMatch 处理 CraftMaterial 的组合，并根据组合生成 Medicine
    [System.Serializable]
    public class Prescription
    {
        [field: SerializeField] public List<CraftMaterial> CraftMaterials { get; set; }
        [field: SerializeField] public List<int> Weights { get; set; } = new List<int>();
        [field: SerializeField] public Medicine ResultMedicine { get; set; }
        [field: SerializeField] public List<FirePeriod> FirePeriods { get; set; }

        public Prescription(List<CraftMaterial> craftMaterials, Medicine resultMedicine)
        {
            // 注意使用 this 区分形参与字段
            this.CraftMaterials = craftMaterials;
            this.ResultMedicine = resultMedicine;
        }

        // 用于匹配配方
        public static Medicine GetMedicineFromMaterials(
            List<CraftMaterial> materials,
            List<Prescription> knownRecipes)
        {
            foreach (var recipe in knownRecipes)
            {
                // Recipe 匹配逻辑：数量相同 && 成分相同
                if (recipe.CraftMaterials.Count == materials.Count &&
                    recipe.CraftMaterials.All(m => materials.Contains(m)))
                {
                    return recipe.ResultMedicine;
                }
            }
            return null; // 未找到匹配配方
        }

    }

    public struct FirePeriod
    {
        public FirePower FirePower;
        public float Duration;
    }

    public enum Quality
    {
        Shit = 0,
        Low = 1,
        Middle = 2,
        Good = 3,
        Excellent = 4
    }
}