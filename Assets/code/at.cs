using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("设置")]
    public int damageAmount = 20;      // 伤害数值
    public float attackRange = 2.0f;   // 攻击距离
    public LayerMask enemyLayer;       // 敌人所在的图层(Layer)

    [Header("组件引用")]
    public Animator animator;          // 动画控制器

    void Update()
    {
        // 0是左键，1是右键，2是中键
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    void PerformAttack()
    {
        // 1. 播放攻击动画 (需要在Animator里设置一个名为 "Attack" 的 Trigger)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 2. 发射射线检测前方单位
        // origin: 射线的起点 (位置稍微抬高一点，防止打到地面)
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
        // direction: 射线的方向 (角色面朝的方向)
        Vector3 rayDirection = transform.forward;

        RaycastHit hit;

        // 发射射线
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange, enemyLayer))
        {
            // 检查被打中的物体是否有生命值脚本
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);

                // 可选：在这里添加击中特效或音效
                Debug.Log("击中了 " + hit.collider.name);
            }
        }
    }

    // 在编辑器中绘制攻击范围辅助线 (仅Scene窗口可见)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
        Gizmos.DrawRay(rayOrigin, transform.forward * attackRange);
    }
}