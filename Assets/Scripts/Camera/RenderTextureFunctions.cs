using UnityEngine;

namespace PixelPerfectURP
{
    // 分辨率同步模式枚举
    public enum ResolutionSynchronizationMode
    {
        SetHeight,  // 根据高度计算宽度
        SetWidth,   // 根据宽度计算高度
        SetBoth     // 使用给定的宽高，不调整比例
    }
    
    public static class RenderTextureFunctions
    {
        /// <summary>
        /// 根据给定的宽高比例和同步模式计算纹理分辨率
        /// </summary>
        /// <param name="aspect">宽高比</param>
        /// <param name="resolution">初始分辨率</param>
        /// <param name="resolutionSynchronizationMode">分辨率同步模式</param>
        /// <returns>调整后的分辨率</returns>
        public static Vector2Int TextureResultion(float aspect, Vector2Int resolution, ResolutionSynchronizationMode resolutionSynchronizationMode)
        {
            return resolutionSynchronizationMode switch
            {
                ResolutionSynchronizationMode.SetHeight => new Vector2Int(Mathf.RoundToInt(resolution.y * aspect), resolution.y),  // 按高度同步宽度
                ResolutionSynchronizationMode.SetWidth => new Vector2Int(resolution.x, Mathf.RoundToInt(resolution.x / aspect)),  // 按宽度同步高度
                ResolutionSynchronizationMode.SetBoth => new Vector2Int(resolution.x, resolution.y),                             // 同步宽度和高度
                _ => Vector2Int.one,                                                                                          // 默认返回 (1, 1)
            };
        }

        /// <summary>
        /// 创建一个指定分辨率的 RenderTexture 纹理
        /// </summary>
        /// <param name="textureSize">纹理的宽高</param>
        /// <returns>成功创建的 RenderTexture，创建失败则返回 null</returns>
        public static RenderTexture CreateRenderTexture(Vector2Int textureSize)
        {
            RenderTexture newTexture = new RenderTexture(textureSize.x, textureSize.y, 32, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point // 设置纹理的过滤模式为 Point（像素化效果）
            };

            if (newTexture.Create())
            {
                return newTexture; // 成功创建纹理，返回该 RenderTexture
            }
            else
            {
                return null; // 创建失败，返回 null
            }
        }
    }
}
