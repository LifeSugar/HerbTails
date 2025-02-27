using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;
using HT;

public class AssetUpadater : EditorWindow
{
    private string message = "Asset Upadater";

    [MenuItem("Tools/Asset Upadater")]
    private void OnGUI()
    {
        GUILayout.Label("Asset Upadater", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label(message, EditorStyles.boldLabel);
        
        GUILayout.Space(20);

        if (GUILayout.Button("Update Herbs", EditorStyles.miniButton))
        {
            
        }
    }
    
    
    public static void UpdateHerbsFromExcel()
    {
        // Excel 文件路径
        string excelPath = "Assets/Data/Herbs.xlsx";
        // 已经存在的 ScriptableObject 路径
        string assetPath = "Assets/Resources/HTHerbScriptableObject.asset";

        // 加载已有的资产
        HerbScriptableObject herbSO = AssetDatabase.LoadAssetAtPath<HerbScriptableObject>(assetPath);
        if (herbSO == null)
        {
            Debug.LogError("找不到资产: " + assetPath);
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

            // 假设第一行为表头，从第二行开始读取
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                // 根据列的顺序依次读取
                // 注意空格或空单元格处理，或先判断 CellType
                string name          = row.GetCell(0).StringCellValue;
                string icon          = row.GetCell(1).StringCellValue;
                string description   = row.GetCell(2).StringCellValue;
                int    count         = (int)row.GetCell(3).NumericCellValue;
                string modelPath     = row.GetCell(4).StringCellValue;
                float  weight        = (float)row.GetCell(5).NumericCellValue;
                bool   coarseGrinded = bool.Parse(row.GetCell(6).StringCellValue);
                bool   fineGrinded   = bool.Parse(row.GetCell(7).StringCellValue);

                // 查找或新建 Herb
                Herb existingHerb = herbSO.herbs.Find(h => h.Name == name);
                if (existingHerb != null)
                {
                    // 更新
                    existingHerb.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(icon);
                    existingHerb.Description = description;
                    existingHerb.Count = count;
                    existingHerb.ModelPath = modelPath;
                    existingHerb.Weight = weight;
                }
                else
                {
                    // 新增
                    Herb newHerb = new Herb
                    {
                        Name = name,
                        Icon = AssetDatabase.LoadAssetAtPath<Sprite>(icon),
                        Description = description,
                        Count = count,
                        ModelPath = modelPath,
                        Weight = weight,
                    };
                    herbSO.herbs.Add(newHerb);
                }
            }
        }

        // 保存修改
        EditorUtility.SetDirty(herbSO);
        AssetDatabase.SaveAssets();
        Debug.Log("Herbs 已从 Excel 更新完成！");
    }
}