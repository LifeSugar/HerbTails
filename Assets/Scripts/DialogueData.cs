using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite characterPortrait;
    [TextArea(3, 5)] public string dialogueText;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/New Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines;
        
    public event System.Action<int> OnDialogueLineChanged; //这里用一个事件来管理到哪一行触发方法

    public void RaiseOnDialogueLineChanged(int lineIndex)
    {
        OnDialogueLineChanged?.Invoke(lineIndex);
    }
    
}