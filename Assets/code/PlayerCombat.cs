using System.Collections;
using UnityEngine;

public class PlayerCombat : CombatEntity
{
    [Header("玩家特有设置")]
    public Meleeweapon weaponScript;
    public float attackDuration = 0.2f; // 攻击判定持续时间
    public float attackCooldown = 0.5f; // 攻击冷却

    private bool isAttacking = false;

    void Start()
    {
        targetTag = "Enemy"; // 玩家的目标是 Enemy
    }

    void Update()
    {
        if (isDead) return;

        // 按下J键或鼠标左键攻击
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            if (!isAttacking)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        // 1. 这里可以播放攻击动画
        // anim.SetTrigger("Attack");

        // 2. 开启判定框 (如果是瞬发)
        weaponScript.EnableHitbox();

        // 3. 判定框持续时间
        yield return new WaitForSeconds(attackDuration);

        // 4. 关闭判定框
        weaponScript.DisableHitbox();

        // 5. 冷却时间
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}