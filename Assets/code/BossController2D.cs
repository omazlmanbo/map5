using UnityEngine;

public class BossController2D : MonoBehaviour
{
    [Header("设置")]
    public float attackRange = 8f;      // 触发攻击的距离
    public float fireRate = 2f;         // 攻击间隔（秒）
    public GameObject missilePrefab;    // 导弹预制体
    public Transform firePoint;         // 发射点位置

    [Header("引用")]
    public Transform player;            // 玩家变换组件
    public Animator animator;           // 动画组件

    private float nextFireTime = 0f;

    void Update()
    {
        if (player == null) return;

        // 计算 Boss 到玩家的距离
        float distance = Vector2.Distance(transform.position, player.position);

        // 如果在范围内且冷却结束
        if (distance <= attackRange && Time.time >= nextFireTime)
        {
            Attack();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Attack()
    {
        // 1. 播放攻击动画 (Animator中需有名为 "Attack" 的 Trigger)
        if (animator != null) animator.SetTrigger("Attack");

        // 2. 生成导弹
        GameObject missileGO = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);

        // 3. 锁定瞬间方向：计算从发射点指向玩家当前位置的单位向量
        Vector2 shootDirection = (player.position - firePoint.position).normalized;

        // 4. 将方向传递给导弹脚本
        Missile2D missileScript = missileGO.GetComponent<Missile2D>();
        if (missileScript != null)
        {
            missileScript.Launch(shootDirection);
        }
    }

    // 在编辑器中绘制攻击范围圆圈
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}