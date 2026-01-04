using UnityEngine;
using UnityEngine.UI; // 必须引用，用于控制屏幕变黑
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("组件引用 (需要手动拖拽)")]
    public Animator animator;              // 玩家的动画控制器
    public MonoBehaviour movementScript;   // 玩家的移动脚本 (例如 PlayerController)
    public Image blackScreenPanel;         // UI上的黑色图片 (用于遮挡屏幕)

    [Header("死亡设置")]
    public string deathAnimTrigger = "Die"; // 动画机里的 Trigger 名字
    public float fadeSpeed = 0.5f;          // 屏幕变黑的速度

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // 游戏开始时，确保黑屏界面是隐藏或透明的
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(false);
            Color c = blackScreenPanel.color;
            c.a = 0; // 透明度设为0
            blackScreenPanel.color = c;
        }
    }

    // --- 供外部 (怪物) 调用的受伤函数 ---
    public void TakeDamage(int damage)
    {
        if (isDead) return; // 死了就不再受伤害

        currentHealth -= damage;
        Debug.Log($"玩家受到伤害: -{damage}, 当前血量: {currentHealth}");

        // 这里可以加一个 受伤动画 (Hurt)
        // if (animator != null) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // 1. 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger(deathAnimTrigger);
        }

        // 2. 停止玩家移动 (禁用移动脚本)
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // 可选：禁用碰撞体，防止尸体挡路
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. 屏幕变黑
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            StartCoroutine(FadeToBlack());
        }
    }

    // 协程：控制透明度渐变
    IEnumerator FadeToBlack()
    {
        Color color = blackScreenPanel.color;
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            blackScreenPanel.color = color;
            yield return null; // 等待下一帧
        }
    }
}