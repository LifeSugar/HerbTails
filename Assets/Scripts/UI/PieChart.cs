using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using HT;
public class PieChart : MonoBehaviour
{
    public List<float> values;      // 输入的数据值
    public List<Color> colors;      // 各扇区的颜色
    public Sprite sliceSprite;  // 扇区使用的Sprite（建议使用纯白色）
    public Image maskSprite;
    public Color maskColor;
    public float duration;
    


    void Start()
    {
        SetUpMaskVisual(maskSprite);
        GenerateChart();
        // CountDown(maskSprite, duration);
    }

    public void clearChart()
    {
        values.Clear();
        colors.Clear();
    }

    public void GenerateChart()
    {
        // 清除旧的扇区
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        float total = CalculateTotal();
        if (total <= 0) return;

        CreateSlices(total);
    }

    float CalculateTotal()
    {
        float total = 0;
        foreach (float value in values)
        {
            total += value;
        }
        return total;
    }

    void CreateSlices(float total)
    {
        float currentAngle = 0f;

        for (int i = 0; i < values.Count; i++)
        {
            float normalizedValue = values[i] / total;
            GameObject slice = CreateSliceObject(i);
            SetupSliceVisual(slice, i);
            SetupSliceTransform(slice, ref currentAngle, normalizedValue);
        }
    }

    GameObject CreateSliceObject(int index)
    {
        GameObject slice = new GameObject($"Slice {index}");
        slice.transform.SetParent(transform, false);
        slice.AddComponent<Image>();
        return slice;
    }

    void SetupSliceVisual(GameObject slice, int index)
    {
        Image image = slice.GetComponent<Image>();
        image.sprite = sliceSprite ? sliceSprite : Resources.GetBuiltinResource<Sprite>("UI/Sprite/Default");
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 2; // 从顶部开始填充
        image.color = colors[index % colors.Count];
    }

    void SetupSliceTransform(GameObject slice, ref float currentAngle, float normalizedValue)
    {
        RectTransform rect = slice.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0, 0, -currentAngle);
        
        Image image = slice.GetComponent<Image>();
        image.fillAmount = normalizedValue;
        
        currentAngle += normalizedValue * 360f;
    }

    void SetUpMaskVisual(Image mask)
    {
        mask.type = Image.Type.Filled;
        mask.fillMethod = Image.FillMethod.Radial360;
        mask.fillOrigin = 2;
        mask.color = maskColor;
    }

    
    float originFillAmount = 0;
    private float targetFillAmount = 1;
    public void CountDown(Image mask, float duration)
    {
        Sequence countDown = DOTween.Sequence();
        countDown.Append(
            DOTween.To(
                () => originFillAmount,
                v =>
                {
                    originFillAmount = v;
                    mask.fillAmount = originFillAmount;
                },
                targetFillAmount,
                duration
                ).SetEase(Ease.Linear)
            );
    }
}