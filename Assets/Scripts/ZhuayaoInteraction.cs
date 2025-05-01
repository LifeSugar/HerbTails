using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HT
{
public class ZhuayaoInteraction : MonoBehaviour
{
   public DialogueData startDialogue;
        public GameView desk;

        void Start()
        {
            startDialogue.OnDialogueLineChanged += FocusOndesk;
            startDialogue.OnDialogueLineChanged += MoveBack;
            if (startDialogue != null)
            {
                DialogueManager.instance.currentData = startDialogue;
                InputHandler.instance.inDialogue = true;
                DialogueManager.instance.StartDialogue();
            }
        }

        void FocusOndesk(int currentline)
        {
            if (currentline != 3)
            {
                return;
            }
            else
            {
                InputHandler.instance.SetGameView(desk, GameState.INSCENE);
                startDialogue.OnDialogueLineChanged -= FocusOndesk;
            }
        }

        void MoveBack(int currentline)
        {
            if (currentline != 4)
            {
                return;
            }
            else
            {
                // Debug.Log("Moving back");
                var view = InputHandler.instance.rightSceneView;
                InputHandler.instance.SetGameView(view, GameState.INSCENE);
                startDialogue.OnDialogueLineChanged -= MoveBack;
            }
        }
}
}
