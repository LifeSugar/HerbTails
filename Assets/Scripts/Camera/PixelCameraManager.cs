using UnityEngine;

namespace PixelPerfectURP
{
    /// <summary>
    /// 为 Unity 检视面板（Inspector）中的字段提供的中文提示
    /// </summary>
    public static class Tooltips
    {
        public const string TT_FOLLOWED_TRANSFORM =
            "此相机需要跟随的Transform，用于像素级修正。";
        
        public const string TT_GRID_MOVEMENT =
            "相机在体素网格上移动，以避免静止对象出现抖动。";
        
        public const string TT_SUB_PIXEL =
            "亚像素调整，用于抵消网格对齐时的块状感。";
        
        public const string TT_FOLLOW_ROTATION =
            "相机是否跟随被跟随物体的旋转。";
        
        public const string TT_GAME_RESOLUTION =
            "游戏渲染纹理分辨率。值越低，像素化越明显。";
        
        public const string TT_RESOLUTION_SYNCHRONIZATION_MODE =
            "如何根据显示宽高比计算 GameResolution。";
        
        public const string TT_CONTROL_GAME_ZOOM =
            "是否由本脚本来控制游戏相机的正交大小。";
        
        public const string TT_GAME_ZOOM =
            "通过调整相机正交大小，控制场景的放大倍率。";
        
        public const string TT_VIEW_ZOOM =
            "在像素大小不变的前提下，拉近或拉远视图相机的画面。";
        
        public const string TT_USE_PERSPECTIVE =
            "是否使用透视相机（而非正交），部分像素相关特性将会被忽略。";
    }

    /// <summary>
    /// 管理整个像素摄像机系统的脚本（可切换正交或透视）
    /// </summary>
    [ExecuteInEditMode]
    public class PixelCameraManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static PixelCameraManager Instance { get; private set; }

        [Header("Camera Type")]
        [Tooltip(Tooltips.TT_USE_PERSPECTIVE)]
        public bool UsePerspective = false; // 是否使用透视相机

        [Tooltip(Tooltips.TT_FOLLOWED_TRANSFORM)]
        public Transform FollowedTransform;

        [Header("Settings")]
        [Tooltip(Tooltips.TT_GRID_MOVEMENT)]
        public bool VoxelGridMovement = true;
        [Tooltip(Tooltips.TT_SUB_PIXEL)]
        public bool SubpixelAdjustments = true;
        [Tooltip(Tooltips.TT_FOLLOW_ROTATION)]
        public bool FollowRotation = true;

        [Header("Resolution")]
        [Tooltip(Tooltips.TT_RESOLUTION_SYNCHRONIZATION_MODE)]
        public ResolutionSynchronizationMode resolutionSynchronizationMode = 
            ResolutionSynchronizationMode.SetHeight;

        [Tooltip(Tooltips.TT_GAME_RESOLUTION)]
        public Vector2Int GameResolution = new Vector2Int(640, 360);

        [Header("Zoom")]
        [Tooltip(Tooltips.TT_CONTROL_GAME_ZOOM)]
        public bool ControlGameZoom = true;
        [Tooltip(Tooltips.TT_GAME_ZOOM)]
        public float GameCameraZoom = 5f;
        [Tooltip(Tooltips.TT_VIEW_ZOOM)]
        [Range(-1f, 1f)]
        public float ViewCameraZoom = 1f;

        // 内部引用
        Camera gameCamera;
        CanvasViewCamera viewCamera;
        UpscaledCanvas upscaledCanvas;

        float renderTextureAspect;

