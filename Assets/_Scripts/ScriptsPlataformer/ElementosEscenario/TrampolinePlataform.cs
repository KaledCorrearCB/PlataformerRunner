using UnityEngine;

public class TrampolinePlataform : MonoBehaviour
{
    [Header("Configuración")]
    public float bounceForce = 18f;       // fuerza de salto
    public float cooldown = 0.5f;       // tiempo mínimo entre rebotes

    private TrampolineAnimation trampolineAnim;
    private float lastBounceTime = -999f;

    void Awake()
    {
        trampolineAnim = GetComponent<TrampolineAnimation>();
    }

    // ── Detección ─────────────────────────────────────────────────────────────

    // Usa OnTriggerEnter si el Box Collider tiene "Is Trigger" = true  (tu caso ✅)
    void OnTriggerEnter(Collider other)
    {
        TryBounce(other.gameObject);
    }

    // ── Lógica ────────────────────────────────────────────────────────────────

    private void TryBounce(GameObject obj)
    {
        if (Time.time - lastBounceTime < cooldown) return;

        PlayerController player = obj.GetComponent<PlayerController>();
        if (player == null) return;

        // ✅ Condición 1: el jugador debe estar por encima del collider
        bool caeDesdeArriba = obj.transform.position.y > transform.position.y + 0.1f;

        // ✅ Condición 2: debe venir bajando (velocidad vertical negativa)
        // Accedemos a verticalVelocity mediante una propiedad pública
        bool estaBajando = player.VerticalVelocity < 0f;

        if (!caeDesdeArriba || !estaBajando) return;

        lastBounceTime = Time.time;
        player.Bounce(bounceForce);
        trampolineAnim.DispararAnimacion();
    }
}
