using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileParticle : MonoBehaviour
{
    [Header("粒子设置")]
    [SerializeField] private float lifetime = 5f;          // 粒子存活时间
    [SerializeField] private bool destroyOnHit = true;     // 碰撞后是否销毁
    [SerializeField] private LayerMask hitLayers = -1;     // 可以碰撞的图层
    
    private Vector2 direction;
    private float speed;
    private int damage;
    private bool isInitialized = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果没有SpriteRenderer，创建一个简单的红色圆形
        if (spriteRenderer == null)
        {
            CreateDefaultVisual();
        }
        else
        {
            // 设置为红色
            spriteRenderer.color = Color.red;
        }
        
        // 设置碰撞器为触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // 如果没有初始化，自动销毁
        if (!isInitialized)
        {
            Destroy(gameObject, lifetime);
        }
    }
    
    void Update()
    {
        // 如果已初始化，更新移动
        if (isInitialized && rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    // 初始化粒子
    public void Initialize(Vector2 dir, float spd, int dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        isInitialized = true;
        
        // 设置初始速度
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        
        // 旋转粒子朝向移动方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 设置生命周期
        Destroy(gameObject, lifetime);
    }
    
    // 碰撞检测
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查是否碰撞到玩家
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"粒子击中玩家，造成 {damage} 点伤害");
            }
            
            // 碰撞后销毁
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        // 检查是否碰撞到地面或其他障碍物
        else if (hitLayers != 0 && ((1 << collision.gameObject.layer) & hitLayers) != 0)
        {
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
    
    // 创建默认视觉效果（红色圆形）
    void CreateDefaultVisual()
    {
        // 创建SpriteRenderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        // 创建一个简单的红色圆形精灵
        Texture2D texture = new Texture2D(16, 16);
        Color[] colors = new Color[16 * 16];
        
        Vector2 center = new Vector2(8, 8);
        float radius = 6f;
        
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    colors[y * 16 + x] = Color.red;
                }
                else
                {
                    colors[y * 16 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.red;
    }
    
    // 设置粒子颜色
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}

