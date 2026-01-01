using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class MonsterController : MonoBehaviour
{
    [Header("目标")]
    [SerializeField] private Transform playerTransform; // 玩家的位置
    [SerializeField] private string playerTag = "Player"; // 如果没拖拽，自动根据Tag找

    [Header("AI 数值设置")]
    [SerializeField] private float moveSpeed = 3f;      // 移动速度
    [SerializeField] private float chaseRadius = 6f;    // 侦测/追逐范围
    [SerializeField] private float attackRadius = 1.5f; // 触发攻击的距离 (要比追逐范围小)

    [Header("战斗设置")]
    [SerializeField] private int damageAmount = 1;      // 攻击伤害
    [SerializeField] private float attackCooldown = 1.5f; // 攻击间隔
    [SerializeField] private Transform attackPoint;     // 攻击判定的中心点 (在怪物前方)
    [SerializeField] private float attackRange = 0.5f;  // 攻击判定的半径 (武器的长度)
    [SerializeField] private LayerMask playerLayer;     // 玩家所在的图层

    // 内部变量
    private Rigidbody2D rb;
    private Animator anim;
    private float nextAttackTime = 0f;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 如果没有手动拖拽玩家，自动去场景里找
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 计算怪物和玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // --- 状态机逻辑 ---

        // 1. 如果在攻击范围内 -> 攻击
        if (distanceToPlayer <= attackRadius)
        {
            StopMoving();
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // 2. 如果在追逐范围内，但还没到攻击距离 -> 追逐
        else if (distanceToPlayer <= chaseRadius)
        {
            ChasePlayer();
        }
        // 3. 玩家太远 -> 待机
        else
        {
            StopMoving();
        }
    }

    // --- 行为方法 ---

    private void ChasePlayer()
    {
        // 播放走路动画
        anim.SetBool("IsMoving", true);

        // 向玩家移动 (MoveTowards 方式比较平滑)
        Vector2 targetPos = new Vector2(playerTransform.position.x, rb.position.y); // 只在X轴移动，如果你是俯视游戏，去掉y的限制
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);

        // 面向玩家
        LookAtPlayer();
    }

    private void StopMoving()
    {
        anim.SetBool("IsMoving", false);
        // 保持不动
    }

    private void Attack()
    {
        // 1. 播放攻击动画
        anim.SetTrigger("Attack");

        // 2. 造成伤害检测 (近战判定)
        // 我们不希望怪物一抬手玩家就掉血，所以通常检测逻辑写在动画事件里。
        // 但为了简单起见，我们这里直接检测，或者你可以用 Invoke 延迟一点点执行。
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);

        if (hitPlayer != null)
        {
            Debug.Log("攻击命中玩家！");

            // 尝试获取玩家身上的血量脚本并扣血
            PlayerHealth healthScript = hitPlayer.GetComponent<PlayerHealth>();
            if (healthScript != null)
            {
                healthScript.TakeDamage(damageAmount);
            }
        }
    }

    private void LookAtPlayer()
    {
        // 如果玩家在右边，且我面向左 -> 翻转
        if (playerTransform.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        // 如果玩家在左边，且我面向右 -> 翻转
        else if (playerTransform.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // --- 调试辅助线 (Gizmos) ---
    // 这个函数会让你在编辑器里看到红圈和黄圈，方便调节范围
    private void OnDrawGizmosSelected()
    {
        // 画出追逐范围 (黄色)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // 画出触发攻击的范围 (红色)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // 画出攻击判定的具体打击点 (实心小红球)
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(attackPoint.position, attackRange);
        }
    }
}