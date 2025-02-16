using System;
using PixelPerfectURP;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Herbs
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance;
        public GameState gameStateNow = 0;

        [Header("抓药的相机参数")] public GameView ZhuaYao;
        [Header("磨药的相机参数")] public GameView MoYao;
        [Header("煎药的相机参数")] public GameView JianYao;
        [Header("记录变换前的相机参数")] public GameView previousGameView;
        
        // 用来跟踪当前是否在插值过渡中
        private bool isTransitioning = false;
        // 插值时长
        public float transitionDuration = 1f;
        
        [Header("制药过程的UI是否打开")] public bool craftUIOn = false;
        public RectTransform craftUICanvas;
        
        //构建事件，状态转换时通知各个“制药小游戏”。
        
        public delegate void stateChangeHandler(GameState gameState, GameState previousGameState);
        public event stateChangeHandler OnStateChange;

        
            
            
        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one InputHandler in scene.");
            }
            instance = this;
        }

        private void Start()
        {
            
        }

        void Update()
        {
            if (!isTransitioning)
            {
                //测试
                if (Input.GetKeyDown(KeyCode.J))
                {
                    SwitchGameState(GameState.JIANYAO);
                }

                if (Input.GetKeyDown(KeyCode.T))
                {
                    SwitchGameState(GameState.TOPDOWN);
                }
                
                GameStateHandler();
            }
            
        }

        void GameStateHandler()
        {
            switch (gameStateNow)
            {
                case GameState.JIANYAO:
                    JianYaoHandler.instance.Tick();
                    break;
                default:
                    break;
            }
        }

        public void SwitchGameState( GameState gameState )
        {
            switch (gameState)
            {
                case GameState.TOPDOWN:
                    InitalizeTopDownGameView();
                    SetGameView(previousGameView, GameState.TOPDOWN); 
                    break;
                case GameState.ZHUAYAO:
                    SetGameView(ZhuaYao, GameState.ZHUAYAO);
                    break;
                case GameState.MOYAO:
                    SetGameView(MoYao, GameState.MOYAO);
                    break;
                case GameState.JIANYAO:
                    SetGameView(JianYao, GameState.JIANYAO);
                    break;
                default:
                    break;
            }
        }

        private void SetGameView(GameView gameView, GameState gameState)
        {
            //触发场景变换事件
            OnStateChange?.Invoke(gameState, gameStateNow );
            if (gameStateNow == GameState.TOPDOWN)
            {
                previousGameView.SetGameView(PixelCameraManager.Instance.transform, PixelCameraManager.Instance.GameResolution.y, PixelCameraManager.Instance.UsePerspective);
            }
            // 标记进入过渡
            isTransitioning = true;
            
            // 1. 相机位置与旋转的插值

            // 创建一个 DOTween 的序列 (Sequence) 可并行或串行地执行多个 tween
            Sequence seq = DOTween.Sequence();
            // 相机移动插值
            seq.Append(
                PixelCameraManager.Instance.transform.parent.transform
                    .DOMove(gameView.transform.position, transitionDuration)
                    .SetEase(Ease.OutQuad)
            );
            // 同时插值旋转（使用四元数可避免万向节问题）
            seq.Join(
                PixelCameraManager.Instance.transform.parent.transform
                    .DORotateQuaternion(gameView.transform.rotation, transitionDuration)
                    .SetEase(Ease.OutQuad)
            );
            // 2. 分辨率插值 (整型)
            
            int startRes = PixelCameraManager.Instance.GameResolution.y;
            int endRes   = gameView.cameraRes;
            // 将分辨率插值加入同一个序列中
            seq.Join(
                DOTween.To(
                    () => startRes,         // 读取初始值
                    x => {                  // 写入过程，每帧更新
                        int i = x;
                        //仅按 Y 插值
                        PixelCameraManager.Instance.GameResolution =
                            new Vector2Int(0, i);
                    },
                    endRes,                // 目标分辨率
                    transitionDuration     // 插值时间
                ).SetEase(Ease.Flash)
            );

            // 3. Tween 结束后的回调
            seq.OnComplete(() => 
            {
                // 在插值结束后再切换相机模式
                // PixelCameraManager.Instance.SwitchCameraMode(gameView.isPerspective);
                gameStateNow = gameState;

                // 过渡结束
                isTransitioning = false;
            });
            // 开始执行序列
            seq.Play();
        }

        private void InitalizeTopDownGameView()
        {
            craftUICanvas.gameObject.SetActive(true);
            Vector2 targetPos = new Vector2(706, 1100);
            craftUICanvas.DOAnchorPos(targetPos, 0.56f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    craftUIOn = false;
                });
        }
        
        
    }

    [Serializable]
    public class GameView
    {
        public Transform transform;
        public int cameraRes;
        public bool isPerspective;

        public void SetGameView(Transform transform, int cameraRes, bool isPerspective)
        {
            this.transform.position = transform.position;
            this.transform.rotation = transform.rotation;
            this.cameraRes = cameraRes;
            this.isPerspective = isPerspective;
        }
    }
    
    [Serializable]
    public enum GameState
    {
        TOPDOWN = 0,
        ZHUAYAO = 1,
        MOYAO = 2,
        JIANYAO = 3
    }
}