using UnityEngine;
using UnityEditor;

namespace HT
{
    public class ScriptableObjectManager : MonoBehaviour
    {
        //创建指定的资源
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            // 检查 Resources 文件夹中是否已有同类型的资源
            if (Resources.Load(typeof(T).ToString()) == null)
            {
                // 如果未找到同类型资源，则生成资源的唯一路径并创建资源
                string assetPath =
                    AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/" + typeof(T).ToString() + ".asset");

                AssetDatabase.CreateAsset(asset, assetPath); // 在指定路径创建 ScriptableObject 资源
                AssetDatabase.SaveAssets(); // 保存资源
                AssetDatabase.Refresh(); // 刷新编辑器以显示新资源
                EditorUtility.FocusProjectWindow(); // 聚焦到项目窗口
                Selection.activeObject = asset; // 选中新创建的资源
            }
            else
            {
                // 如果资源已存在，输出一条警告
                Debug.LogWarning(typeof(T).ToString() + " already created!");
            }
        }
        
        // 创建 HerbScriptableObject
        [MenuItem("Assets/Inventory/Create Herb List")]
        public static void CreateHerbs()
        {
            ScriptableObjectManager.CreateAsset<HerbScriptableObject>();
        }

        // 创建 GrindedHerbScriptableObject
        [MenuItem("Assets/Inventory/Create Grinded Herb List")]
        public static void CreateGrindedHerbs()
        {
            ScriptableObjectManager.CreateAsset<GrindedHerbScriptableObject>();
        }

        // 创建 CraftMaterialScriptableObject
        [MenuItem("Assets/Inventory/Create Craft Material List")]
        public static void CreateCraftMaterials()
        {
            ScriptableObjectManager.CreateAsset<CraftMaterialScriptableObject>();
        }

        // 创建 MedicineScriptableObject
        [MenuItem("Assets/Inventory/Create Medicine List")]
        public static void CreateMedicines()
        {
            ScriptableObjectManager.CreateAsset<MedicineScriptableObject>();
        }
        
    }
}