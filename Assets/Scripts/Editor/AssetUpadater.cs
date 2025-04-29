using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using Codice.Client.BaseCommands.Update;
using Codice.CM.Common.Purge;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;
using HT;

public class AssetUpadater : EditorWindow
{
    private string message = "Asset Upadater";
    // Excel 文件路径
    static string excelPath = "Assets/Resources/DataExcels/asset.xlsx";
    static string itemPath = "Assets/Resources/HT.ItemScriptableObject.asset";
    static string craftMaterialPath = "Assets/Resources/HT.CraftMaterialScriptableObject.asset";
    // 已经存在的 Herbs路径
    static string herbsPath = "Assets/Resources/HT.HerbScriptableObject.asset";
    static string grindedHerbsPath = "Assets/Resources/HT.GrindedHerbScriptableObject.asset";


    [MenuItem("Tools/Asset Upadater")]
    public static void ShowWindow()
    {
        AssetUpadater window = (AssetUpadater)GetWindow(typeof(AssetUpadater));
        window.minSize = new Vector2(400, 300);
    }
    private void OnGUI()
    {
        GUILayout.Label("Asset Upadater", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label(message, EditorStyles.boldLabel);
        GUILayout.Label("表格文件路径为：", EditorStyles.boldLabel);
        GUILayout.Label(excelPath, EditorStyles.boldLabel);

        GUILayout.Space(20);

        if (GUILayout.Button("Update Herbs", EditorStyles.miniButton))
        {
            UpdateHerbsFromExcel();
        }
        GUILayout.Label("Herbs为sheet0", EditorStyles.boldLabel);

        GUILayout.Space(10); 
        if (GUILayout.Button("Update Grinded Herbs", EditorStyles.miniButton))
        {
            UpdateGrindedHerbsFromExcel();
        }
        GUILayout.Label("Grinded Herbs为sheet1", EditorStyles.boldLabel);
    }

    
    
    public static void UpdateHerbsFromExcel()
{
    HerbScriptableObject herbSO = AssetDatabase.LoadAssetAtPath<HerbScriptableObject>(herbsPath);
    if (herbSO == null)
    {
        Debug.LogError("找不到资产: " + herbsPath);
        return;
    }

    if (!File.Exists(excelPath))
    {
        Debug.LogError("找不到 Excel 文件: " + excelPath);
        return;
    }

    using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
    {
        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);

        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            if (row.GetCell(0) == null)
            {
                Debug.Log($"加载了{i + 1}行herb");
                i = sheet.LastRowNum + 1;
                continue;
            }

            string name = row.GetCell(0).StringCellValue.Trim();
            string description = row.GetCell(2)?.StringCellValue.Trim() ?? "";
            int count = (int)(row.GetCell(3)?.NumericCellValue ?? 0);
            float weight = (float)(row.GetCell(4)?.NumericCellValue ?? 0);
            string prefabPath = row.GetCell(5)?.StringCellValue.Trim() ?? "";

            // --- 读取颜色，优先背景色 ---
            Color color = Color.white;
            if (row.GetCell(6) != null)
            {
                ICell cell = row.GetCell(6);
                ICellStyle style = cell.CellStyle;
                if (style != null && style.FillForegroundColorColor is XSSFColor xssfColor)
                {
                    byte[] rgb = xssfColor.RGB;
                    if (rgb != null && rgb.Length >= 3)
                    {
                        color = new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f, 1f);
                    }
                    else
                    {
                        TryParseColorFromText(cell, ref color, i);
                    }
                }
                else
                {
                    TryParseColorFromText(cell, ref color, i);
                }
            }

            // --- 读取 GrindedHerb 和 SlicedHerb ---
            string grindedHerb = "Grinded" + name; // 默认值
            if (row.GetCell(7) != null && !string.IsNullOrEmpty(row.GetCell(7).ToString().Trim()))
            {
                grindedHerb = row.GetCell(7).ToString().Trim();
            }

            string slicedHerb = "Sliced" + name; // 默认值
            if (row.GetCell(8) != null && !string.IsNullOrEmpty(row.GetCell(8).ToString().Trim()))
            {
                slicedHerb = row.GetCell(8).ToString().Trim();
            }

            // --- 查找或新增 ---
            Herb existingHerb = herbSO.herbs.Find(h => h.Name == name);
            if (existingHerb != null)
            {
                GetIcon(row, 1, existingHerb);
                existingHerb.Description = description;
                existingHerb.Count = count;
                existingHerb.Weight = weight;
                existingHerb.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                existingHerb.Color = color;
                existingHerb.GrindedHerb = grindedHerb;
                existingHerb.SlicedHerb = slicedHerb;
            }
            else
            {
                Herb newHerb = new Herb
                {
                    Name = name,
                    Description = description,
                    Count = count,
                    Weight = weight,
                    Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath),
                    Color = color,
                    GrindedHerb = grindedHerb,
                    SlicedHerb = slicedHerb
                };
                GetIcon(row, 1, newHerb);
                herbSO.herbs.Add(newHerb);
            }
        }
    }

    EditorUtility.SetDirty(herbSO);
    UpadateItems<Herb>(herbSO.herbs);
    UpdateCraftMaterials<Herb>(herbSO.herbs);
    AssetDatabase.SaveAssets();
    Debug.Log("Herbs 已从 Excel 更新完成！");
    
}
    
    public static void UpdateGrindedHerbsFromExcel()
{
    GrindedHerbScriptableObject grindedHerbSO = AssetDatabase.LoadAssetAtPath<GrindedHerbScriptableObject>(grindedHerbsPath);
    if (grindedHerbSO == null)
    {
        Debug.LogError("找不到资产: " + grindedHerbsPath);
        return;
    }

    if (!File.Exists(excelPath))
    {
        Debug.LogError("找不到 Excel 文件: " + excelPath);
        return;
    }

    using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
    {
        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(1); // 读取第2个sheet (GrindedHerbs)

        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            if (row.GetCell(0) == null)
            {
                Debug.Log($"加载了{i + 1}行grinded herb");
                i = sheet.LastRowNum + 1;
                continue;
            }

            string name = row.GetCell(0).StringCellValue.Trim();
            string description = row.GetCell(2)?.StringCellValue.Trim() ?? "";
            int count = (int)(row.GetCell(3)?.NumericCellValue ?? 0);
            float weight = (float)(row.GetCell(4)?.NumericCellValue ?? 0);

            // 创建或更新
            GrindedHerb existingGrindedHerb = grindedHerbSO.grindedHerbs.Find(g => g.Name == name);
            if (existingGrindedHerb != null)
            {
                existingGrindedHerb.Name = name;
                existingGrindedHerb.Description = description;
                existingGrindedHerb.Count = count;
                existingGrindedHerb.Weight = weight;
                GetIcon(row, 1, existingGrindedHerb);
            }
            else
            {
                GrindedHerb newGrindedHerb = new GrindedHerb
                {
                    Name = name,
                    Description = description,
                    Count = count,
                    Weight = weight
                };
                GetIcon(row, 1, newGrindedHerb);
                grindedHerbSO.grindedHerbs.Add(newGrindedHerb);
            }
        }
    }

    EditorUtility.SetDirty(grindedHerbSO);
    UpadateItems(grindedHerbSO.grindedHerbs);
    UpdateCraftMaterials<GrindedHerb>(grindedHerbSO.grindedHerbs);
    AssetDatabase.SaveAssets();
    Debug.Log("Grinded Herbs 已从 Excel 更新完成！");
}


    
    public static void GetIcon(IRow row,  int iconColumnIndex,  Item item)
    {
        string iconData = row.GetCell(iconColumnIndex).StringCellValue.Trim(); // "Assets/Textures/Icons/SpriteSheet.png|TestGreenSprite"
        string[] parts = iconData.Split('|');
        if (parts.Length == 2)
        {
            string sheetPath = parts[0].Trim();
            string spriteName = parts[1].Trim();
 
            
            // 加载 sprite sheet 中的所有 Sprite（确保 sprite sheet 的 Import Settings 中 Sprite Mode 为 Multiple）
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                .OfType<Sprite>()
                .ToArray();
            string extension = ".aseprite";
            string cleanSheetPath = sheetPath;
            if (cleanSheetPath.EndsWith(extension, System.StringComparison.OrdinalIgnoreCase))
            {
                cleanSheetPath = cleanSheetPath.Substring(0, cleanSheetPath.Length - extension.Length);
            }
            
            // 除后缀的 cleanSheetPath 拼接 spriteName
            // Sprite targetSprite = sprites.FirstOrDefault(s => s.name.Equals(cleanSheetPath + "_" + spriteName, System.StringComparison.OrdinalIgnoreCase));
            Sprite targetSprite = sprites[int.Parse(spriteName)];
    
            if (targetSprite != null)
            {
                item.Icon = targetSprite;
            }
            else
            {
                Debug.LogWarning($"在 {sheetPath} 中未找到名称为 {cleanSheetPath}_{spriteName} 的 Sprite");
            }
        }
        else
        {
            Debug.LogWarning("Icon 数据格式不正确，请确保格式为: SpriteSheetPath|SpriteName");
        }
    }
    private static void TryParseColorFromText(ICell cell, ref Color color, int rowIndex)
    {
        string colorString = cell.ToString().Trim();
        if (!string.IsNullOrEmpty(colorString))
        {
            string[] rgb = colorString.Split(',');
            if (rgb.Length >= 3 &&
                float.TryParse(rgb[0], out float r) &&
                float.TryParse(rgb[1], out float g) &&
                float.TryParse(rgb[2], out float b))
            {
                float a = 1f;
                if (rgb.Length == 4 && float.TryParse(rgb[3], out float alpha))
                {
                    a = alpha;
                }
                color = new Color(r, g, b, a);
            }
            else
            {
                Debug.LogWarning($"第{rowIndex + 1}行的颜色文字格式错误: {colorString}");
            }
        }
    }

    

    public static void UpadateItems<T>( List<T> inputsitems) where T : Item
    {
        ItemScriptableObject itemSO = AssetDatabase.LoadAssetAtPath<ItemScriptableObject>(itemPath);
        var items = itemSO.items;
        EditorUtility.SetDirty(itemSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"ItemScriptableObject 已更新，共 {inputsitems.Count} 条");
        for (int i = 0; i < inputsitems.Count; i++)
        {
            Item existingItem = items.Find(item => item.Name.Equals(inputsitems[i].Name));
            if (existingItem != null)
            {
                existingItem.Name = inputsitems[i].Name;
                existingItem.Description = inputsitems[i].Description;
                existingItem.Count = inputsitems[i].Count;
                existingItem.Icon = inputsitems[i].Icon;
            }
            else
            {
                Item newItem = new Item()
                {
                    Name = inputsitems[i].Name,
                    Description = inputsitems[i].Description,
                    Icon = inputsitems[i].Icon,
                    Count = inputsitems[i].Count
                };
                items.Add(newItem);
            }
        }
    }

    public static void UpdateCraftMaterials<T>(List<T> inputsitems) where T : CraftMaterial
    {
        CraftMaterialScriptableObject craftSO = AssetDatabase.LoadAssetAtPath<CraftMaterialScriptableObject>(craftMaterialPath);
        var items = craftSO.craftMaterials;
        EditorUtility.SetDirty(craftSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"CraftMaterialScriptableObject 已更新，共 {inputsitems.Count} 条");
        for (int i = 0; i < inputsitems.Count; i++)
        {
            CraftMaterial existingItem = items.Find(item => item.Name.Equals(inputsitems[i].Name));
            if (existingItem != null)
            {
                existingItem.Name = inputsitems[i].Name;
                existingItem.Description = inputsitems[i].Description;
                existingItem.Count = inputsitems[i].Count;
                existingItem.Icon = inputsitems[i].Icon;
                existingItem.Weight = inputsitems[i].Weight;
            }
            else
            {
                CraftMaterial newItem = new CraftMaterial()
                {
                    Name = inputsitems[i].Name,
                    Description = inputsitems[i].Description,
                    Icon = inputsitems[i].Icon,
                    Weight = inputsitems[i].Weight,
                    Count = inputsitems[i].Count
                };
                items.Add(newItem);
            }
        }
    }
}