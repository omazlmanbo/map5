using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InstantKillZone : MonoBehaviour
{
    [Header("碰撞设置")]
    [SerializeField] private bool useTrigger = true;  // 是否使用触发器（推荐）
    [SerializeField] private string playerTag = "Player";  // 玩家标签
    
    [Header("可选设置")]
    [SerializeField] private bool destroyOnContact = false;  // 碰撞后是否销毁此物体
    
    private void Start()
    {
        // 确保碰撞器设置正确
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            if (useTrigger && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"InstantKillZone: 已将 {gameObject.name} 的碰撞器设置为触发器");
            }
            else if (!useTrigger && col.isTrigger)
            {
                col.isTrigger = false;
                Debug.LogWarning($"InstantKillZone: 已将 {gameObject.name} 的碰撞器设置为非触发器");
            }
        }
        else
        {
            Debug.LogError($"InstantKillZone: {gameObject.name} 缺少 Collider2D 组件！");
        }
    }
    
    // 触发器碰撞检测（推荐使用）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        
        if (other.CompareTag(playerTag))
        {
            KillPlayer(other.gameObject);
        }
    }
    
    // 普通碰撞检测
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        
        if (collision.gameObject.CompareTag(playerTag))
        {
            KillPlayer(collision.gameObject);
        }
    }
    
    // 杀死玩家
    private void KillPlayer(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            // 直接将血量设为0
            playerHealth.SetHealth(0);
            Debug.Log($"InstantKillZone: 玩家 {player.name} 碰到 {gameObject.name}，血量已归零！");
        }
        else
        {
            Debug.LogWarning($"InstantKillZone: 玩家 {player.name} 没有 PlayerHealth 组件！");
        }
        
        // 如果设置了碰撞后销毁，则销毁此物体
        if (destroyOnContact)
        {
            Destroy(gameObject);
        }
    }
}