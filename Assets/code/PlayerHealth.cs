using UnityEngine;
using UnityEngine.UI; // �������ã����ڿ�����Ļ���
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("��������")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("������� (��Ҫ�ֶ���ק)")]
    public Animator animator;              // ��ҵĶ���������
    public MonoBehaviour movementScript;   // ��ҵ��ƶ��ű� (���� PlayerController)
    public Image blackScreenPanel;         // UI�ϵĺ�ɫͼƬ (�����ڵ���Ļ)

    [Header("��������")]
    public string deathAnimTrigger = "Die"; // ��������� Trigger ����
    public float fadeSpeed = 0.5f;          // ��Ļ��ڵ��ٶ�

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // ��Ϸ��ʼʱ��ȷ���������������ػ�͸����
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(false);
            Color c = blackScreenPanel.color;
            c.a = 0; // ͸������Ϊ0
            blackScreenPanel.color = c;
        }
    }

    // --- ���ⲿ (����) ���õ����˺��� ---
    public void TakeDamage(int damage)
    {
        if (isDead) return; // ���˾Ͳ������˺�

        currentHealth -= damage;
        Debug.Log($"����ܵ��˺�: -{damage}, ��ǰѪ��: {currentHealth}");

        // ������Լ�һ�� ���˶��� (Hurt)
        // if (animator != null) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // 1. ������������
        if (animator != null)
        {
            animator.SetTrigger(deathAnimTrigger);
        }

        
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // ��ѡ��������ײ�壬��ֹʬ�嵲·
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. ��Ļ���
        if (blackScreenPanel != null)
        {
            blackScreenPanel.gameObject.SetActive(true);
            StartCoroutine(FadeToBlack());
        }
    }

    // Э�̣�����͸���Ƚ���
    IEnumerator FadeToBlack()
    {
        Color color = blackScreenPanel.color;
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * fadeSpeed;
            blackScreenPanel.color = color;
            yield return null; 
        }
    }
}