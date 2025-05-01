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
    static string medicinePath = "Assets/Resources/HT.MedicineScriptableObject.asset";
    static string prescriptionPath = "Assets/Resources/HT.PrescriptionScriptableObject.asset";


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
            ResourceManager.instance.LoadEverything();
        }
        GUILayout.Label("Herbs为sheet0", EditorStyles.boldLabel);

        GUILayout.Space(10); 
        if (GUILayout.Button("Update Grinded Herbs", EditorStyles.miniButton))
        {
            UpdateGrindedHerbsFromExcel();
            ResourceManager.instance.LoadEverything();
        }
        GUILayout.Label("Grinded Herbs为sheet1", EditorStyles.boldLabel);
        
        GUILayout.Space(20);
        if (GUILayout.Button("Update Medicines", EditorStyles.miniButton))
        {
            UpdateMedicinesFromExcel();
            ResourceManager.instance.LoadEverything();
        }
        GUILayout.Label("Medicines 为 sheet2", EditorStyles.boldLabel);
        
        GUILayout.Space(20);
        if (GUILayout.Button("Update Prescriptions", EditorStyles.miniButton))
        {
            UpdatePrescriptionsFromExcel();
            ResourceManager.instance.LoadEverything();
        }
            
        GUILayout.Label("Prescriptions 为 sheet3", EditorStyles.boldLabel);
        
        GUILayout.Space(20);
        if (GUILayout.Button("Clear All", EditorStyles.miniButton))
            ClearAll();
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
    UpadateItems<GrindedHerb>(grindedHerbSO.grindedHerbs);
    UpdateCraftMaterials<GrindedHerb>(grindedHerbSO.grindedHerbs);
    AssetDatabase.SaveAssets();
    Debug.Log("Grinded Herbs 已从 Excel 更新完成！");
}
    
        public static void UpdateMedicinesFromExcel()
    {
        // 加载 MedicineScriptableObject
        var medSO = AssetDatabase.LoadAssetAtPath<MedicineScriptableObject>(medicinePath);
        if (medSO == null)
        {
            Debug.LogError("找不到资产: " + medicinePath);
            return;
        }

        if (!File.Exists(excelPath))
        {
            Debug.LogError("找不到 Excel 文件: " + excelPath);
            return;
        }

        using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(2); // 第三个 sheet

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                // 如果 Name 列空了，就当做到底部
                if (row.GetCell(0) == null || string.IsNullOrWhiteSpace(row.GetCell(0).ToString()))
                {
                    Debug.Log($"加载了 {i + 1} 行 medicine，遇到空行终止");
                    break;
                }

                string name        = row.GetCell(0).StringCellValue.Trim();
                string iconData    = row.GetCell(1)?.StringCellValue.Trim() ?? "";
                string desc        = row.GetCell(2)?.StringCellValue.Trim() ?? "";
                int    count       = (int)(row.GetCell(3)?.NumericCellValue ?? 0);

                // 查找或新增
                Medicine existing = medSO.medicines.Find(m => m.Name == name);
                if (existing == null)
                {
                    var m = new Medicine()
                    {
                        Name = name,
                        Description = desc,
                        Count = count,
                        
                    };
                    GetIcon(row, 1, m);
                    medSO.medicines.Add(m);
                }
                else
                {
                    existing.Description = desc;
                    existing.Count       = count;
                    GetIcon(row, 1, existing);
                }
            }
        }

        EditorUtility.SetDirty(medSO);
        // 同步到通用的 ItemScriptableObject 和 CraftMaterialScriptableObject
        UpadateItems<Medicine>(medSO.medicines);
        AssetDatabase.SaveAssets();
        Debug.Log("Medicines 已从 Excel 的 sheet2 更新完成！");
    }


    
    public static void GetIcon(IRow row,  int iconColumnIndex,  Item item)
    {
        var iconData = row.GetCell(iconColumnIndex)?.StringCellValue?.Trim();
        if (string.IsNullOrEmpty(iconData))
        {
            Debug.LogWarning("Icon 数据为空");
            return;
        }

        var parts = iconData.Split('|');
        if (parts.Length != 2)
        {
            Debug.LogWarning("Icon 数据格式不正确，请确保格式为: SpriteSheetPath|SpriteIndexOrName");
            return;
        }

        string sheetPath     = parts[0].Trim();    // e.g. "Assets/.../excel.aseprite" 或者 ".../icons.png"
        string spriteKey     = parts[1].Trim();    // 要么是数字索引，要么是 Sprite 名称

        var allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        var sprites   = allAssets.OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogWarning($"在路径 {sheetPath} 没有加载到任何 Sprite");
            return;
        }

        Sprite target = null;

        if (int.TryParse(spriteKey, out int idx))
        {
            if (idx >= 0 && idx < sprites.Length)
                target = sprites[idx];
        }

        if (target == null)
            target = sprites.FirstOrDefault(s => s.name.Equals(spriteKey, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            
            string withoutExt = Path.ChangeExtension(sheetPath, null);
            string baseName   = Path.GetFileNameWithoutExtension(withoutExt);
            string composite  = $"{baseName}_{spriteKey}";
            target = sprites.FirstOrDefault(s => s.name.Equals(composite, StringComparison.OrdinalIgnoreCase));
        }

        if (target != null)
        {
            item.Icon = target;
        }
        else
        {
            Debug.LogWarning($"无法在 {sheetPath} 中找到 对应 “{spriteKey}” 的 Sprite");
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
    
    public static void UpdatePrescriptionsFromExcel()
{
    var so = AssetDatabase.LoadAssetAtPath<PrescriptionScriptableObject>(prescriptionPath);
    if (so == null)
    {
        Debug.LogError("找不到资产: " + prescriptionPath);
        return;
    }
    if (!File.Exists(excelPath))
    {
        Debug.LogError("找不到 Excel 文件: " + excelPath);
        return;
    }

    using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
    IWorkbook wb = new XSSFWorkbook(stream);
    ISheet sheet = wb.GetSheetAt(3);

    Prescription current = null;
    string currentField = null;

    for (int r = 0; r <= sheet.LastRowNum; r++)
    {
        IRow row = sheet.GetRow(r);
        if (row == null) continue;

        // A 列文本
        var aCell = row.GetCell(0);
        string aText = aCell?.ToString().Trim();

        if (!string.IsNullOrEmpty(aText))
        {
            // 碰到 Name，开新处方，然后跳过本行解析数据
            if (aText.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                var nameCell = row.GetCell(1);
                if (nameCell == null) continue;

                string name = nameCell.ToString().Trim();
                current = so.prescriptions.Find(p => p.Name == name);
                if (current == null)
                {
                    current = new Prescription
                    {
                        Name = name,
                        CraftMaterials = new List<CraftMaterial>(),
                        Weights        = new List<int>(),
                        FirePeriods    = new List<FirePeriod>()
                    };
                    so.prescriptions.Add(current);
                }
                else
                {
                    current.CraftMaterials.Clear();
                    current.Weights.Clear();
                    current.FirePeriods.Clear();
                    current.ResultMedicine = null;
                }

                currentField = null;
                continue;
            }

            // 碰到字段标题（也要解析本行的第一条数据）
            if (aText.Equals("CraftMaterials|weight", StringComparison.OrdinalIgnoreCase)
             || aText.Equals("Medicine",             StringComparison.OrdinalIgnoreCase)
             || aText.Equals("FirePeriod",           StringComparison.OrdinalIgnoreCase))
            {
                currentField = aText;
                // 注意：这里不跳 continue，让下面也能读取本行 B 列数据
            }
        }

        // 如果不是 Name 行，且已经在某个 prescription 里，且 currentField 已经选定
        if (current == null || string.IsNullOrEmpty(currentField))
            continue;

        var dataCell = row.GetCell(1);
        if (dataCell == null) continue;
        string raw = dataCell.ToString().Trim();
        if (string.IsNullOrEmpty(raw)) continue;

        switch (currentField)
        {
            case "CraftMaterials|weight":
                {
                    var parts = raw.Split('|');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int w))
                    {
                        var mat = ResourceManager.instance.GetCraftMaterial(parts[0]);
                        if (mat != null)
                        {
                            current.CraftMaterials.Add(mat);
                            current.Weights.Add(w);
                        }
                    }
                }
                break;

            case "Medicine":
                {
                    var med = ResourceManager.instance.GetMedicine(raw);
                    if (med != null)
                        current.ResultMedicine = med;
                }
                break;

            case "FirePeriod":
                {
                    var parts = raw.Split('|');
                    if (parts.Length == 2
                     && Enum.TryParse<FirePower>(parts[0], true, out var fp)
                     && float.TryParse(parts[1], out var dur))
                    {
                        current.FirePeriods.Add(new FirePeriod
                        {
                            FirePower = fp,
                            Duration  = dur
                        });
                    }
                }
                break;
        }
    }

    EditorUtility.SetDirty(so);
    AssetDatabase.SaveAssets();
    Debug.Log("Prescriptions 已从 sheet3 更新完成！");
}


    // public static void UpdatePrescriptionsFromExcel()
    // {
    //     // 1. 加载 SO
    //     var so = AssetDatabase.LoadAssetAtPath<PrescriptionScriptableObject>(prescriptionPath);
    //     if (so == null)
    //     {
    //         Debug.LogError("找不到资产: " + prescriptionPath);
    //         return;
    //     }
    //
    //     if (!File.Exists(excelPath))
    //     {
    //         Debug.LogError("找不到 Excel 文件: " + excelPath);
    //         return;
    //     }
    //
    //     // 2. 打开第四个 sheet
    //     using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
    //     IWorkbook wb = new XSSFWorkbook(stream);
    //     ISheet sheet = wb.GetSheetAt(3);
    //
    //     Prescription current = null;
    //     string currentField = null;
    //
    //     // 3. 从第 0 行开始，遇到 A 列写了 “Name” 就开启一个新处方
    //     for (int r = 0; r <= sheet.LastRowNum; r++)
    //     {
    //         IRow row = sheet.GetRow(r);
    //         if (row == null) continue;
    //
    //         // 读取 A 列
    //         var aCell = row.GetCell(0);
    //         string aText = aCell?.ToString().Trim();
    //
    //         if (!string.IsNullOrEmpty(aText))
    //         {
    //             // A 列有内容，可能是 Name / CraftMaterials|weight / Medicine / FirePeriod
    //             if (aText.Equals("Name", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 // 新处方起点
    //                 var nameCell = row.GetCell(1);
    //                 if (nameCell == null) continue;
    //
    //                 string name = nameCell.ToString().Trim();
    //                 // 尝试找已存在
    //                 current = so.prescriptions.Find(p => p.Name == name);
    //                 if (current == null)
    //                 {
    //                     current = new Prescription
    //                     {
    //                         Name = name,
    //                         CraftMaterials = new List<CraftMaterial>(),
    //                         Weights = new List<int>(),
    //                         FirePeriods = new List<FirePeriod>()
    //                     };
    //                     so.prescriptions.Add(current);
    //                 }
    //                 else
    //                 {
    //                     // 清空旧数据
    //                     current.CraftMaterials.Clear();
    //                     current.Weights.Clear();
    //                     current.FirePeriods.Clear();
    //                     current.ResultMedicine = null;
    //                 }
    //
    //                 // 重置state，不立刻解析本行第 B 列（Name行只有名称）
    //                 currentField = null;
    //             }
    //             else
    //             {
    //                 // 切换到下划字段
    //                 currentField = aText;
    //             }
    //
    //             continue;
    //         }
    //
    //         // 如果 A 列空，说明是 currentField 下某个子项
    //         if (current == null || string.IsNullOrEmpty(currentField))
    //             continue;
    //
    //         var dataCell = row.GetCell(1);
    //         if (dataCell == null) continue;
    //         string raw = dataCell.ToString().Trim();
    //         if (string.IsNullOrEmpty(raw)) continue;
    //
    //         switch (currentField)
    //         {
    //             case "CraftMaterials|weight":
    //             {
    //                 var parts = raw.Split('|');
    //                 if (parts.Length == 2 &&
    //                     int.TryParse(parts[1], out int w))
    //                 {
    //                     var mat = ResourceManager.instance.GetCraftMaterial(parts[0]);
    //                     if (mat != null)
    //                     {
    //                         current.CraftMaterials.Add(mat);
    //                         current.Weights.Add(w);
    //                     }
    //                 }
    //             }
    //                 break;
    //
    //             case "Medicine":
    //             {
    //                 var med = ResourceManager.instance.GetMedicine(raw);
    //                 if (med != null)
    //                     current.ResultMedicine = med;
    //             }
    //                 break;
    //
    //             case "FirePeriod":
    //             {
    //                 var parts = raw.Split('|');
    //                 if (parts.Length == 2 &&
    //                     Enum.TryParse<FirePower>(parts[0], true, out var fp) &&
    //                     float.TryParse(parts[1], out var dur))
    //                 {
    //                     current.FirePeriods.Add(new FirePeriod
    //                     {
    //                         FirePower = fp,
    //                         Duration = dur
    //                     });
    //                 }
    //             }
    //                 break;
    //         }
    //     }
    //
    // }
    //  public static void UpdatePrescriptionsFromExcel()
    // {
    //     // 1. 加载 PrescriptionScriptableObject
    //     var prescrSO = AssetDatabase.LoadAssetAtPath<PrescriptionScriptableObject>(prescriptionPath);
    //     if (prescrSO == null)
    //     {
    //         Debug.LogError("找不到资产: " + prescriptionPath);
    //         return;
    //     }
    //     if (!File.Exists(excelPath))
    //     {
    //         Debug.LogError("找不到 Excel 文件: " + excelPath);
    //         return;
    //     }
    //
    //     // 2. 打开第四个 sheet（index=3）
    //     using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
    //     IWorkbook workbook = new XSSFWorkbook(stream);
    //     ISheet sheet = workbook.GetSheetAt(3);
    //
    //     // 3. 第一行从 col=1 开始，每个非空单元格是一条新的处方
    //     IRow headerRow = sheet.GetRow(0);
    //     for (int col = 1; col < headerRow.LastCellNum; col++)
    //     {
    //         var nameCell = headerRow.GetCell(col);
    //         if (nameCell == null || string.IsNullOrWhiteSpace(nameCell.ToString()))
    //             continue;
    //
    //         string prescName = nameCell.StringCellValue.Trim();
    //         // 查找或新增 Prescription 对象
    //         var presc = prescrSO.prescriptions.Find(p => p.Name == prescName);
    //         if (presc == null)
    //         {
    //             presc = new Prescription
    //             {
    //                 Name           = prescName,
    //                 CraftMaterials = new List<CraftMaterial>(),
    //                 Weights        = new List<int>(),
    //                 FirePeriods    = new List<FirePeriod>()
    //             };
    //             prescrSO.prescriptions.Add(presc);
    //         }
    //         else
    //         {
    //             // 清空旧数据
    //             presc.CraftMaterials.Clear();
    //             presc.Weights.Clear();
    //             presc.FirePeriods.Clear();
    //             presc.ResultMedicine = null;
    //         }
    //
    //         // 4. 按行扫描这一列，把值分发到对应的属性里
    //         string currentField = null;
    //         for (int row = 1; row <= sheet.LastRowNum; row++)
    //         {
    //             IRow r = sheet.GetRow(row);
    //             if (r == null) continue;
    //
    //             // A 列如果不空，更新 currentField
    //             var fieldCell = r.GetCell(0);
    //             if (fieldCell != null && !string.IsNullOrWhiteSpace(fieldCell.ToString()))
    //                 currentField = fieldCell.StringCellValue.Trim();
    //
    //             // 取 B/C/D… 列 currentField 对应列
    //             var dataCell = r.GetCell(col);
    //             if (dataCell == null || string.IsNullOrWhiteSpace(dataCell.ToString()))
    //                 continue;
    //             string raw = dataCell.StringCellValue.Trim();
    //
    //             switch (currentField)
    //             {
    //                 case "CraftMaterials|weight":
    //                     {
    //                         var parts = raw.Split('|');
    //                         if (parts.Length == 2)
    //                         {
    //                             var mat = ResourceManager.instance.GetCraftMaterial(parts[0]);
    //                             if (mat != null &&
    //                                 int.TryParse(parts[1], out int w))
    //                             {
    //                                 presc.CraftMaterials.Add(mat);
    //                                 presc.Weights.Add(w);
    //                             }
    //                         }
    //                     }
    //                     break;
    //
    //                 case "Medicine":
    //                     {
    //                         var med = ResourceManager.instance.GetMedicine(raw);
    //                         presc.ResultMedicine = med;
    //                     }
    //                     break;
    //
    //                 case "FirePeriod":
    //                     {
    //                         var parts = raw.Split('|');
    //                         if (parts.Length == 2 &&
    //                             Enum.TryParse<FirePower>(parts[0], true, out var fp) &&
    //                             float.TryParse(parts[1], out var dur))
    //                         {
    //                             presc.FirePeriods.Add(new FirePeriod
    //                             {
    //                                 FirePower = fp,
    //                                 Duration  = dur
    //                             });
    //                         }
    //                     }
    //                     break;
    //             }
    //         }
    //     }
    //
    //     // 5. 保存
    //     EditorUtility.SetDirty(prescrSO);
    //     AssetDatabase.SaveAssets();
    //     Debug.Log("Prescriptions 已从 Excel 的 sheet3 更新完成！");
    // }

    public void ClearAll()
    {
        var so = AssetDatabase.LoadAssetAtPath<PrescriptionScriptableObject>(prescriptionPath);
        so.prescriptions.Clear();
        var herbs = AssetDatabase.LoadAssetAtPath<HerbScriptableObject>(herbsPath);
        herbs.herbs.Clear();
        var medicines = AssetDatabase.LoadAssetAtPath<MedicineScriptableObject>(medicinePath);
        medicines.medicines.Clear();
        var craftm = AssetDatabase.LoadAssetAtPath<CraftMaterialScriptableObject>(craftMaterialPath);
        craftm.craftMaterials.Clear();
        var iso = AssetDatabase.LoadAssetAtPath<ItemScriptableObject>(itemPath);
        iso.items.Clear();
    }
}