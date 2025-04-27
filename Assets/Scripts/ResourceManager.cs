using System;
using System.Collections.Generic;
using UnityEngine;

namespace HT
{
    public class ResourceManager : MonoBehaviour
    {
        // 用于缓存各类数据项的名称和在列表中的索引
        Dictionary<string, int> item_ids = new Dictionary<string, int>();
        Dictionary<string, int> herb_ids = new Dictionary<string, int>();
        Dictionary<string, int> grindedHerb_ids = new Dictionary<string, int>();
        Dictionary<String, int> slicedHerb_ids = new Dictionary<String, int>();
        Dictionary<string, int> craftMaterial_ids = new Dictionary<string, int>();
        Dictionary<string, int> medicine_ids = new Dictionary<string, int>();

        // 单例模式，方便其他脚本通过 ResourceManager.instance 调用接口
        public static ResourceManager instance;

        private void Awake()
        {
            // 初始化单例实例
            instance = this;

            // 加载各类型的资源数据，并将名称与索引缓存到字典中
            LoadItems();
            LoadHerbs();
            LoadGrindedHerbs();
            LoadCraftMaterials();
            LoadMedicines();
        }

        #region Items
        /// <summary>
        /// 从 Resources 文件夹中加载 ItemScriptableObject 资源，并将每个 Item 的 Name 和索引缓存到字典中
        /// </summary>
        void LoadItems()
        {
            ItemScriptableObject obj = Resources.Load("HT.ItemScriptableObject") as ItemScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.ItemScriptableObject could not be loaded!");
                return;
            }

            // 遍历 items 列表
            for (int i = 0; i < obj.items.Count; i++)
            {
                // 如果字典中已存在该名称则输出重复警告，否则添加到字典
                if (item_ids.ContainsKey(obj.items[i].Name))
                {
                    Debug.Log("Item is a duplicate: " + obj.items[i].Name);
                }
                else
                {
                    item_ids.Add(obj.items[i].Name, i);
                }
            }
        }

        /// <summary>
        /// 根据名称获取 Item 对象
        /// </summary>
        public Item GetItem(string name)
        {
            // 从 Resources 中重新加载资源
            ItemScriptableObject obj = Resources.Load("HT.ItemScriptableObject") as ItemScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.ItemScriptableObject could not be loaded!");
                return null;
            }

