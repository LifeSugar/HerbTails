using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    // ���ô˷��������л���ָ������
    public void GoToPatientScene()
    {
        SceneManager.LoadScene("prototype"); // �滻Ϊ��ĳ�������
    }
    public void QuitGame()
    {
        Debug.Log("Exiting game..."); // �༭���в����˳�������ʱ�ɼ���ʾ
        Application.Quit();
    }
}
