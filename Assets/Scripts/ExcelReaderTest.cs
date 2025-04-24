using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;
using UnityEngine.UI;

public class ExcelReaderTest : MonoBehaviour
{
    string excelPath = "Assets/Resources/test.xlsx";
    public string testString;
    public int testInt;
    public float testFloat;
    public bool testBool;

    void Start()
    {
        if (!File.Exists(excelPath))
        {
            Debug.LogError("excel file doesn't exist");
        }
        
        using (FileStream fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheetAt(0);
            IRow row = sheet.GetRow(0);
            testString = row.GetCell(0).StringCellValue;
            testInt = (int) row.GetCell(1).NumericCellValue;
            testFloat = (float) row.GetCell(2).NumericCellValue;
            testBool = row.GetCell(3).BooleanCellValue;
        }

    }

    
}
