using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrapplingRope : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────

    [Header("Punto de anclaje")]
    [Tooltip("El Transform de donde cuelga la cuerda (la barra superior)")]
    public Transform anchorPoint;

    [Header("Configuración de swing")]
    [Tooltip("Fuerza extra que se aplica al soltar para el impulso")]
    public float launchForce = 8f;

    [Tooltip("Velocidad máxima durante el balanceo")]
    public float maxSwingSpeed = 12f;

    // ─────────────────────────────────────────
    //  Privados
    // ─────────────────────────────────────────

    private PlayerController player;
    private CharacterController charCon;
    private Transform playerTransform;

    private bool isSwinging;
    private float ropeLength;
    private Vector3 swingVelocity;

    public bool IsSwinging => isSwinging;
    public Vector3 PlayerPosition => playerTransform != null
     ? playerTransform.position + Vector3.up * 1f  // ajusta el 1f a la altura de tus manos
     : Vector3.zero;

    private Vector3 frozenPosition; // ✅ posición donde se congela el jugador

    private float releaseCooldown = 0f;
    private const float RELEASE_COOLDOWN = 0.5f;

    // ─────────────────────────────────────────
    //  Trigger — entrada y salida del jugador
    // ─────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // ✅ Ignorar re-entrada durante el cooldown post-suelta
        if (releaseCooldown > 0f) return;
        if (isSwinging) return;
        if (!other.CompareTag("Player")) return;

        player = other.GetComponent<PlayerController>();
        charCon = other.GetComponent<CharacterController>();
        playerTransform = other.transform;

        if (player == null || charCon == null) return;

        StartSwing();
    }

    private void OnTriggerExit(Collider other)
    {
        // Seguridad: si el jugador sale del trigger sin soltar, lo liberamos
        if (!isSwinging) return;
        if (!other.CompareTag("Player")) return;

        // Solo lo liberamos si el trigger es más pequeño que el arco
        // (por ahora lo dejamos que el jugador use Jump para soltar)

        ForceRelease();
    }


    // Limpieza de emergencia — por si el jugador muere o se teletransporta
    public void ForceRelease()
    {
        if (!isSwinging) return;

        isSwinging = false;
        releaseCooldown = RELEASE_COOLDOWN; // ✅ también en liberación forzada

        if (player != null) player.OnGrappleStop(Vector3.zero);

        player = null;
        charCon = null;
        playerTransform = null;

        Debug.Log("[GrapplingRope] Liberación forzada.");
    }

    private void OnDisable()
    {
        ForceRelease();
    }

    // ─────────────────────────────────────────
    //  Iniciar swing
    // ─────────────────────────────────────────



    private void StartSwing()
    {
        ropeLength = Vector3.Distance(playerTransform.position, anchorPoint.position);

        // ✅ Guardar la posición de entrada y congelarlo ahí
        frozenPosition = playerTransform.position;

        isSwinging = true;
        player.OnGrappleStart(this);

        Debug.Log($"[GrapplingRope] Swing iniciado. Longitud={ropeLength:F1}m");
    }

    // ─────────────────────────────────────────
    //  FixedUpdate — física del péndulo rígido
    // ─────────────────────────────────────────

    void Update()
    {
        if (releaseCooldown > 0f) releaseCooldown -= Time.deltaTime;

        if (!isSwinging) return;

        // ✅ Mantener al jugador completamente estático en la posición de agarre
        playerTransform.position = frozenPosition;
    }

    // ─────────────────────────────────────────
    //  Soltar — llamado desde PlayerController
    //  cuando el jugador presiona Jump
    // ─────────────────────────────────────────

    public void ReleaseSwing()
    {
        if (!isSwinging) return;

        isSwinging = false;

        // ✅ Activar cooldown para evitar re-agarre inmediato
        releaseCooldown = RELEASE_COOLDOWN;

        Vector3 launchVelocity = Vector3.up * launchForce;
        player.OnGrappleStop(launchVelocity);

        player = null;
        charCon = null;
        playerTransform = null;

        Debug.Log($"[GrapplingRope] Soltado. Impulso={launchVelocity}");
    }

    // ─────────────────────────────────────────
    //  Gizmo — ver el ancla en Scene view
    // ─────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (anchorPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(anchorPoint.position, 0.2f);
        Gizmos.DrawLine(transform.position, anchorPoint.position);
    }





}