using System;
using System.Collections.Generic;
using Herbs;
using UnityEngine;

namespace DefaultNamespace
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance;
        public GameState gameState = 0;

        [Header("抓药的相机参数")] public GameView ZhuaYao;
        [Header("磨药的相机参数")] public GameView MoYao;
        [Header("煎药的相机参数")] public GameView JianYao;
        
        
            
            
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
            JianYaoHandler.instance.InitializeJianyao();
        }
        
        
    }

    [Serializable]
    public class GameView
    {
        public Transform transform;
        public int CameraRes = 200;
        public bool isPerspective = true;
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