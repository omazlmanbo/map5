using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 打印撞到的物体名字，看看有没有反应
        Debug.Log("触发了碰撞，撞到的物体是: " + collision.name);

        if (collision.CompareTag("Player"))
        {
            Debug.Log("检测到玩家！正在尝试跳转场景...");
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.Log("撞到的不是玩家，标签是: " + collision.tag);
        }
    }
}