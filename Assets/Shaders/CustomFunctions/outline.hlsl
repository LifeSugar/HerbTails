#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED


//在 HLSL 中，sampler 是用于控制纹理采样行为的对象。它负责定义如何从纹理中获取数据
//所用的纹理和他们的采样器是Unity内置的，必须声明，此文件才能使用这些内置资源
TEXTURE2D (_CameraDepthTexture);
SAMPLER (sampler_CameraDepthTexture);

TEXTURE2D (_CameraNormalsTexture);
SAMPLER (sampler_CameraNormalsTexture);


TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);


float GetDepth(float2 UV)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, UV).x;
}

float3 GetNormal(float2 UV)
{
    float3 worldNormal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, UV).xyz;
    return mul((float3x3) UNITY_MATRIX_V, worldNormal) * 2.0 - 1.0;
}

float4 _CameraNormalsTexture_TexelSize;  // 纹理的纹素大小，xy 分别对应纹理的宽度和高度的倒数，用于计算相邻像素的偏移量。

void Outline_float(float2 UV, float edgethickness, float DepthThreshold, float NormalsThreshold, float3 NormalEdgeBias, float DepthEdgeStrength, float NormalEdgeStrength, out float Outline)
{
    // 获取当前纹理的单个纹素大小（偏移量），用于计算相邻像素位置。
    float2 texelSize = _CameraNormalsTexture_TexelSize.xy * edgethickness;

    // 获取当前 UV 对应的深度值
    float depth = GetDepth(UV);
    
    // 获取当前 UV 对应的法线值（从法线贴图中采样）
    float3 normal = GetNormal(UV);
    
    // 存储相邻像素的 UV 坐标
    float2 uv_side[4];
    
    // 计算相邻 4 个像素的 UV 坐标（上、下、左、右）
    uv_side[0] = UV + float2(0.0, texelSize.y);  // 上方像素
    uv_side[1] = UV - float2(0.0, texelSize.y);  // 下方像素
    uv_side[2] = UV + float2(texelSize.x, 0.0);  // 右侧像素
    uv_side[3] = UV - float2(texelSize.x, 0.0);  // 左侧像素

    // --------------------------
    // 1. 深度边缘检测（Depth Edge Detection）
    // --------------------------

    float depths[4];  // 用于存储相邻像素的深度值
    float depthDifference = 0.0;  // 累积深度差异值

    // 遍历相邻 4 个像素，计算与当前像素的深度差异
    [unroll]
    for (int i = 0; i < 4; i++)
    {
        depths[i] = GetDepth(uv_side[i]);           // 获取相邻像素的深度值
        depthDifference += depth - depths[i];   // 累积深度差异
    }

    // 如果深度差异超过给定的阈值 DepthThreshold，则认为是一个深度边缘（返回 1，否则返回 0）
    float depthEdge = step(DepthThreshold, depthDifference);

    // --------------------------
    // 2. 法线边缘检测（Normal Edge Detection）
    // --------------------------

    float3 normals[4];  // 用于存储相邻像素的法线值
    float dotSum = 0.0; // 用于累积法线差异的平方和

    [unroll]
    for (int j = 0; j < 4; j++)
    {
        normals[j] = GetNormal(uv_side[j]);           // 获取相邻像素的法线值
        if (abs(depths[j] - depth) < 0.001)  // 仅对深度接近的像素进行法线检测
        {
            float3 normalDiff = normal - normals[j];  // 计算法线差异向量
        
            // 计算法线差异与偏置向量的点积，用于检测法线边缘的方向性
            float normalBiasDiff = dot(normalDiff, NormalEdgeBias);

            // 使用 smoothstep 平滑处理法线差异的边缘响应,防止并不显著的法线边缘存在锯齿或不连续
            float normalIndicator = smoothstep(-0.01, .01, normalBiasDiff);
        
            // 累积法线差异的平方和，并乘以边缘指示器
            dotSum += dot(normalDiff, normalDiff) * normalIndicator;
        }

    }

    // 对累积的法线差异进行平方根处理，得到最终的法线边缘指示值
    float indicator = sqrt(dotSum);

    // 如果法线差异超过给定的阈值 NormalsThreshold，则认为是一个法线边缘（返回 1，否则返回 0）
    float normalEdge = step(NormalsThreshold, indicator);

    // --------------------------
    // 3. 合并深度边缘和法线边缘结果
    // --------------------------

    // 如果深度差异小于 0，忽略法线边缘并返回 0。
    // 如果深度差异大于阈值，则认为是一个深度边缘并使用 DepthEdgeStrength 强调它；
    // 否则，如果是一个法线边缘，则使用 NormalEdgeStrength 强调它。
    Outline = (depthDifference < 0) ? (NormalEdgeStrength * normalEdge) :        // 深度差异小于 0，不描边
          (depthEdge > 0.0) ? (DepthEdgeStrength * depthEdge) :  // 深度边缘描边
          (NormalEdgeStrength * normalEdge); // 法线边缘描边

}

float4 GetColor(float2 UV)
{
    return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UV);
}

void OutlineColors_float(float2 UV, float EdgeThickness, float DepthThreshold, float NormalsThreshold, float3 NormalEdgeBias, float DepthEdgeStrength, float NormalEdgeStrength, out float4 Out)
{
    float Outline;
    Outline_float(UV, EdgeThickness, DepthThreshold, NormalsThreshold, NormalEdgeBias, DepthEdgeStrength, NormalEdgeStrength, Outline);
    Out = Outline * GetColor(UV);
}





#endif