using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBehavior : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRange = 15f;    // 检测玩家的范围
    
    [Header("攻击设置")]
    [SerializeField] private GameObject missilePrefab;     // 导弹预制体
    [SerializeField] private Transform firePoint;          // 发射点（如果为空，使用Boss位置）
    [SerializeField] private float attackCooldown = 3f;    // 攻击冷却时间
    [SerializeField] private float missileSpeed = 8f;      // 导弹速度
    [SerializeField] private int missileDamage = 25;      // 导弹造成的伤害
    
    [Header("攻击动作设置")]
    [SerializeField] private Animator animator;           // 动画控制器（可选）
    [SerializeField] private string attackTrigger = "Attack"; // 攻击动画触发器名称
    [SerializeField] private float aimDuration = 1f;       // 瞄准持续时间（播放攻击动作的时间）
    
    [Header("可选设置")]
    [SerializeField] private bool flipSprite = true;       // 是否翻转精灵朝向玩家
    [SerializeField] private LayerMask obstacleLayer;      // 障碍物图层（导弹会被阻挡）
    
    private Transform playerTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private Vector2 lockedMissileDirection;  // 锁定后的导弹方向（弹道不变）
    private Coroutine attackCoroutine;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 自动寻找标签为 "Player" 的游戏对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BossBehavior: 未找到标签为 'Player' 的游戏对象！");
        }
        
        // 如果没有指定发射点，使用Boss位置
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // 如果没有指定Animator，尝试自动获取
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    void Update()
    {
        if (isDead || playerTarget == null) return;
        
        // 计算到玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        
        // 在检测范围内
        if (distanceToPlayer <= detectionRange)
        {
            // 朝向玩家
            LookAtPlayer();
            
            // 尝试攻击（有冷却时间）
            if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                StartAttack();
            }
        }
    }
    
    // 朝向玩家
    void LookAtPlayer()
    {
        if (playerTarget == null) return;
        
        Vector2 direction = (playerTarget.position - transform.position).normalized;
        
        // 翻转精灵朝向玩家
        if (flipSprite && spriteRenderer != null)
        {
            if (direction.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    
    // 开始攻击
    void StartAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        
        attackCoroutine = StartCoroutine(AttackSequence());
    }
    
    // 攻击序列协程
    IEnumerator AttackSequence()
    {
        isAttacking = true;
        
        // 1. 瞄准玩家并锁定方向
        if (playerTarget != null)
        {
            Vector2 aimDirection = (playerTarget.position - (Vector3)firePoint.position).normalized;
            lockedMissileDirection = aimDirection;
        }
        else
        {
            lockedMissileDirection = Vector2.right;
        }
        
        // 2. 播放攻击动作
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }
        
        // 3. 等待瞄准时间（播放攻击动作的时间）
        yield return new WaitForSeconds(aimDuration);
        
        // 4. 发射导弹（使用锁定的方向，弹道不变）
        FireMissile();
        
        // 5. 重置攻击状态和冷却时间
        isAttacking = false;
        lastAttackTime = Time.time;
    }
    
    // 发射导弹
    void FireMissile()
    {
        if (missilePrefab == null) return;
        
        // 实例化导弹
        GameObject missile = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
        
        // 设置导弹的方向和速度（使用锁定的方向，弹道不变）
        ProjectileParticle missileScript = missile.GetComponent<ProjectileParticle>();
        if (missileScript != null)
        {
            missileScript.Initialize(lockedMissileDirection, missileSpeed, missileDamage);
        }
        else
        {
            // 如果没有ProjectileParticle脚本，使用Rigidbody2D
            Rigidbody2D missileRb = missile.GetComponent<Rigidbody2D>();
            if (missileRb != null)
            {
                missileRb.linearVelocity = lockedMissileDirection * missileSpeed;
            }
            
            // 旋转导弹朝向移动方向
            float angle = Mathf.Atan2(lockedMissileDirection.y, lockedMissileDirection.x) * Mathf.Rad2Deg;
            missile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        Debug.Log($"Boss {gameObject.name} 发射了导弹，方向: {lockedMissileDirection}");
    }
    
    // 设置Boss为死亡状态
    public void SetDead(bool dead)
    {
        isDead = dead;
        if (isDead)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            isAttacking = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // 在Scene视图中绘制检测范围（仅编辑器可见）
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制发射点
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.3f);
        }
    }
}


