using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // ── CONFIGURAÇÕES DE MOVIMENTO ────────────────────────────────────────────
    [Header("Movimento")]
    public float walkSpeed    = 5f;
    public float sprintSpeed  = 9f;
    public float rotationSpeed = 15f;

    [Header("Física de Pulo")]
    public float jumpForce        = 12f;
    public float fallMultiplier   = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Sistemas de Game Feel")]
    public float coyoteTime     = 0.15f;
    public float jumpBufferTime = 0.2f;

    [Header("Detecção de Chão")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private Vector3 footOffset = new Vector3(0, -0.1f, 0);

    // ── SONS DE PASSOS ────────────────────────────────────────────────────────
    [Header("Som de Passos Andar")]
    [SerializeField] private AudioSource walkstepAudioSource;
    [SerializeField] private AudioClip[] walkstepClips;

    [Header("Som de Passos Correr")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Som de Passos Pular")]
    [SerializeField] private AudioSource jumpstepAudioSource;
    [SerializeField] private AudioClip[] jumpstepClips;

    // ── ESTADO INTERNO ────────────────────────────────────────────────────────
    private Rigidbody  rb;
    private Animator   animator;
    private Transform  cameraTransform;

    private Vector2 movementInput;
    private bool    isSprinting       = false;
    private bool    isJumpPressed     = false;
    private bool    isGrounded        = false;
    private float   coyoteTimeCounter = 0f;
    private float   jumpBufferCounter = 0f;
    private float   lastJumpTime      = 0f;

    // ── INICIALIZAÇÃO ─────────────────────────────────────────────────────────
    private void Awake()
    {
        rb       = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        FindCamera();
    }

    private void FindCamera()
    {
        // Camera.main exige tag "MainCamera" — fallback por nome
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
        {
            var camGO = GameObject.Find("Main Camera");
            if (camGO != null) cameraTransform = camGO.transform;
        }

        if (cameraTransform == null)
            Debug.LogError("[Player] Câmera não encontrada! Certifique-se de que a Main Camera tem tag 'MainCamera'.");
    }

    // ── UPDATE: leitura de input direto (Keyboard.current / Mouse) ───────────
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Bloqueia input do jogador durante diálogo
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
        {
            movementInput = Vector2.zero;
            isSprinting   = false;
            return;
        }

        // Movimento (WASD)
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        movementInput = new Vector2(x, y);

        // Sprint (Shift esquerdo)
        isSprinting = kb.leftShiftKey.isPressed;

        // Jump buffer
        if (kb.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        // Saber se o botão ainda está pressionado (para pulo variável)
        isJumpPressed = kb.spaceKey.isPressed;

        // Coyote Time
        coyoteTimeCounter = isGrounded
            ? coyoteTime
            : Mathf.Max(0f, coyoteTimeCounter - Time.deltaTime);

        // Countdown do jump buffer
        jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
    }

    // ── FIXED UPDATE: física ─────────────────────────────────────────────────
    private void FixedUpdate()
    {
        // 1. Ground Check (com delay de decolagem para evitar re-trigger imediato)
        if (Time.time > lastJumpTime + 0.1f)
            isGrounded = Physics.CheckSphere(transform.position + footOffset, groundCheckRadius, groundLayer);
        else
            isGrounded = false;

        // 2. Movimento horizontal
        HandleMovement();

        // 3. Pulo
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            ExecuteJump();

        // 4. Gravidade customizada
        ApplyCustomGravity();

        // 5. Dados para o Animator
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VelocityY",  rb.linearVelocity.y);
            animator.SetBool("Walk",  movementInput.sqrMagnitude > 0.01f);
            animator.SetBool("Run",   isSprinting && movementInput.sqrMagnitude > 0.01f);
        }
    }

    // ── MOVIMENTO E ROTAÇÃO ───────────────────────────────────────────────────
    private void HandleMovement()
    {
        // Tenta encontrar câmera se ainda não foi encontrada
        if (cameraTransform == null) { FindCamera(); return; }

        // Direção relativa à câmera (ignora pitch)
        Vector3 camFwd   = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right,   Vector3.up).normalized;

        Vector2 input    = movementInput.sqrMagnitude < 0.01f ? Vector2.zero : movementInput;
        Vector3 movement = (camFwd * input.y + camRight * input.x).normalized;

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // Velocidade horizontal via velocity — respeita colisões do Physics corretamente
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);

        if (movement != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        isGrounded        = false;
        lastJumpTime      = Time.time;

        animator?.SetTrigger("Jump");
    }

    private void ApplyCustomGravity()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier   - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !isJumpPressed)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    // ── GIZMOS ────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + footOffset, groundCheckRadius);
    }

    // ── SONS DE PASSOS (chamados por Animation Events) ────────────────────────
    private void OnStep()
    {
        if (walkstepAudioSource == null || walkstepClips.Length == 0) return;
        walkstepAudioSource.PlayOneShot(walkstepClips[Random.Range(0, walkstepClips.Length)]);
    }

    private void OnSprintStep()
    {
        if (footstepAudioSource == null || footstepClips.Length == 0) return;
        footstepAudioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
    }

    private void OnJumpStep()
    {
        if (jumpstepAudioSource == null || jumpstepClips.Length == 0) return;
        jumpstepAudioSource.PlayOneShot(jumpstepClips[Random.Range(0, jumpstepClips.Length)]);
    }
}
