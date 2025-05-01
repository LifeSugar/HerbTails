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
        //
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
        [field: SerializeField] public String Name { get; set; } = null;
        [field: SerializeField] public List<CraftMaterial> CraftMaterials { get; set; }
        [field: SerializeField] public List<int> Weights { get; set; } = new List<int>();
        [field: SerializeField] public Medicine ResultMedicine { get; set; }
        [field: SerializeField] public List<FirePeriod> FirePeriods { get; set; }

        public Prescription()
        {
            
        }

        // 用于匹配配方
        // public static Medicine GetMedicineFromMaterials(
        //     List<CraftMaterial> materials,
        //     List<Prescription> knownRecipes)
        // {
        //     foreach (var recipe in knownRecipes)
        //     {
        //         // Recipe 匹配逻辑：数量相同 && 成分相同
        //         if (recipe.CraftMaterials.Count == materials.Count &&
        //             recipe.CraftMaterials.All(m => materials.Contains(m)))
        //         {
        //             return recipe.ResultMedicine;
        //         }
        //     }
        //     return null; // 未找到匹配配方
        // }
        
        

        public static Medicine GetMedicineFromMaterials(
            List<CraftMaterial> materials,
            List<Prescription>  knownRecipes)
        {
            // 把玩家实际投放的材料先汇总成  <Name, Count>
            var materialDict = materials
                .GroupBy(m => m.Name)                       // ← 只看 Name
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var recipe in knownRecipes)
            {
                //数量总和不同就肯定不匹配
                if (recipe.CraftMaterials.Count != materials.Count) continue;

                //把配方材料也汇总成 <Name, Count>
                var recipeDict = recipe.CraftMaterials
                    .GroupBy(m => m.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 两张“统计表”必须完全一致
                bool same =
                    recipeDict.Count == materialDict.Count &&        // 键数量一样
                    recipeDict.All(kvp =>
                        materialDict.TryGetValue(kvp.Key, out int c) // 同名材料存在
                        && c == kvp.Value);                          // 且数量一致

                if (same)
                    return recipe.ResultMedicine;
            }

            return null; // 未找到匹配配方
        }

        
        public FirePower GetFirePowerByElapsedTime(float elapsedTime)
        {
            if (FirePeriods == null || FirePeriods.Count == 0 || elapsedTime < 0f)
                return 0f;

            float accumulated = 0f;

            foreach (var period in FirePeriods)
            {
                accumulated += period.Duration;

                if (elapsedTime < accumulated)     // 落在这一段
                    return period.FirePower;
            }

            // 超出所有时间段——配方已烧完
            return 0;      
        }

        

        public int GetWeightBias(List<int> weights)
        {
            if (weights.Count != this.Weights.Count)
            {
                return 100;
            }
            else
            {
                int result = 0;
                for (int i = 0; i < weights.Count; i++)
                {
                    result += Mathf.Abs(this.Weights[i] - weights[i]);
                }
                return result;
            }
        }

        public float GetTotalDuration()
        {
            float result = 0f;
            foreach (var d in FirePeriods)
            {
                result += d.Duration;
            }
            return result;
        }
        

        public void ReorderInputsByRecipeOrder(List<CraftMaterial> materials,
            List<int>          weights)
        {
            if (materials == null || weights == null)
                throw new ArgumentNullException("materials / weights 不能为 null");

            if (materials.Count != weights.Count)
                throw new ArgumentException("materials 与 weights 长度不一致");

            if (materials.Count != CraftMaterials.Count)
                throw new ArgumentException("输入材料数量与配方不一致");


            var name2Index = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < CraftMaterials.Count; i++)
                name2Index[ CraftMaterials[i].Name ] = i;

            var sortedMaterials = new CraftMaterial[materials.Count];
            var sortedWeights   = new int[weights.Count];

            for (int i = 0; i < materials.Count; i++)
            {
                int target = name2Index[ materials[i].Name ];
                sortedMaterials[target] = materials[i];
                sortedWeights[target]   = weights[i];
            }


            for (int i = 0; i < materials.Count; i++)
            {
                materials[i] = sortedMaterials[i];
                weights[i]   = sortedWeights[i];
            }
        }



    }

    [System.Serializable]
    public struct FirePeriod
    {
        public FirePower FirePower;
        public float Duration;
    }

    public enum Quality
    {
        Shit = 0,
        Low = 1,
        Good = 2,
        Excellent = 3
    }
}