            // 根据缓存字典获取对应的索引
            int index = GetItemIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Item not found: " + name);
                return null;
            }

            // 返回对应索引的 Item 对象
            return obj.items[index];
        }

        /// <summary>
        /// 根据名称从缓存中获取 Item 的索引
        /// </summary>
        int GetItemIdFromString(string name)
        {
            int index = -1;
            if (item_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        #endregion

        #region Herbs
        /// <summary>
        /// 从 Resources 文件夹中加载 HerbScriptableObject 资源，并缓存每个 Herb 的 Name 和索引
        /// </summary>
        void LoadHerbs()
        {
            HerbScriptableObject obj = Resources.Load("HT.HerbScriptableObject") as HerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.HerbScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.herbs.Count; i++)
            {
                if (herb_ids.ContainsKey(obj.herbs[i].Name))
                {
                    Debug.Log("Herb is a duplicate: " + obj.herbs[i].Name);
                }
                else
                {
                    herb_ids.Add(obj.herbs[i].Name, i);
                }
            }
        }

        /// <summary>
        /// 根据名称获取 Herb 对象
        /// </summary>
        public Herb GetHerb(string name)
        {
            HerbScriptableObject obj = Resources.Load("HT.HerbScriptableObject") as HerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.HerbScriptableObject could not be loaded!");
                return null;
            }

            int index = GetHerbIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Herb not found: " + name);
                return null;
            }
            return obj.herbs[index];
        }

        /// <summary>
        /// 根据名称从缓存中获取 Herb 的索引
        /// </summary>
        int GetHerbIdFromString(string name)
        {
            int index = -1;
            if (herb_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        #endregion

        #region Grinded Herbs
        /// <summary>
        /// 加载 GrindedHerbScriptableObject 资源，并缓存每个研磨后草药的 Name 与索引
        /// </summary>
        void LoadGrindedHerbs()
        {
            GrindedHerbScriptableObject obj = Resources.Load("HT.GrindedHerbScriptableObject") as GrindedHerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.GrindedHerbScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.grindedHerbs.Count; i++)
            {
                if (grindedHerb_ids.ContainsKey(obj.grindedHerbs[i].Name))
                {
                    Debug.Log("Grinded Herb is a duplicate: " + obj.grindedHerbs[i].Name);
                }
                else
                {
                    grindedHerb_ids.Add(obj.grindedHerbs[i].Name, i);
                }
            }
        }

        /// <summary>
        /// 根据名称获取 GrindedHerb 对象
        /// </summary>
        public GrindedHerb GetGrindedHerb(string name)
        {
            GrindedHerbScriptableObject obj = Resources.Load("HT.GrindedHerbScriptableObject") as GrindedHerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.GrindedHerbScriptableObject could not be loaded!");
                return null;
            }

            int index = GetGrindedHerbIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Grinded Herb not found: " + name);
                return null;
            }
            return obj.grindedHerbs[index];
        }

        /// <summary>
        /// 根据名称从缓存中获取 GrindedHerb 的索引
        /// </summary>
        int GetGrindedHerbIdFromString(string name)
        {
            int index = -1;
            if (grindedHerb_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        #endregion
        
        #region SlicedHerbs
        
        void LoadSlicedHerbs()
        {
            SlicedHerbScriptableObject obj = Resources.Load("HT.SlicedHerbScriptableObject") as SlicedHerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.GrindedHerbScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.slicedHerbs.Count; i++)
            {
                if (grindedHerb_ids.ContainsKey(obj.slicedHerbs[i].Name))
                {
                    Debug.Log("Grinded Herb is a duplicate: " + obj.slicedHerbs[i].Name);
                }
                else
                {
                    grindedHerb_ids.Add(obj.slicedHerbs[i].Name, i);
                }
            }
        }
        
        public SlicedHerb GetSlicedHerb(string name)
        {
            SlicedHerbScriptableObject obj = Resources.Load("HT.GrindedHerbScriptableObject") as SlicedHerbScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.SlicedHerbScriptableObject could not be loaded!");
                return null;
            }

            int index = GetSlicedHerbIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Grinded Herb not found: " + name);
                return null;
            }
            return obj.slicedHerbs[index];
        }
        
        int GetSlicedHerbIdFromString(string name)
        {
            int index = -1;
            if (grindedHerb_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        
        
        #endregion

        #region Craft Materials
        /// <summary>
        /// 加载 CraftMaterialScriptableObject 资源，并缓存每个制作材料的 Name 与索引
        /// </summary>
        void LoadCraftMaterials()
        {
            CraftMaterialScriptableObject obj = Resources.Load("HT.CraftMaterialScriptableObject") as CraftMaterialScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.CraftMaterialScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.craftMaterials.Count; i++)
            {
                if (craftMaterial_ids.ContainsKey(obj.craftMaterials[i].Name))
                {
                    Debug.Log("Craft Material is a duplicate: " + obj.craftMaterials[i].Name);
                }
                else
                {
                    craftMaterial_ids.Add(obj.craftMaterials[i].Name, i);
                }
            }
        }

        /// <summary>
        /// 根据名称获取 CraftMaterial 对象
        /// </summary>
        public CraftMaterial GetCraftMaterial(string name)
        {
            CraftMaterialScriptableObject obj = Resources.Load("HT.CraftMaterialScriptableObject") as CraftMaterialScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.CraftMaterialScriptableObject could not be loaded!");
                return null;
            }

            int index = GetCraftMaterialIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Craft Material not found: " + name);
                return null;
            }
            return obj.craftMaterials[index];
        }

        /// <summary>
        /// 根据名称从缓存中获取 CraftMaterial 的索引
        /// </summary>
        int GetCraftMaterialIdFromString(string name)
        {
            int index = -1;
            if (craftMaterial_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        #endregion

        #region Medicines
        /// <summary>
        /// 加载 MedicineScriptableObject 资源，并缓存每个药品的 Name 与索引
        /// </summary>
        void LoadMedicines()
        {
            MedicineScriptableObject obj = Resources.Load("HT.MedicineScriptableObject") as MedicineScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.MedicineScriptableObject could not be loaded!");
                return;
            }

            for (int i = 0; i < obj.medicines.Count; i++)
            {
                if (medicine_ids.ContainsKey(obj.medicines[i].Name))
                {
                    Debug.Log("Medicine is a duplicate: " + obj.medicines[i].Name);
                }
                else
                {
                    medicine_ids.Add(obj.medicines[i].Name, i);
                }
            }
        }

        /// <summary>
        /// 根据名称获取 Medicine 对象
        /// </summary>
        public Medicine GetMedicine(string name)
        {
            MedicineScriptableObject obj = Resources.Load("HT.MedicineScriptableObject") as MedicineScriptableObject;
            if (obj == null)
            {
                Debug.Log("Herbs.MedicineScriptableObject could not be loaded!");
                return null;
            }

            int index = GetMedicineIdFromString(name);
            if (index == -1)
            {
                Debug.Log("Medicine not found: " + name);
                return null;
            }
            return obj.medicines[index];
        }

        /// <summary>
        /// 根据名称从缓存中获取 Medicine 的索引
        /// </summary>
        int GetMedicineIdFromString(string name)
        {
            int index = -1;
            if (medicine_ids.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }
        #endregion
    }
}
