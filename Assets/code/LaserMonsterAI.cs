using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserMonsterAI : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRange = 10f;    // 检测玩家的范围
    
    [Header("激光攻击设置")]
    [SerializeField] private Transform laserOrigin;         // 激光发射点（如果为空，使用怪物位置）
    [SerializeField] private float laserRange = 15f;      // 激光射程
    [SerializeField] private float laserWidth = 0.7f;      // 激光宽度
    [SerializeField] private float attackCooldown = 3f;    // 攻击冷却时间
    [SerializeField] private float laserDuration = 0.2f;   // 激光持续时间（固定1秒，不可修改）
    [SerializeField] private int damageOnHit = 30;     // 击中玩家造成的伤害
    
    [Header("视觉效果")]
    [SerializeField] private LineRenderer laserLineRenderer; // 激光线条渲染器（如果为空会自动创建）
    [SerializeField] private Color laserColor = Color.red;  // 激光颜色
    [SerializeField] private Material laserMaterial;        // 激光材质（可选）
    
    [Header("警告效果（虚影提示）")]
    [SerializeField] private bool showWarning = true;       // 是否显示警告效果
    [SerializeField] private float warningDuration = 0.5f;  // 警告持续时间（固定1秒）
    [SerializeField] private Color warningColor = new Color(1f, 1f, 0f, 0.3f); // 警告颜色（半透明黄色）
    [SerializeField] private float warningWidth = 0.15f;    // 警告线宽度
    
    [Header("可选设置")]
    [SerializeField] private bool flipSprite = true;       // 是否翻转精灵朝向玩家
    [SerializeField] private LayerMask obstacleLayer;       // 障碍物图层（激光会被阻挡）
    
    private Transform playerTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private bool isFiringLaser = false;
    private float lastAttackTime = 0f;
    private float laserStartTime = 0f;
    private Coroutine laserCoroutine;
    
    // 激光锁定方向（警告和发射时都使用，不再改变）
    private Vector2 lockedLaserDirection;
    private Vector2 lockedLaserOrigin;
    private bool hasHitPlayer = false;  // 是否已经击中玩家（只造成一次伤害）
    
    // 警告线渲染器（虚影提示）
    private LineRenderer warningLineRenderer;
    
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
            Debug.LogWarning("LaserMonsterAI: 未找到标签为 'Player' 的游戏对象！");
        }
        
        // 如果没有指定激光发射点，使用怪物位置
        if (laserOrigin == null)
        {
            laserOrigin = transform;
        }
        
        // 如果没有指定LineRenderer，创建一个
        if (laserLineRenderer == null)
        {
            CreateLaserRenderer();
        }
        
        // 创建警告线渲染器（虚影提示）
        if (showWarning)
        {
            CreateWarningRenderer();
        }
        
        // 初始化激光为不可见
        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
        
        if (warningLineRenderer != null)
        {
            warningLineRenderer.enabled = false;
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
            
            // 尝试发射激光
            if (!isFiringLaser && Time.time >= lastAttackTime + attackCooldown)
            {
                StartLaserAttack();
            }
        }
        
        // 如果正在发射激光，检查伤害和持续时间（方向已锁定，不再更新）
        if (isFiringLaser)
        {
            CheckLaserDamage();
            
            // 确保激光在1秒后消失（双重保险）
            if (Time.time >= laserStartTime + laserDuration)
            {
                StopLaser();
            }
        }
    }
    
    // 创建激光渲染器
    void CreateLaserRenderer()
    {
        GameObject laserObj = new GameObject("LaserRenderer");
        laserObj.transform.SetParent(transform);
        laserObj.transform.localPosition = Vector3.zero;
        
        laserLineRenderer = laserObj.AddComponent<LineRenderer>();
        laserLineRenderer.startWidth = laserWidth;
        laserLineRenderer.endWidth = laserWidth;
        laserLineRenderer.positionCount = 2;
        laserLineRenderer.useWorldSpace = true;
        laserLineRenderer.sortingOrder = 10; // 确保激光显示在其他对象之上
        
        // 设置材质
        if (laserMaterial != null)
        {
            laserLineRenderer.material = laserMaterial;
        }
        else
        {
            // 使用默认材质
            laserLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        laserLineRenderer.startColor = laserColor;
        laserLineRenderer.endColor = laserColor;
        laserLineRenderer.enabled = false;
    }
    
    // 创建警告线渲染器（虚影提示）
    void CreateWarningRenderer()
    {
        GameObject warningObj = new GameObject("WarningRenderer");
        warningObj.transform.SetParent(transform);
        warningObj.transform.localPosition = Vector3.zero;
        
        warningLineRenderer = warningObj.AddComponent<LineRenderer>();
        warningLineRenderer.startWidth = warningWidth;
        warningLineRenderer.endWidth = warningWidth;
        warningLineRenderer.positionCount = 2;
        warningLineRenderer.useWorldSpace = true;
        warningLineRenderer.sortingOrder = 9; // 在激光下方
        
        // 设置材质
        if (laserMaterial != null)
        {
            warningLineRenderer.material = laserMaterial;
        }
        else
        {
            warningLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        warningLineRenderer.startColor = warningColor;
        warningLineRenderer.endColor = warningColor;
        warningLineRenderer.enabled = false;
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
    
    // 开始激光攻击
    void StartLaserAttack()
    {
        if (laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
        }
        
        laserCoroutine = StartCoroutine(LaserAttackCoroutine());
    }
    
    // 激光攻击协程
    IEnumerator LaserAttackCoroutine()
    {
        // 首先锁定方向（警告和发射都使用这个固定方向）
        if (playerTarget != null)
        {
            lockedLaserOrigin = laserOrigin.position;
            lockedLaserDirection = (playerTarget.position - (Vector3)lockedLaserOrigin).normalized;
        }
        else
        {
            lockedLaserOrigin = laserOrigin.position;
            lockedLaserDirection = Vector2.right;
        }
        
        // 警告阶段：显示虚影提示，方向固定，从出现后不移动
        if (showWarning && warningLineRenderer != null)
        {
            // 设置一次预警位置（固定，之后不再移动）
            UpdateWarningVisualFixed();
            warningLineRenderer.enabled = true;
            
            // 等待预警时间（位置已固定，不再更新）
            yield return new WaitForSeconds(warningDuration);
            
            warningLineRenderer.enabled = false;
        }
        
        // 发射激光
        isFiringLaser = true;
        laserStartTime = Time.time;
        hasHitPlayer = false;  // 重置击中标志
        
        // 显示激光并锁定方向
        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = true;
            UpdateLockedLaserVisual();
        }
        
        // 持续发射激光1秒
        yield return new WaitForSeconds(laserDuration);
        
        // 停止激光
        StopLaser();
    }
    
    // 更新警告视觉（虚影提示，使用固定方向）
    void UpdateWarningVisualFixed()
    {
        if (warningLineRenderer == null) return;
        
        Vector2 origin = lockedLaserOrigin;
        Vector2 direction = lockedLaserDirection;
        
        // 使用Raycast检测障碍物
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserRange, obstacleLayer);
        
        Vector2 endPoint;
        if (hit.collider != null)
        {
            endPoint = hit.point;
        }
        else
        {
            endPoint = origin + direction * laserRange;
        }
        
        // 更新警告线位置
        warningLineRenderer.SetPosition(0, origin);
        warningLineRenderer.SetPosition(1, endPoint);
    }
    
    // 更新锁定激光视觉（发射后方向不再改变）
    void UpdateLockedLaserVisual()
    {
        if (laserLineRenderer == null) return;
        
        // 使用Raycast检测障碍物
        RaycastHit2D hit = Physics2D.Raycast(lockedLaserOrigin, lockedLaserDirection, laserRange, obstacleLayer);
        
        Vector2 endPoint;
        if (hit.collider != null)
        {
            // 如果击中障碍物，激光在障碍物处停止
            endPoint = hit.point;
        }
        else
        {
            // 如果没有障碍物，激光延伸到最大射程
            endPoint = lockedLaserOrigin + lockedLaserDirection * laserRange;
        }
        
        // 更新LineRenderer的位置
        laserLineRenderer.SetPosition(0, lockedLaserOrigin);
        laserLineRenderer.SetPosition(1, endPoint);
    }
    
    // 检查激光伤害（只检查一次，击中后不再造成伤害）
    void CheckLaserDamage()
    {
        if (playerTarget == null || hasHitPlayer) return;
        
        // 使用锁定的激光方向检测是否击中玩家
        // 检测玩家图层和障碍物图层
        int playerLayer = 1 << playerTarget.gameObject.layer;
        LayerMask hitLayer = obstacleLayer | playerLayer;
        
        RaycastHit2D hit = Physics2D.Raycast(lockedLaserOrigin, lockedLaserDirection, laserRange, hitLayer);
        
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // 对玩家造成伤害（只造成一次）
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnHit);
                Debug.Log($"激光击中玩家，造成 {damageOnHit} 点伤害");
                hasHitPlayer = true;  // 标记已击中，不再造成伤害
            }
        }
    }
    
    // 停止激光
    void StopLaser()
    {
        isFiringLaser = false;
        lastAttackTime = Time.time;
        hasHitPlayer = false;  // 重置击中标志
        
        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
        
        if (warningLineRenderer != null)
        {
            warningLineRenderer.enabled = false;
        }
    }
    
    // 设置怪物为死亡状态
    public void SetDead(bool dead)
    {
        isDead = dead;
        if (isDead)
        {
            StopLaser();
            if (laserCoroutine != null)
            {
                StopCoroutine(laserCoroutine);
            }
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // 在Scene视图中绘制检测范围和激光范围（仅编辑器可见）
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制激光范围
        if (laserOrigin != null)
        {
            Gizmos.color = Color.red;
            Vector2 direction = playerTarget != null 
                ? (playerTarget.position - laserOrigin.position).normalized 
                : Vector2.right;
            Gizmos.DrawLine(laserOrigin.position, (Vector2)laserOrigin.position + direction * laserRange);
        }
    }
}

