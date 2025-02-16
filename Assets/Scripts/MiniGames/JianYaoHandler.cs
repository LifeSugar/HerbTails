using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Herbs
{
    public class JianYaoHandler : MonoBehaviour
    {
        [Header("扇子")] public ShanZi shanzi;
        public bool shanziInHand;

        [Header("锅")] public BoilPot pot;

        [Header("UI")] public RectTransform JianYaoPanel;
        public RectTransform marker;
        public float speedControl = 1f; //整体修正
        public GameObject smallFireIcon;
        public GameObject middleFireIcon;
        public GameObject largeFireIcon;

        // Marker活动范围
        public float minX = 30f;
        public float maxX = 300f;

        // Marker基础移动速度
        public float markerSpeed = -50f;

        // 原先用来判断火力区间的三个关键值
        public float noFireRange;
        public float smallFireRange;
        public float middleFireRange;

        [Header("火力")] public FirePower currentFirePower;
        // 是否开始煮药（只要出现过点击就开始）
        public bool boiling = false;

        [Header("火力切换『滞后区』(避免边缘抖动)")] public float hysteresis = 5f; // 进入与退出同一区间时的缓冲带

        // ========== 点击频率控火参数 ==========
        [Header("控火参数")] public float timeWindow = 1f;
        public float smoothingSpeed = 5f;
        public float thresholdFreq = 2f;
        public float maxFreq = 10f;
        public float maxValue = 150f;
        
        // ========== 点击频率计算 ==========
        private List<float> clickTimes = new List<float>();
        private float smoothedFreq = 0f;
        [FormerlySerializedAs("currentValue")] public float clickFrequency = 0f;

        // 单例
        public static JianYaoHandler instance { get; private set; }

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("there is more than one JianYaoHandler in the scene");
            }

            instance = this;
        }

        void Start()
        {
            pot = this.gameObject.GetComponentInChildren<BoilPot>();
            shanzi = this.gameObject.GetComponentInChildren<ShanZi>();
            InputHandler.instance.OnStateChange += SwitchJianYao;
        }

        /// <summary>
        /// 切换到/退出“煎药”界面时的UI动画
        /// </summary>
        public void SwitchJianYao(GameState gameState, GameState previousGameState)
        {
            Debug.Log($"从 {previousGameState} 切换到 {gameState}");

            if (gameState == GameState.JIANYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                // 如果是从 TOPDOWN 切换过来，把炼金UI收起
                if (previousGameState == GameState.TOPDOWN)
                {
                    RectTransform craftUI = InputHandler.instance.craftUICanvas;
                    Vector2 targetPos = new Vector2(706, 37);
                    craftUI.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad)
                        .OnComplete(() => { InputHandler.instance.craftUIOn = true; });
                }

                // 把煎药面板移动到可视区域
                Vector2 targetPos_FireBar = new Vector2(-283.5f, 400f);
                JianYaoPanel.DOAnchorPos(targetPos_FireBar, duration).SetEase(Ease.OutQuad);
            }
            else if (previousGameState == GameState.JIANYAO)
            {
                float duration = InputHandler.instance.transitionDuration;
                // 把煎药面板挪到屏幕外
                Vector2 targetPos_FireBar = new Vector2(-283.5f, -1600f);
                JianYaoPanel.DOAnchorPos(targetPos_FireBar, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() => { JianYaoPanel.localPosition = new Vector2(-283.5f, 600f); });
            }
        }

        /// <summary>
        /// 由 InputHandler 的 Update 调用
        /// </summary>
        public void Tick()
        {
            // 扇子是否在手上，如果在，则可以控制火力
            shanzi.SwitchShanzi();
            if (shanziInHand)
            {
                ControlFire(); // 计算 currentValue（基于点击频率）
                shanzi.Wave(clickFrequency); // 扇子挥动动画或效果
            }
            else
            {
                clickFrequency = 0;
            }
            
            if (boiling)
            {
                UpdateFirePower(); // 根据 marker 的 x 判断当前火力
                UpdateFireIcon(currentFirePower);
                pot.SetAnimationParameter(currentFirePower);
                MarkerMove(markerSpeed,MarkerSpeedOffset(clickFrequency));
            }
        }

        /// <summary>
        /// 计算点击频率 --> currentValue
        /// </summary>
        void ControlFire()
        {
            // 1. 检测点击
            if (Input.GetMouseButtonDown(0))
            {
                clickTimes.Add(Time.time);
            }

            // 2. 移除超过窗口期的点击记录
            while (clickTimes.Count > 0 && (Time.time - clickTimes[0] > timeWindow))
            {
                clickTimes.RemoveAt(0);
            }

            // 3. 计算滑动窗口内的“瞬时频率”（次/秒）
            float rawFreq = clickTimes.Count / timeWindow;

            // 4. 用插值的方式，让 smoothedFreq 逐渐逼近 rawFreq
            smoothedFreq = Mathf.Lerp(smoothedFreq, rawFreq, Time.deltaTime * smoothingSpeed);

            // 5. 根据 smoothedFreq 进行阈值判断 & [0, maxValue] 映射
            if (smoothedFreq < thresholdFreq)
            {
                clickFrequency = 0f;
            }
            else
            {
                float t = (smoothedFreq - thresholdFreq) / (maxFreq - thresholdFreq);
                t = Mathf.Clamp01(t);
                clickFrequency = t * maxValue;
            }

            // 一旦有了非零火力值，我们就设 boiling = true
            if (clickFrequency > 0 && !boiling)
            {
                boiling = true;
            }
        }


        //----------------
        // 定义在类中作为成员变量，用于 SmoothDamp 内部计算速度
        private float markerVelocityX = 0f;

        // 这个 smoothTime 越大越平滑
        public float smoothTime = 0.1f;

        /// <summary>
        /// 移动火力指示器Marker。移动方向由当前火力等级决定。
        /// </summary>
        private void MarkerMove(float selfSpeed, float offsetSpeed)
        {
            Vector2 currentPos = marker.anchoredPosition;
            float factor = 0f;

            // 根据当前火力等级确定系数
            switch (currentFirePower)
            {
                case FirePower.NONE:
                    factor = 0.5f;
                    break;
                case FirePower.SMALL:
                    factor = 0.8f;
                    break;
                case FirePower.MIDDLE:
                    factor = 1.3f;
                    break;
                case FirePower.LARGE:
                    factor = 1.8f;
                    break;
            }

            // 计算目标位置
            float targetX = currentPos.x + (selfSpeed * factor + offsetSpeed) * Time.deltaTime * speedControl; ;

            // 使用 SmoothDamp 来平滑过渡到目标位置
            float newX = Mathf.SmoothDamp(currentPos.x, targetX, ref markerVelocityX, smoothTime);

            // 限制在 [minX, maxX] 范围内
            newX = Mathf.Clamp(newX, minX, maxX);
            currentPos.x = newX;
            marker.anchoredPosition = currentPos;
        }

        /// <summary>
        /// 【核心改动】使用“滞后区(Hysteresis)”来防止在区间临界值处抖动
        /// </summary>
        private void UpdateFirePower()
        {
            float x = marker.anchoredPosition.x;

            // 计算不同区间“进入”阈值和“退出”阈值
            float smallEnter = noFireRange + hysteresis; // NONE -> SMALL
            float smallExit = noFireRange - hysteresis; // SMALL -> NONE

            float middleEnter = smallFireRange + hysteresis; // SMALL -> MIDDLE
            float middleExit = smallFireRange - hysteresis; // MIDDLE -> SMALL

            float largeEnter = middleFireRange + hysteresis; // MIDDLE -> LARGE
            float largeExit = middleFireRange - hysteresis; // LARGE -> MIDDLE

            switch (currentFirePower)
            {
                case FirePower.NONE:
                    // 只有当 x > smallEnter 才从 NONE 进入 SMALL
                    if (x > smallEnter)
                    {
                        currentFirePower = FirePower.SMALL;
                    }

                    break;

                case FirePower.SMALL:
                    // 如果 x < smallExit 则回到 NONE
                    if (x < smallExit)
                    {
                        currentFirePower = FirePower.NONE;
                    }
                    // 如果 x > middleEnter 则进入 MIDDLE
                    else if (x > middleEnter)
                    {
                        currentFirePower = FirePower.MIDDLE;
                    }

                    break;

                case FirePower.MIDDLE:
                    // 如果 x < middleExit 则退回 SMALL
                    if (x < middleExit)
                    {
                        currentFirePower = FirePower.SMALL;
                    }
                    // 如果 x > largeEnter 则进入 LARGE
                    else if (x > largeEnter)
                    {
                        currentFirePower = FirePower.LARGE;
                    }

                    break;

                case FirePower.LARGE:
                    // 如果 x < largeExit 则退回 MIDDLE
                    if (x < largeExit)
                    {
                        currentFirePower = FirePower.MIDDLE;
                    }

                    break;
            }
        }

        private void UpdateFireIcon(FirePower firePower)
        {
            switch (firePower)
            {
                case FirePower.NONE:
                    smallFireIcon.SetActive(false);
                    middleFireIcon.SetActive(false);
                    largeFireIcon.SetActive(false);
                    break;
                case FirePower.SMALL:
                    smallFireIcon.SetActive(true);
                    middleFireIcon.SetActive(false);
                    largeFireIcon.SetActive(false);
                    break;
                case FirePower.MIDDLE:
                    smallFireIcon.SetActive(false);
                    middleFireIcon.SetActive(true);
                    largeFireIcon.SetActive(false);
                    break;
                case FirePower.LARGE:
                    smallFireIcon.SetActive(false);
                    middleFireIcon.SetActive(false);
                    largeFireIcon.SetActive(true);
                    break;
                default:
                    break;
            }
        }
        
        

        /// <summary>
        /// 根据点击频率currentValue，额外增加的marker移动速度
        /// （示例：越快点击 -> markerSpeedOffset越大 -> marker移动速度越快）
        /// </summary>
        private float MarkerSpeedOffset(float currency)
        {
            float offset = 0f;
            if (currency > 75)
            {
                offset = 2800;
            }
            else if (currency > 40)
            {
                offset = 1800;
            }
            else if (currency > 10)
            {
                offset = 1200;
            }
            else if (currency > 5)
            {
                offset = 600;
            }
            else
            {
                offset = 0;
            }

            return offset;
        }
    }

    [System.Serializable]
    public enum FirePower
    {
        NONE = 0,
        SMALL = 1,
        MIDDLE = 2,
        LARGE = 3
    }
}