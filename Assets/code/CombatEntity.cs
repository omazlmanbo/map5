using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    [Header("基础设置")]
    public string targetTag; // 攻击目标的Tag (例如 Player设置 "Enemy", Enemy设置 "Player")
    public bool isDead = false;

    // 死亡逻辑
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " 被击败！");

        // 在这里添加死亡特效、音效或动画
        // Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject); // 简单起见直接销毁
    }
}