        #region 单例初始化
        private void Awake()
        {
            // 确保只有一个实例
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        void OnEnable()
        {
            this.Initialize();
        }

        void LateUpdate()
        {
            this.UpdateCameraSystem();
        }

        /// <summary>
        /// 相机像素在世界空间中对应的大小（仅正交模式有意义）
        /// </summary>
        float PixelWorldSize
            => 2f * this.gameCamera.orthographicSize / this.gameCamera.pixelHeight;

        /// <summary>
        /// 获取当前相机的目标纹理分辨率
        /// </summary>
        Vector2Int TargetTextureResolution
            => this.gameCamera.targetTexture == null
               ? Vector2Int.left
               : new Vector2Int(
                     this.gameCamera.targetTexture.width,
                     this.gameCamera.targetTexture.height);

        /// <summary>
        /// 将给定世界坐标对齐到像素网格上（仅在正交模式下有用）
        /// </summary>
        public Vector3 PositionToGrid(Vector3 worldPosition)
        {
            // 如果是透视相机，直接返回原始坐标
            if (this.UsePerspective) return worldPosition;

            // 正交模式下，才计算网格对齐
            var localPosition = this.transform.InverseTransformDirection(worldPosition);
            var localPositionInPixels = localPosition / this.PixelWorldSize;
            var integerMovement = (Vector3)Vector3Int.RoundToInt(localPositionInPixels);
            var movement = integerMovement * this.PixelWorldSize;
            return (movement.x * this.transform.right)
                 + (movement.y * this.transform.up)
                 + (movement.z * this.transform.forward);
        }

        /// <summary>
        /// 安全设置游戏相机正交大小，避免出现 size = 0 的情况
        /// </summary>
        float SetGameZoom(float zoom)
        {
            var checkedZoom = Mathf.Approximately(zoom, 0f) ? 0.01f : zoom;
            this.gameCamera.orthographicSize = checkedZoom;
            return checkedZoom;
        }

        /// <summary>
        /// 同步视图相机的裁剪平面
        /// </summary>
        void SynchronizeClipPlanes()
        {
            // 若是透视，使用同样的 near/far
            if (this.UsePerspective)
            {
                this.viewCamera.SetClipPlanes(
                    this.gameCamera.nearClipPlane,
                    this.gameCamera.farClipPlane
                );
            }
            else
            {
                // 正交模式下，考虑到视图相机本地 Z 偏移
                this.viewCamera.SetClipPlanes(
                    0f,
                    this.gameCamera.farClipPlane - this.viewCamera.transform.localPosition.z
                );
            }
        }

        /// <summary>
        /// 初始化：获取相机、视图相机与放大画布，并做层级和属性检查
        /// </summary>
        private void Initialize()
        {
            // 获取游戏相机
            if (this.gameCamera == null)
            {
                if (!this.TryGetComponent(out this.gameCamera))
                {
                    Debug.LogError("Camera component not found on PixelCameraManager!");
                }
            }

            // 根据 UsePerspective 切换相机模式
            this.gameCamera.orthographic = !this.UsePerspective;

            // 获取视图相机
            if (this.viewCamera == null)
            {
                this.viewCamera = FindAnyObjectByType(typeof(CanvasViewCamera)) as CanvasViewCamera;
                if (this.viewCamera == null)
                {
                    Debug.LogError("viewCamera is null. Please assign a CanvasViewCamera in the scene!");
                }
            }

            // 获取放大画布
            if (this.upscaledCanvas == null)
            {
                this.upscaledCanvas = FindAnyObjectByType(typeof(UpscaledCanvas)) as UpscaledCanvas;
                if (this.upscaledCanvas == null)
                {
                    Debug.LogError("upscaledCanvas is null. Please assign an UpscaledCanvas in the scene!");
                }
            }

            // 检查层级关系
            if (this.transform.parent == null)
            {
                Debug.LogError("No parent object found. Check your prefab setup!");
            }
            else if (this.transform.parent.childCount > 2)
            {
                Debug.LogWarning("PixelCameraManager's parent usually only has 2 children: This manager and the target transform.");
            }

            // 被跟随的变换
            if (this.FollowedTransform == null)
            {
                Debug.LogError("FollowedTransform is null. Please set it in the inspector.");
            }

            // 同步裁剪平面
            this.SynchronizeClipPlanes();
        }

        /// <summary>
        /// 设置新的渲染纹理到画布和游戏相机，并记录当前宽高比
        /// </summary>
        void SetRenderTexture(float aspect, RenderTexture newRenderTexture)
        {
            this.upscaledCanvas.SetCanvasRenderTexture(newRenderTexture);
            this.gameCamera.targetTexture = newRenderTexture;
            this.renderTextureAspect = aspect;
        }

        /// <summary>
        /// 每帧更新整个像素相机系统
        /// </summary>
        void UpdateCameraSystem()
        {
            // 1. 检测宽高比或分辨率是否变化
            var aspectRatioChanged = (this.viewCamera != null) &&
                                     (this.renderTextureAspect != this.viewCamera.Aspect);
            var pixelResolutionChanged = this.GameResolution != this.TargetTextureResolution;
            var resizeCanvas = false;

            if (aspectRatioChanged || pixelResolutionChanged || this.gameCamera.targetTexture == null)
            {
                // 根据视图相机实际宽高比进行同步计算
                if (this.viewCamera != null)
                {
                    this.GameResolution = RenderTextureFunctions.TextureResultion(
                        this.viewCamera.Aspect,
                        this.GameResolution,
                        this.resolutionSynchronizationMode
                    );
                }

                // 释放旧纹理
                if (this.gameCamera.targetTexture != null)
                {
                    this.gameCamera.targetTexture.Release();
                }

                // 创建新纹理
                var newRenderTexture = RenderTextureFunctions.CreateRenderTexture(this.GameResolution);

                // 设置到画布和相机
                if (this.viewCamera != null)
                {
                    this.SetRenderTexture(this.viewCamera.Aspect, newRenderTexture);
                }
                else
                {
                    this.SetRenderTexture(1f, newRenderTexture);
                }

                resizeCanvas = true;
            }
            else if (Application.isEditor && 
                     this.upscaledCanvas != null && 
                     this.upscaledCanvas.MaterialHasRenderTexture)
            {
                // 在编辑器下，如果材质已绑定 RT，仍然重设一下以防万一
                this.SetRenderTexture(this.renderTextureAspect, this.gameCamera.targetTexture);
                resizeCanvas = true;
            }

            // 若是透视相机，跳过以下「正交专用」逻辑
            if (this.UsePerspective)
            {
                UpdateTransformOnly();
                return;
            }

            // ============= 以下为正交模式下的逻辑 =============

            // 2. 处理游戏相机的缩放 (Orthographic Size)
            var orthographicSizeChanged = 
                !Mathf.Approximately(this.gameCamera.orthographicSize, this.GameCameraZoom);

            if (!this.ControlGameZoom)
            {
                // 若不由本脚本控制，则用实际相机尺寸回写到 GameCameraZoom
                this.GameCameraZoom = this.gameCamera.orthographicSize;
                resizeCanvas = true;
            }

            if (this.ControlGameZoom && orthographicSizeChanged)
            {
                // 若由本脚本控制且尺寸变了，则更新相机正交尺寸
                this.GameCameraZoom = this.SetGameZoom(this.GameCameraZoom);
                resizeCanvas = true;
            }

            // 3. 处理视图相机的缩放
            if (this.viewCamera != null)
            {
                if (orthographicSizeChanged || pixelResolutionChanged ||
                    !Mathf.Approximately(this.ViewCameraZoom, this.viewCamera.Zoom))
                {
                    // 防止分辨率极小导致越界
                    var canvasOnScreenLimit = 1 - (2f / this.GameResolution.y);
                    if (this.GameResolution.y < 3)
                    {
                        canvasOnScreenLimit = 1f;
                        Debug.LogWarning("GameResolution is too small, unexpected behavior may occur.");
                    }

                    // 保证 ViewCameraZoom 不为 0 且在 -1~1 之间
                    this.ViewCameraZoom = Mathf.Approximately(this.ViewCameraZoom, 0f)
                        ? 0.01f
                        : Mathf.Clamp(this.ViewCameraZoom, -1f, 1f);

                    // 设置视图相机缩放
                    this.viewCamera.SetZoom(
                        this.ViewCameraZoom * canvasOnScreenLimit,
                        this.GameCameraZoom
                    );
                }
            }

            // 4. 如果需要，重新调整画布大小
            if (resizeCanvas)
            {
                var gameResolutionAspect = (float)this.GameResolution.x / this.GameResolution.y;
                this.upscaledCanvas.ResizeCanvas(gameResolutionAspect, this.GameCameraZoom * 2f);
            }

            // 5. 更新相机 Transform
            UpdateTransformOnly();

            // 6. 亚像素校正
            if (this.SubpixelAdjustments && this.viewCamera != null)
            {
                var targetViewPosition = 
                    this.gameCamera.WorldToViewportPoint(this.FollowedTransform.position);
                
                this.viewCamera.AdjustSubPixelPosition(
                    targetViewPosition,
                    this.upscaledCanvas.transform.localScale
                );
            }
        }

        /// <summary>
        /// 根据当前设置，更新位置与旋转（包含网格对齐/旋转跟随）
        /// </summary>
        private void UpdateTransformOnly()
        {
            // 跟随目标旋转
            if (this.FollowRotation)
            {
                this.transform.rotation = this.FollowedTransform.rotation;
            }

            // 透视模式下忽略 VoxelGridMovement
            if (this.UsePerspective)
            {
                this.transform.position = this.FollowedTransform.position;
            }
            else
            {
                // 正交下可选择是否对齐像素网格
                if (this.VoxelGridMovement)
                {
                    this.transform.position = this.PositionToGrid(this.FollowedTransform.position);
                }
                else
                {
                    this.transform.position = this.FollowedTransform.position;
                }
            }
        }

        /// <summary>
        /// 公用方法：切换相机模式（正交或透视），并重新初始化
        /// </summary>
        /// <param name="usePerspective">true = 启用透视; false = 启用正交</param>
        public void SwitchCameraMode(bool usePerspective)
        {
            this.UsePerspective = usePerspective;
            // 切换后，需要更新相机属性并重新执行初始化/同步
            if (this.gameCamera != null)
            {
                this.gameCamera.orthographic = !this.UsePerspective;
            }

            // 重新执行初始化
            this.Initialize();
        }
    }
}
