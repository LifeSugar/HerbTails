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
    // 已经存在的 Herbs路径
    static string herbsPath = "Assets/Resources/HT.HerbScriptableObject.asset";

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
    }
    
    
    public static void UpdateHerbsFromExcel()
    {


        // 加载已有的资产
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
            // 读取 workbook
            IWorkbook workbook = new XSSFWorkbook(stream);
            // 假设只读取第一个 sheet
            ISheet sheet = workbook.GetSheetAt(0);

            // 第一行为表头，从第二行开始读取
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                // 根据列的顺序依次读取
                // 注意空格或空单元格处理，或先判断 CellType
                if (row.GetCell(0) == null)
                {
                    
                    Debug.Log($"加载了{i+1}行herb");
                    i = sheet.LastRowNum + 1;
                    continue;
                }
                string name          = row.GetCell(0).StringCellValue;
                // string icon          = row.GetCell(1).StringCellValue;
                string description   = row.GetCell(2).StringCellValue;
                int    count         = (int)row.GetCell(3).NumericCellValue;
                float  weight        = (float)row.GetCell(4).NumericCellValue;
                string prfabPath       = row.GetCell(5).StringCellValue;

                // 查找或新建 Herb
                Herb existingHerb = herbSO.herbs.Find(h => h.Name == name);
                if (existingHerb != null)
                {
                    // 更新
                    // existingHerb.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(icon);
                    GetIcon(row, 1, existingHerb);
                    existingHerb.Description = description;
                    existingHerb.Count = count;
                    existingHerb.Weight = weight;
                    existingHerb.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prfabPath);
                }
                else
                {
                    // 新增
                    Herb newHerb = new Herb
                    {
                        Name = name,
                        // Icon = AssetDatabase.LoadAssetAtPath<Sprite>(icon),
                        Description = description,
                        Count = count,
                        Weight = weight,
                        Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prfabPath)
                    };
                    GetIcon(row, 1, newHerb);
                    herbSO.herbs.Add(newHerb);
                }
                
                
            }
        }

        // 保存修改
        EditorUtility.SetDirty(herbSO);
        AssetDatabase.SaveAssets();
        Debug.Log("Herbs 已从 Excel 更新完成！");
        UpadateItems(herbSO.herbs);
    }
    
    public static void GetIcon(IRow row,  int iconColumnIndex,  Herb herb)
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
                herb.Icon = targetSprite;
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

    public static void UpadateItems<T>( List<T> inputsitems) where T : Item
    {
        ItemScriptableObject itemSO = AssetDatabase.LoadAssetAtPath<ItemScriptableObject>(itemPath);
        var items = itemSO.items;
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
}