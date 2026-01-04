using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterAI2D : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3.5f;        // 移动速度
    [SerializeField] private float detectionRange = 10f;    // 检测范围
    
    [Header("攻击设置")]
    [SerializeField] private int damageToPlayer = 10;       // 对玩家造成的伤害
    [SerializeField] private float attackCooldown = 1.0f;   // 攻击冷却时间
    [SerializeField] private float attackRange = 1.5f;     // 攻击范围（触碰距离）
    
    [Header("可选设置")]
    [SerializeField] private bool flipSprite = true;       // 是否翻转精灵朝向玩家
    
    private Transform playerTarget;
    private Rigidbody2D rb;
    private float lastAttackTime = 0f;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    
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
            Debug.LogWarning("MonsterAI2D: 未找到标签为 'Player' 的游戏对象！");
        }
    }
    
    void Update()
    {
        // 如果怪物已死或没找到玩家，不执行AI逻辑
        if (isDead || playerTarget == null) return;
        
        // 计算到玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        
        // 在检测范围内，追逐玩家
        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
            
            // 在攻击范围内，尝试攻击
            if (distanceToPlayer <= attackRange)
            {
                TryAttack();
            }
        }
    }
    
    void FixedUpdate()
    {
        // 在FixedUpdate中处理物理移动
        if (isDead || playerTarget == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        
        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            // 计算移动方向
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            
            // 移动怪物（保持Y轴速度不变，只改变X轴）
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            
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
    }
    
    // 追逐玩家（在Update中调用，用于逻辑判断）
    private void ChasePlayer()
    {
        // 主要逻辑在FixedUpdate中处理移动
        // 这里可以添加其他追逐相关的逻辑
    }
    
    // 尝试攻击玩家
    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 检查是否真的在攻击范围内
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            if (distance <= attackRange)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    }
    
    // 攻击玩家
    private void AttackPlayer()
    {
        PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToPlayer);
            Debug.Log($"怪物 {gameObject.name} 攻击了玩家，造成 {damageToPlayer} 点伤害");
        }
    }
    
    // 碰撞检测：当怪物触碰到玩家时造成伤害
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        
        // 如果碰撞的是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 检查攻击冷却
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                    Debug.Log($"怪物 {gameObject.name} 触碰攻击了玩家，造成 {damageToPlayer} 点伤害");
                    lastAttackTime = Time.time;
                }
            }
        }
    }
    
    // 持续碰撞时也可以造成伤害（可选）
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                    Debug.Log($"怪物 {gameObject.name} 持续触碰攻击了玩家，造成 {damageToPlayer} 点伤害");
                    lastAttackTime = Time.time;
                }
            }
        }
    }
    
    // 设置怪物为死亡状态（可被其他脚本调用）
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
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

