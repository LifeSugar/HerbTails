using System.Collections;
using System.Collections.Generic;
using HT;
// using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    // 当前对话数据（在交互时传入特定的对话数据）
    public DialogueData currentData;

    // 当前文本在对话数据中的行索引
    private int currentLineIndex = 0;

    // 标记整体对话是否处于激活状态
    private bool isDialogueActive = false;

    // 标记当前是否正处于逐字显示的打字机效果中
    private bool isTyping = false;

    // 当前正在显示文本的完整内容，用于跳过打字机效果时直接显示完整文本
    private string currentFullSentence;

    // 用来保存正在运行的逐字显示协程引用，这样可用于停止协程
    private Coroutine typeSentenceCoroutine = null;

    // UI相关组件：TextMeshProUGUI 用于显示角色名字及对话文本；Image 用于显示角色头像
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterName;
    public Image characterPortrait;
    public GameObject dialoguePanel;
    // public GameObject endDialoguePanel;

    // 每个字符显示之间的延迟时间（单位：秒），可根据需要调整打字速度
    [Header("每个字符出现的间隔")]
    public float letterDelay = 0.05f;

    // 周期性调用发出声音的的间隔时间（单位：秒），此处设置为 0.3 秒
    [Header("发出声音的间隔")]
    public float periodicInterval = 0.3f;

    // [Header("小动物的声音")] 
    // public EventReference voice;
    //
    // public EventReference monkeyvoice;

    // 单例模式，方便其他脚本通过 DialogueManager.instance 来访问
    public static DialogueManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instance of DialogueManager");
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnsceneLoaded;
    }

    private void OnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Scene Loaded: " + arg0.name);
        SceneManager.MoveGameObjectToScene(this.gameObject, arg0);
    }

    void Start()
    {
        
    }

    /// <summary>
    /// Tick 方法：负责处理玩家输入逻辑
    /// 当对话激活时，监听空格键或鼠标左键：
    ///   - 如果正在打字，则立即停止打字协程，并显示完整文本
    ///   - 否则进入下一行对话
    /// 此方法将在其他脚本的 Update 方法中调用，而不在本脚本内定义 Update
    /// </summary>
    public void Tick()
    {
        if (!isDialogueActive)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 如果正在打字效果中，则停止协程，并直接显示完整文本
                if (typeSentenceCoroutine != null)
                {
                    StopCoroutine(typeSentenceCoroutine);
                    typeSentenceCoroutine = null;
                }

                dialogueText.text = currentFullSentence;
                isTyping = false;
            }
            else
            {
                // 如果当前不在打字状态，则进入下一行对话
                NextDialogueLine();
            }
        }
    }

    /// <summary>
    /// 开始对话：初始化对话面板、行索引，并显示第一行对话
    /// 同时调用 RaiseOnDialogueLineChanged 事件通知其他系统当前对话行
    /// </summary>
    public void StartDialogue()
    {
        if (currentData.lines.Count > 0)
        {
            isDialogueActive = true;
            dialoguePanel.SetActive(true);
            // endDialoguePanel.SetActive(false);
            currentLineIndex = 0;
            ShowLine();
            currentData.RaiseOnDialogueLineChanged(currentLineIndex);
        }

        InputHandler.instance.inDialogue = true;
        // GameManager.instance.gameState = GameState.InDialogue;
    }

    /// <summary>
    /// 进入下一行对话，如果超出总行数则结束对话
    /// </summary>
    void NextDialogueLine()
    {
        currentLineIndex++;
        if (currentLineIndex < currentData.lines.Count)
        {
            ShowLine();
            currentData.RaiseOnDialogueLineChanged(currentLineIndex);
        }
        else
        {
            currentData.RaiseOnDialogueLineChanged(currentLineIndex);
            EndDialogue();
            
        }
    }

    /// <summary>
    /// 显示当前行对话：设置角色名字、头像，并启动逐字显示文本的协程
    /// </summary>
    void ShowLine()
    {
        DialogueLine line = currentData.lines[currentLineIndex];
        characterName.text = line.characterName;
        characterPortrait.sprite = line.characterPortrait;

        // 保存完整的对话文本，用于逐字显示和跳过时直接显示
        currentFullSentence = line.dialogueText;

        // 如果已有正在运行的逐字显示协程则先停止它
        if (typeSentenceCoroutine != null)
        {
            StopCoroutine(typeSentenceCoroutine);
        }

        // 启动逐字显示文本的协程
        typeSentenceCoroutine = StartCoroutine(TypeSentence(currentFullSentence));
    }

    /// <summary>
    /// TypeSentence 协程：实现逐字显示文本（打字机效果），并在过程中每隔周期性时间调用一次指定方法
    /// </summary>
    /// <param name="sentence">完整对话文本</param>
    /// <returns></returns>
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true; // 标记打字开始
        dialogueText.text = ""; // 清空文本显示
        float periodicTimer = 0f; // 定时器，用于控制周期性方法调用

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(letterDelay);
            // 累加延时值到周期定时器
            periodicTimer += letterDelay;
            // 每当定时器大于或等于指定的周期间隔时，调用一次指定方法，并重置定时器
            if (periodicTimer >= periodicInterval)
            {
                CallPeriodicMethod();
                periodicTimer = 0f;
            }
        }

        isTyping = false; // 打字结束
        typeSentenceCoroutine = null;
    }

    /// <summary>
    /// CallPeriodicMethod 方法：每隔一定时间调用一次
    /// 即每间隔一段事件发出一次声音
    /// </summary>
    void CallPeriodicMethod()
    {
        // 这里还可以根据currentData.line.charactorname来判断是哪个角色，播放不同的音效
         // AudioManager.instance.PlayOneShot(voice, Camera.main.transform.position);
         // if (currentData.lines[currentLineIndex].characterName != "monkey")
         // {
         //     AudioManager.instance.PlayOneShot(monkeyvoice, Camera.main.transform.position);
         // }
    }

    /// <summary>
    /// 结束对话：关闭对话面板，激活结束对话的 UI，同时还原游戏状态
    /// </summary>
    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        // GameManager.instance.gameState = GameState.ThirdPerson;
        InputHandler.instance.inDialogue = false;
    }
}