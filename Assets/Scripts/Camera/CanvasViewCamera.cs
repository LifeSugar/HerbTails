using UnityEngine;

namespace PixelPerfectURP
{
     /// <summary>
    /// UI相机，通过调整像素化游戏相机在放大显示时的位置来平滑UI
    /// </summary>
    [ExecuteInEditMode]
	public class CanvasViewCamera : MonoBehaviour
    {
        Camera canvasCamera; // UI相机组件
        public float Aspect => this.canvasCamera.aspect; // 相机的宽高比
        public float Zoom { get; private set; } // 当前缩放倍率

        void OnEnable()
		{
            this.Zoom = -1;
            this.Initialize(); // 初始化组件
        }

        /// <summary>
        /// 初始化此组件
        /// </summary>
        void Initialize()
        {
            if (!this.TryGetComponent(out this.canvasCamera)) // 检查并获取 Camera 组件
            {
                Debug.LogError("需要一个 Camera 组件！"); // 如果没有相机组件，输出错误信息
            }
            else if (this.canvasCamera.orthographic == false) // 检查相机是否为正交模式
            {
                Debug.LogWarning("像素相机系统仅支持正交模式。已将视图相机切换为正交模式！");
                this.canvasCamera.orthographic = true; // 将相机模式切换为正交
            }
        }

        /// <summary>
        /// 调整在显示画布上的位置以平滑移动
        /// </summary>
        /// <param name="targetViewPosition">目标视图位置</param>
        /// <param name="canvasLocalScale">画布的本地缩放</param>
        public void AdjustSubPixelPosition(Vector2 targetViewPosition, Vector2 canvasLocalScale)
		{
            var localPosition = (targetViewPosition - new Vector2(0.5f, 0.5f)) * canvasLocalScale; // 计算相对于画布中心的位置
            this.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -1f); // 更新相机的本地位置
        }

        /// <summary>
        /// 设置相机的正交缩放
        /// </summary>
        /// <param name="inputZoom">输入的缩放倍率</param>
        /// <param name="halfCanvasHeight">画布高度的一半</param>
        public void SetZoom(float inputZoom, float halfCanvasHeight)
        {
            this.canvasCamera.orthographicSize = inputZoom * halfCanvasHeight; // 根据缩放倍率调整正交尺寸
            this.Zoom = inputZoom;
        }

        /// <summary>
        /// 设置相机的近裁剪平面和远裁剪平面
        /// </summary>
        /// <param name="near">近裁剪平面</param>
        /// <param name="far">远裁剪平面</param>
        public void SetClipPlanes(float near, float far)
        {
            this.canvasCamera.nearClipPlane = near;
            this.canvasCamera.farClipPlane = far;
        }
    }
}