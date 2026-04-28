using UnityEngine;

public class TrampolinePlataform : MonoBehaviour
{
    [Header("Configuración")]
    public float bounceForce = 18f;       // fuerza de salto
    public float cooldown = 0.5f;       // tiempo mínimo entre rebotes

    private TrampolineAnimation trampolineAnim;
    private float lastBounceTime = -999f;
    private UnlockableMechanic _unlockable;

    [Header("Audio")]
    public AudioClip bounceSound;
    [Range(0f, 1f)] public float bounceVolume = 0.5f;
    private AudioSource _audioSource;

    [Header("VFX")]
    public GameObject bounceVFX;

    void Awake()
    {
        trampolineAnim = GetComponent<TrampolineAnimation>();
        _unlockable = GetComponentInParent<UnlockableMechanic>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.volume = bounceVolume;
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

        // ✅ SFX
        if (bounceSound != null)
            _audioSource.PlayOneShot(bounceSound, bounceVolume);

        // ✅ VFX — se destruye solo cuando termina
        if (bounceVFX != null)
        {
            GameObject effect = Instantiate(bounceVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(effect, 2f);
        }
    }
}
