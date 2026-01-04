using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    [Header("重生设置")]
    [SerializeField] private Transform spawnPoint;          // 重生点（如果为空，则使用初始位置）
    [SerializeField] private float respawnDelay = 1.0f;     // 重生延迟时间
    
    [Header("无敌时间设置")]
    [SerializeField] private float invincibilityDuration = 2.0f;  // 受伤后的无敌时间
    [SerializeField] private bool enableInvincibility = true;     // 是否启用无敌时间
    
    [Header("视觉效果（可选）")]
    [SerializeField] private SpriteRenderer spriteRenderer; // 用于闪烁效果
    
    private Vector3 initialPosition;  // 游戏开始时的位置
    private bool isDead = false;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    
    // 事件：可以用于UI更新等
    public System.Action<int, int> OnHealthChanged;  // 当前血量, 最大血量
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerRespawn;
    
    void Start()
    {
        // 初始化血量
        currentHealth = maxHealth;
        
        // 记录初始位置作为重生点（如果没有指定spawnPoint）
        if (spawnPoint == null)
        {
            initialPosition = transform.position;
        }
        else
        {
            initialPosition = spawnPoint.position;
        }
        
        // 如果没有指定spriteRenderer，尝试自动获取
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 通知血量变化
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        // 处理无敌时间
        if (isInvincible && enableInvincibility)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // 闪烁效果
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                
                // 恢复透明度
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }
    
    // 受到伤害
    public void TakeDamage(int amount)
    {
        // 如果已死或处于无敌状态，不受到伤害
        if (isDead || (isInvincible && enableInvincibility))
        {
            return;
        }
        
        // 扣血
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);  // 确保血量不为负
        
        Debug.Log($"玩家受到 {amount} 点伤害，当前血量: {currentHealth}/{maxHealth}");
        
        // 通知血量变化
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 如果血量归零，触发死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 受伤后进入无敌状态
            if (enableInvincibility)
            {
                StartInvincibility();
            }
        }
    }
    
    // 恢复血量
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);  // 确保不超过最大血量
        
        Debug.Log($"玩家恢复 {amount} 点血量，当前血量: {currentHealth}/{maxHealth}");
        
        // 通知血量变化
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 设置血量
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 死亡
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("玩家死亡！");
        
        // 通知死亡事件
        OnPlayerDeath?.Invoke();
        
        // 停止玩家移动（可选）
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // 延迟后重生
        StartCoroutine(Respawn());
    }
    
    // 重生协程
    private IEnumerator Respawn()
    {
        // 等待重生延迟
        yield return new WaitForSeconds(respawnDelay);
        
        // 重置血量
        currentHealth = maxHealth;
        isDead = false;
        
        // 回到起始位置
        Vector3 respawnPosition = spawnPoint != null ? spawnPoint.position : initialPosition;
        transform.position = respawnPosition;
        
        // 重置速度
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"玩家重生在位置: {respawnPosition}");
        
        // 通知重生事件
        OnPlayerRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 重生后短暂无敌
        if (enableInvincibility)
        {
            StartInvincibility();
        }
    }
    
    // 开始无敌时间
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }
    
    // 获取当前血量
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // 获取最大血量
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    // 获取血量百分比
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    // 检查是否死亡
    public bool IsDead()
    {
        return isDead;
    }
    
    // 检查是否无敌
    public bool IsInvincible()
    {
        return isInvincible;
    }
    
    // 设置重生点
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        if (spawnPoint != null)
        {
            initialPosition = spawnPoint.position;
        }
    }
}

