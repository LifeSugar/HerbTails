using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    // 调用此方法即可切换到指定场景
    public void GoToPatientScene()
    {
        SceneManager.LoadScene("prototype"); // 替换为你的场景名称
    }
    public void QuitGame()
    {
        Debug.Log("Exiting game..."); // 编辑器中不会退出，开发时可见提示
        Application.Quit();
    }
}
