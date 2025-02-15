using UnityEngine;

namespace PixelPerfectURP
{
    /// <summary>
    /// 用于显示游戏相机渲染结果的放大画布（Upscaled Canvas）。
    /// </summary>
    [ExecuteInEditMode]
    public class UpscaledCanvas : MonoBehaviour
    {
        /// <summary>
        /// 材质中用于存储低分辨率纹理的属性名。
        /// </summary>
        const string materialVariableName = "_CameraOutputTex";

        /// <summary>
        /// 画布所使用的材质引用。
        /// </summary>
        Material canvasMaterial;

        void OnEnable()
        {
            this.Initialize();  // 在启用时进行初始化
        }

        /// <summary>
        /// 初始化放大画布：检查并获取渲染器和材质，校正自身父物体的缩放。
        /// </summary>
        void Initialize()
        {
            // 1. 检查是否存在 MeshRenderer
            if (!this.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                Debug.LogError("需要一个 MeshRenderer 组件来显示放大画布！");
            }
            else
            {
                // 2. 获取所使用的材质
                this.canvasMaterial = meshRenderer.sharedMaterial;
                if (this.canvasMaterial == null)
                {
                    Debug.LogError("canvasMaterial 为 null，请在 MeshRenderer 上设置材质！");
                }
            }

            // 3. 检查父物体的缩放是否为 (1,1,1)
            if (this.transform.parent.localScale != Vector3.one)
            {
                Debug.LogWarning("UpscaledCanvas 的父物体缩放应为 Vector3.one，现已自动修正！");
                this.transform.parent.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 判断该材质中是否包含 RenderTexture 属性。
        /// </summary>
        public bool MaterialHasRenderTexture => this.canvasMaterial.HasProperty(materialVariableName);

        /// <summary>
        /// 根据给定的宽高比和高度调整画布大小。
        /// </summary>
        /// <param name="aspect">宽高比（宽 / 高）</param>
        /// <param name="canvasHeight">画布的目标高度</param>
        public void ResizeCanvas(float aspect, float canvasHeight)
        {
            // 将画布缩放设置为 ( 宽度, 高度, 1 )
            this.transform.localScale = new Vector3(canvasHeight * aspect, canvasHeight, 1f);
        }

        /// <summary>
        /// 设置材质中的 RenderTexture，用于显示游戏相机的低分辨率渲染结果。
        /// </summary>
        /// <param name="renderTexture">要显示的 RenderTexture</param>
        public void SetCanvasRenderTexture(RenderTexture renderTexture)
        {
            // 将 renderTexture 赋值到材质的 "_LowResTexture" 属性
            this.canvasMaterial.SetTexture(materialVariableName, renderTexture);
        }
    }
}