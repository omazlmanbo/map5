using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedMonsterAI : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRange = 10f;    // 检测玩家的范围
    
    [Header("原地移动设置")]
    [SerializeField] private float idleMoveRange = 2f;      // 原地移动的范围
    [SerializeField] private float idleMoveSpeed = 1.5f;    // 原地移动速度
    
    [Header("攻击设置")]
    [SerializeField] private GameObject projectilePrefab;    // 粒子子弹预制体
    [SerializeField] private Transform firePoint;            // 发射点（如果为空，使用怪物位置）
    [SerializeField] private float fireRate = 2f;           // 发射频率（每秒发射次数）
    [SerializeField] private float projectileSpeed = 5f;     // 粒子速度
    [SerializeField] private int damagePerProjectile = 10;  // 每个粒子造成的伤害
    
    [Header("可选设置")]
    [SerializeField] private bool flipSprite = true;       // 是否翻转精灵朝向玩家
    
    private Transform playerTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;      // 初始位置（用于原地移动）
    private Vector3 targetPosition;    // 当前移动目标位置
    private float lastFireTime = 0f;
    private bool isDead = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 记录初始位置
        startPosition = transform.position;
        targetPosition = startPosition;
        
        // 自动寻找标签为 "Player" 的游戏对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("RangedMonsterAI: 未找到标签为 'Player' 的游戏对象！");
        }
        
        // 如果没有指定发射点，使用怪物位置
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // 如果没有指定粒子预制体，创建一个简单的
        if (projectilePrefab == null)
        {
            CreateDefaultProjectilePrefab();
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
            
            // 尝试发射粒子
            TryFireProjectile();
        }
        
        // 执行原地移动
        IdleMovement();
    }
    
    void FixedUpdate()
    {
        // 在FixedUpdate中处理物理移动
        if (isDead) return;
        
        // 移动到目标位置
        Vector2 direction = (targetPosition - transform.position);
        if (direction.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector2(direction.normalized.x * idleMoveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    
    // 原地移动逻辑
    void IdleMovement()
    {
        // 如果到达目标位置，选择新的目标位置
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 在初始位置周围随机选择一个新目标
            Vector2 randomOffset = Random.insideCircle * idleMoveRange;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
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
    
    // 尝试发射粒子
    void TryFireProjectile()
    {
        if (Time.time >= lastFireTime + (1f / fireRate))
        {
            FireProjectile();
            lastFireTime = Time.time;
        }
    }
    
    // 发射粒子
    void FireProjectile()
    {
        if (projectilePrefab == null || playerTarget == null) return;
        
        // 计算朝向玩家的方向
        Vector2 direction = (playerTarget.position - firePoint.position).normalized;
        
        // 实例化粒子
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // 设置粒子的方向和速度
        ProjectileParticle projectileScript = projectile.GetComponent<ProjectileParticle>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, projectileSpeed, damagePerProjectile);
        }
        else
        {
            // 如果没有ProjectileParticle脚本，使用Rigidbody2D
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
        }
        
        Debug.Log($"怪物 {gameObject.name} 发射了粒子朝向玩家");
    }
    
    // 创建默认粒子预制体（如果没有指定）
    void CreateDefaultProjectilePrefab()
    {
        Debug.LogWarning("RangedMonsterAI: 未指定粒子预制体，请在Inspector中指定！");
    }
    
    // 设置怪物为死亡状态
    public void SetDead(bool dead)
    {
        isDead = dead;
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // 在Scene视图中绘制检测范围（仅编辑器可见）
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制原地移动范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, idleMoveRange);
    }
}

