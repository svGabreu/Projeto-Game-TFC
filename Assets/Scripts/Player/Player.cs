using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // -- CONFIGURAÇÕES DE MOVIMENTO E PULO
    //Variáveis 
    [Header("Movimento")]
    public float walkSpeed = 5f;     // A velocidade normal 
    public float sprintSpeed = 9f;   // A velocidade correndo
    public float rotationSpeed = 15f;
    private bool isSprinting = false; // Guarda se o botão está apertado

    [Header("Física de Pulo")] // Pulo Curto, Gravidade Dinâmica, etc.
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private bool isJumpPressed = false;
    private float lastJumpTime = 0f;

    [Header("Sistemas de Game Feel")] // Coyote Time e Jump Buffer
    public float coyoteTime = 0.15f; // Tempo que o jogador pode pular após sair do chão
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Detecção de Chão")] // Usando CheckSphere para detectar o chão, com um pequeno delay após pular para evitar "re-gravidade" imediata
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private Vector3 footOffset = new Vector3(0, -0.1f, 0);
    private bool isGrounded;

    // -- COMPONENTES E REFERÊNCIAS
    private Rigidbody rb;
    private Animator animator;
    private Transform cameraTransform;
    private Vector2 movementInput;

    // -- SONS DE PASSOS
    [Header("Som de Passos Andar")]
    [SerializeField] private AudioSource walkstepAudioSource;
    [SerializeField] private AudioClip[] walkstepClips;

    [Header("Som de Passos Correr")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Som de Passos Pular")]
    [SerializeField] private AudioSource jumpstepAudioSource;
    [SerializeField] private AudioClip[] jumpstepClips;

    //Método de inicialização, pega os componentes necessários e a referência para a câmera
    private void Awake() 
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // --- LEITURA DE INPUT ---

    //
    public void OnMove(InputAction.CallbackContext context) // Lê o input de movimento e armazena para usar no FixedUpdate
    {
        movementInput = context.ReadValue<Vector2>();
    }

    // Método que gerencia o Jump Buffer e o estado do botão de pulo
    public void OnJump(InputAction.CallbackContext context) 
    {
        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.started || context.performed) isJumpPressed = true;
        if (context.canceled) isJumpPressed = false;
    }

    // Método que gerencia o estado de sprint
    public void OnSprint(InputAction.CallbackContext context)
    {
        // Quando aperta o botão, é true. Quando solta, é false.
        if (context.performed) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }

    // --- GERENCIAMENTO DE TEMPO E LÓGICA RÁPIDA ---

    private void Update()
    {
        // Lógica do Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Lógica do Jump Buffer
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }


    // --- FÍSICA E MOVIMENTO ---

    private void FixedUpdate()
    {
        // 1. Ground Check com Trava de Decolagem (0.1s de delay após pular)
        if (Time.time > lastJumpTime + 0.1f)
        {
            isGrounded = Physics.CheckSphere(transform.position + footOffset, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = false;
        }

        // 2. Movimento e Rotação
        HandleMovement(); 

        // 3. Execução do Pulo
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            ExecuteJump();
        }

        ApplyCustomGravity();

        // 4. Envio de dados para o Animator
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
        animator.SetBool("Walk", movementInput.sqrMagnitude > 0.01f);
        animator.SetBool("Run", isSprinting && movementInput.sqrMagnitude > 0.01f);
    }

    //
    private void HandleMovement()  
    {
        // Garante que o movimento ignora se a câmera olha pro céu ou chão
        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector2 inputComDeadzone = movementInput.sqrMagnitude < 0.01f ? Vector2.zero : movementInput;
        Vector3 movement = (cameraForward * inputComDeadzone.y + cameraRight * inputComDeadzone.x).normalized;

        // Escolhe a velocidade baseada no botão de correr
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void ExecuteJump() 
    {
        // Zera velocidade Y para evitar pulos super altos se já estiver subindo numa rampa
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Limpa buffers e seta cooldown de decolagem
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        isGrounded = false;
        lastJumpTime = Time.time;

        animator.SetTrigger("Jump");
    }

    private void ApplyCustomGravity() // Aplica a gravidade modificada para criar o efeito de pulo mais alto e mais baixo dependendo do tempo que o botão é segurado
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumpPressed)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }


    // --- AUXILIAR VISUAL ---

    private void OnDrawGizmosSelected() // Desenha a esfera de detecção do chão para facilitar o ajuste no editor
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + footOffset, groundCheckRadius);
    }


    // --- SONS DE PASSOS ---
    private void OnStep() 
    {
         int index = Random.Range(0, walkstepClips.Length);
         walkstepAudioSource.PlayOneShot(walkstepClips[index]);
    }

    private void OnSprintStep()
    {
         int index = Random.Range(0, footstepClips.Length);
         footstepAudioSource.PlayOneShot(footstepClips[index]);
    }

    private void OnJumpStep()
    {
        int index = Random.Range(0, jumpstepClips.Length);
        jumpstepAudioSource.PlayOneShot(jumpstepClips[index]);
    }
}