using UnityEngine;

public class TrampolinePlataform : MonoBehaviour
{
    [Header("Configuración")]
    public float bounceForce = 18f;       // fuerza de salto
    public float cooldown = 0.5f;       // tiempo mínimo entre rebotes

    private TrampolineAnimation trampolineAnim;
    private float lastBounceTime = -999f;
    private UnlockableMechanic _unlockable;
    void Awake()
    {
        trampolineAnim = GetComponent<TrampolineAnimation>();
        // Busca en el padre por si el collider está en el root
        _unlockable = GetComponentInParent<UnlockableMechanic>();
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

        // ✅ Si hay una mecánica desbloqueable y aún no está lista, ignorar
        if (_unlockable != null && !_unlockable.IsUnlocked) return;

        PlayerController player = obj.GetComponent<PlayerController>();
        if (player == null) return;

        bool caeDesdeArriba = obj.transform.position.y > transform.position.y + 0.1f;
        bool estaBajando = player.VerticalVelocity < 0f;

        if (!caeDesdeArriba || !estaBajando) return;

        lastBounceTime = Time.time;
        player.Bounce(bounceForce);
        trampolineAnim.DispararAnimacion();
    }
}
