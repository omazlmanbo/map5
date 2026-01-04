using UnityEngine;

public class Meleeweapon : MonoBehaviour
{
    private CombatEntity owner;
    private BoxCollider2D col;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.enabled = false; // 默认关闭判定框
        owner = GetComponentInParent<CombatEntity>();
    }

    // 开启攻击判定的方法
    public void EnableHitbox()
    {
        col.enabled = true;
    }

    // 关闭攻击判定的方法
    public void DisableHitbox()
    {
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果主人已经死了，这个攻击无效 (防止同归于尽时的延迟判定)
        if (owner.isDead) return;

        // 检查碰撞到的对象是否是目标阵营
        if (other.CompareTag(owner.targetTag))
        {
            CombatEntity victim = other.GetComponent<CombatEntity>();
            if (victim != null && !victim.isDead)
            {
                victim.Die(); // 一击必杀
            }
        }
    }

    // 可视化调试：在Scene窗口画出攻击范围
    void OnDrawGizmos()
    {
        if (GetComponent<BoxCollider2D>() != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
        }
    }
}