using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))] // === ������ǿ����ҪAnimator��� ===
public class PlayerController : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 10f;

    [Header("���(Dash)����")]
    [SerializeField] private float dashDistance = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float tapThreshold = 0.2f;

    [Header("�������")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // �ڲ����
    private Rigidbody2D rb;
    private Animator anim; // === ����������������� ===

    // ״̬����
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    // ����뼲���߼�����
    private float shiftPressTime;
    private bool isSprinting = false;
    private bool isDashing = false;
    private float lastDashTime = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // === ��������ȡAnimator ===
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        CheckGround();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PerformJump();
        }

        HandleShiftInput();
        FlipCharacter();

        // === ������ÿ֡���¶������� ===
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        Move();
    }

    // === ���������������߼� ===
    private void UpdateAnimations()
    {
        // 1. ����ˮƽ�ٶ� (ȡ����ֵ����Ϊ�����ƶ��ٶ��Ǹ���������������ͨ��ֻ����С)
        // ���Ǹ��ݵ�ǰʵ�ʸ����ٶ������ã�����ײǽʱ����Ҳ��ͣ��
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // 2. ���õ���״̬ (������Ծ�𲽺����)
        anim.SetBool("IsGrounded", isGrounded);

        // 3. ���ô�ֱ�ٶ� (�������� ��Ծ�����׶� �� ����׶�)
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private void Move()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // ��ѡ�����ϣ������˲��Ͳ����������������ԼӸ�Trigger��
        // ��ͨ������ IsGrounded �� false ���㹻�ˡ�
    }

    private void HandleShiftInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            shiftPressTime = Time.time;
            isSprinting = false;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Time.time - shiftPressTime > tapThreshold)
            {
                isSprinting = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            float pressDuration = Time.time - shiftPressTime;
            if (pressDuration <= tapThreshold)
            {
                if (Time.time >= lastDashTime + dashCooldown)
                {
                    StartCoroutine(Dash());
                }
            }
            isSprinting = false;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // === ������������̶��� ===
        anim.SetTrigger("Dash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = horizontalInput;
        if (dashDirection == 0) dashDirection = isFacingRight ? 1 : -1;

        rb.linearVelocity = new Vector2(dashDirection * dashDistance, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    // ... CheckGround, Flip, Gizmos ������֮ǰһ�£�ʡ���Խ�ʡƪ�� ...
    // ��ȷ������֮ǰ�� CheckGround, FlipCharacter, Flip, OnDrawGizmos ����

    private void CheckGround()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FlipCharacter()
    {
        if (isDashing) return;
        if (horizontalInput > 0 && !isFacingRight) Flip();
        else if (horizontalInput < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}