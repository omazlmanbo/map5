using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("���")]
    public Animator animator;

    [Header("��ǽ���ǽ�� (2D)")]
    public float climbSpeed = 5f;
    public LayerMask wallLayer;       // ����Ϊ Wall ͼ��
    public float detectionDistance = 0.5f; // �����룬2Dͨ���Ƚ϶�
    public float wallJumpForce = 10f; // ��ǽ������X
    public float wallJumpUpForce = 10f; // ��ǽ������Y

    [Header("���� (2D)")]
    public float glideFallSpeed = 1f; // ���������ٶ�
    public float glideMoveSpeed = 5f; // ����ˮƽ�ٶ�
    public float timeToActivateGlide = 1.2f;

    private Rigidbody2D rb; // ���� Rigidbody2D
    private bool isClimbing = false;
    private bool isGliding = false;
    private float spaceHoldTimer = 0f;
    private float wallJumpCooldown = 0f;

    // ������¼��ҳ��� (1����, -1����)
    private float facingDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ��ȡ 2D ����
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ���³����¼ (������Ұ���)
        float hInput = Input.GetAxisRaw("Horizontal");
        if (hInput != 0) facingDirection = hInput;

        if (wallJumpCooldown > 0) wallJumpCooldown -= Time.deltaTime;

        HandleWallInteraction();
        HandleGlidingInput();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            PerformClimb();
        }
        else if (isGliding)
        {
            PerformGlide();
        }
        // ע�⣺2D ��ͨ������Ҫ�ֶ� rb.useGravity = true��
        // ֻҪ���ǲ��޸� velocity.y ���߰� gravityScale ��Ϊ 1 ����
        else if (!isClimbing && !isGliding)
        {
            rb.gravityScale = 1; // �ָ�����
        }
    }

    void HandleWallInteraction()
    {
        if (wallJumpCooldown > 0) { isClimbing = false; return; }

        // 2D ���߼�⣺�ӽ�����΢����һ���λ�÷���
        // �ؼ�������Ҫ���� facingDirection (�����ǳ���)
        Vector2 rayStart = transform.position;
        Vector2 rayDir = Vector2.right * facingDirection;

        // ʹ�� Physics2D
        RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDir, detectionDistance, wallLayer);

        bool wallInFront = hit.collider != null;

        // ��ǽ��
        if (wallInFront && Input.GetButtonDown("Jump"))
        {
            PerformWallJump();
            return;
        }

        // ��ǽ
        float vInput = Input.GetAxis("Vertical");
        // ֻ�а�ס�ϻ����£�����ǰ��ǽʱ������ǽ
        if (wallInFront && Mathf.Abs(vInput) > 0.1f)
        {
            isClimbing = true;
        }
        else
        {
            isClimbing = false;
        }
    }

    void PerformWallJump()
    {
        isClimbing = false;
        isGliding = false;
        spaceHoldTimer = 0f;

        // ������������ķ�����
        Vector2 jumpDir = new Vector2(-facingDirection * wallJumpForce, wallJumpUpForce);

        rb.linearVelocity = Vector2.zero; // �����ٶ�
        rb.AddForce(jumpDir, ForceMode2D.Impulse); // ʹ�� ForceMode2D

        wallJumpCooldown = 0.5f;
        if (animator) animator.SetTrigger("WallJump");
    }

    void PerformClimb()
    {
        rb.gravityScale = 0; // 2D �ر�����ͨ������Ϊ 0
        float vInput = Input.GetAxis("Vertical");
        // 2D �ƶ�ֻ�ı� X �� Y
        rb.linearVelocity = new Vector2(0, vInput * climbSpeed);
    }

    void HandleGlidingInput()
    {
        // �������Ƿ��е� (2D)
        bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));
        bool isAirborne = !isGrounded;

        if (Input.GetKey(KeyCode.Space) && isAirborne && !isClimbing && wallJumpCooldown <= 0)
        {
            spaceHoldTimer += Time.deltaTime;
            if (spaceHoldTimer >= timeToActivateGlide)
            {
                isGliding = true;
            }
        }
        else
        {
            spaceHoldTimer = 0f;
            isGliding = false;
        }
    }

    void PerformGlide()
    {
        rb.gravityScale = 0; // ��ʱ�ر�����Ӱ�죬��ȫ�ֶ������ٶ�

        // ����ˮƽ�����ٶȣ����� Y �������ٶ�
        float hInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(hInput * glideMoveSpeed, -glideFallSpeed);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetBool("IsClimbing", isClimbing);
        animator.SetBool("IsGliding", isGliding);

        // �򵥵ķ�ת��ɫͼƬ (�����û�������ű����Ʒ�ת)
        if (facingDirection != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = facingDirection > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    // ���Ի���
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // �������ߣ�ע������ֻ��ʾ�⣬��һ����ȫ���� facingDirection �仯
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * detectionDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * detectionDistance);
    }
}