using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Herbs
{
    [System.Serializable]
    public class Item
    {
        [field: SerializeField] public string Name { get; set; } = "Empty";
        [field: SerializeField] public Sprite Icon { get; set; } = null;
        [field: TextArea(3, 5)]
        [field: SerializeField] public string Description { get; set; } = "No Description";

        [field: SerializeField] public int Count { get; set; } = 0;
        
        [field: SerializeField] public string ModelPath { get; set; } = null;
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
        [field: SerializeField] public GrindedHerb CoarseGrinded { get; set; }
        [field: SerializeField] public GrindedHerb FineGrinded { get; set; }
    }

    // GrindedHerb 只能由特定的 Herb 生成
    [System.Serializable]
    public class GrindedHerb : CraftMaterial
    {
        // 如果需要在 Inspector 上可视化 SourceHerb，就用 [field: SerializeField]
        // 如果只需要脚本内逻辑，可以去掉或改成 private
        [field: SerializeField] public Herb SourceHerb { get; private set; }

        // 如果你想在脚本里指定 SourceHerb，可加一个方法或构造来设置
        public GrindedHerb(Herb sourceHerb)
        {
            SourceHerb = sourceHerb;
        }

        // 无参构造函数留给 Unity 序列化用
        public GrindedHerb() { }
    }

    // 药品
    [System.Serializable]
    public class Medicine : Item
    {
        // 在这里可加上任何药品特有的字段
    }

    // BoilMatch 处理 CraftMaterial 的组合，并根据组合生成 Medicine
    [System.Serializable]
    public class Prescription
    {
        [field: SerializeField] public List<CraftMaterial> CraftMaterials { get; set; }
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
}