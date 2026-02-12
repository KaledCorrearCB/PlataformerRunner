using UnityEngine;

/// <summary>
/// Controlador principal para el personaje del Endless Runner.
/// Maneja movimiento automático, sistema de carriles y giros de 90 grados.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Movement Settings")]
    [Tooltip("Velocidad de avance automático del personaje")]
    [SerializeField] private float forwardSpeed = 7f;

    [Tooltip("Velocidad de transición entre carriles")]
    [SerializeField] private float laneChangeSpeed = 10f;

    [Tooltip("Velocidad de rotación al girar 90 grados")]
    [SerializeField] private float turnSpeed = 8f;

    [Header("Lane Settings")]
    [Tooltip("Distancia entre carriles")]
    [SerializeField] private float laneDistance = 3f;

    [Header("Gravity Settings")]
    [Tooltip("Fuerza de gravedad aplicada al personaje")]
    [SerializeField] private float gravity = -20f;

    #endregion

    #region Private Fields

    // Componentes
    private CharacterController characterController;

    // Sistema de Carriles
    private enum Lane { Left = -1, Center = 0, Right = 1 }
    private Lane currentLane = Lane.Center;
    private Lane targetLane = Lane.Center;
    private float targetLateralPosition = 0f;
    private float currentLateralPosition = 0f;

    // Sistema de Dirección
    private Vector3 forwardDirection = Vector3.forward;
    private Vector3 rightDirection = Vector3.right;
    private Quaternion targetRotation;
    private bool isRotating = false;

    // Control de Giros
    private bool isInTurnTrigger = false;
    private bool canTurn = true;

    // Velocidad vertical
    private float verticalVelocity = 0f;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        HandleInput();
        UpdateLanePosition();
        UpdateRotation();
        ApplyMovement();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Procesa la entrada del jugador para cambios de carril y giros.
    /// </summary>
    private void HandleInput()
    {
        // Detectar input de cambio de carril o giro
        bool pressedLeft = Input.GetKeyDown(KeyCode.A);
        bool pressedRight = Input.GetKeyDown(KeyCode.D);

        if (pressedLeft)
        {
            if (isInTurnTrigger && canTurn)
            {
                // Girar a la izquierda (90 grados)
                InitiateTurn(-90f);
            }
            else if (!isRotating)
            {
                // Cambiar al carril izquierdo
                ChangeLane(-1);
            }
        }
        else if (pressedRight)
        {
            if (isInTurnTrigger && canTurn)
            {
                // Girar a la derecha (90 grados)
                InitiateTurn(90f);
            }
            else if (!isRotating)
            {
                // Cambiar al carril derecho
                ChangeLane(1);
            }
        }
    }

    #endregion

    #region Lane System

    /// <summary>
    /// Cambia el carril objetivo del personaje.
    /// </summary>
    /// <param name="direction">-1 para izquierda, 1 para derecha</param>
    private void ChangeLane(int direction)
    {
        int newLane = (int)currentLane + direction;

        // Limitar los carriles entre -1 (Left) y 1 (Right)
        newLane = Mathf.Clamp(newLane, -1, 1);

        if (newLane != (int)currentLane)
        {
            currentLane = (Lane)newLane;
            targetLane = currentLane;
            targetLateralPosition = (int)currentLane * laneDistance;
        }
    }

    /// <summary>
    /// Actualiza la posición lateral del personaje con transición suave.
    /// </summary>
    private void UpdateLanePosition()
    {
        // Interpolación suave hacia el carril objetivo
        currentLateralPosition = Mathf.MoveTowards(
            currentLateralPosition,
            targetLateralPosition,
            laneChangeSpeed * Time.deltaTime
        );
    }

    #endregion

    #region Turn System

    /// <summary>
    /// Inicia un giro de 90 grados en la dirección especificada.
    /// </summary>
    /// <param name="angle">Ángulo de giro (90 para derecha, -90 para izquierda)</param>
    private void InitiateTurn(float angle)
    {
        // Calcular la nueva rotación objetivo
        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + angle, 0);

        // Actualizar las direcciones de movimiento según el ángulo
        forwardDirection = targetRotation * Vector3.forward;
        rightDirection = targetRotation * Vector3.right;

        // Resetear la posición lateral al centro del nuevo camino
        currentLateralPosition = 0f;
        targetLateralPosition = 0f;
        currentLane = Lane.Center;
        targetLane = Lane.Center;

        isRotating = true;
        canTurn = false;
    }

    /// <summary>
    /// Actualiza la rotación del personaje hacia el objetivo.
    /// </summary>
    private void UpdateRotation()
    {
        if (isRotating)
        {
            // Interpolar suavemente hacia la rotación objetivo
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            // Verificar si la rotación está completa
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Aplica el movimiento final al CharacterController.
    /// </summary>
    private void ApplyMovement()
    {
        // Aplicar gravedad
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f; // Pequeña fuerza para mantener grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Calcular movimiento hacia adelante
        Vector3 forwardMovement = forwardDirection * forwardSpeed;

        // Calcular movimiento lateral (relativo a la dirección actual)
        Vector3 lateralMovement = rightDirection * currentLateralPosition;

        // Combinar movimientos
        Vector3 targetPosition = forwardMovement + lateralMovement;

        // Aplicar gravedad
        targetPosition.y = verticalVelocity;

        // Mover el personaje
        characterController.Move(targetPosition * Time.deltaTime);
    }

    #endregion

    #region Trigger Detection

    /// <summary>
    /// Detecta cuando el personaje entra en un trigger de giro.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnTrigger = true;
            canTurn = true;
        }
    }

    /// <summary>
    /// Detecta cuando el personaje sale de un trigger de giro.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnTrigger = false;
        }
    }

    #endregion

    #region Public Methods (Para futuras expansiones)

    /// <summary>
    /// Obtiene la velocidad actual de avance.
    /// </summary>
    public float GetForwardSpeed() => forwardSpeed;

    /// <summary>
    /// Modifica la velocidad de avance (útil para power-ups).
    /// </summary>
    public void SetForwardSpeed(float speed) => forwardSpeed = speed;

    /// <summary>
    /// Verifica si el personaje está en el suelo.
    /// </summary>
    public bool IsGrounded() => characterController.isGrounded;

    /// <summary>
    /// Obtiene el carril actual del personaje.
    /// </summary>
    public int GetCurrentLane() => (int)currentLane;

    #endregion
}