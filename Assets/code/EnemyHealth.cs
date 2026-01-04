using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 接收伤害的公共方法
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " 受到伤害: " + amount + " | 剩余血量: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 已死亡！");
        // 这里可以添加销毁物体或播放死亡动画的逻辑
        // Destroy(gameObject); 
    }
}