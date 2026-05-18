using UnityEngine;
using System.Collections;

public class UnlockableMechanic : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────
    [Header("UI del letrero")]
    public Sprite signIcon; // arrastra el icono desde el Inspector
    [Header("¿Es desbloqueable?")]
    public bool isUnlockable = true;
    public bool IsUnlocked => _isUnlocked;
    [Header("Requisito de desbloqueo")]
    [Tooltip("Total de personas ayudadas necesarias")]
    public int requiredHelpedCount = 2;

    [Tooltip("Filtrar por tipo de kit específico. Si no importa el tipo, deja 'None'")]
    public bool filterByKitType = false;
    public KitType requiredKitType;

    [Header("Referencias")]
    [Tooltip("El letrero que aparece antes de construir")]
    public GameObject signObject;

    [Tooltip("La mecánica en sí (Trampoline, puerta, etc.) — se activa al desbloquear)")]
    public GameObject mechanicRoot;

    [Tooltip("Prefab del personaje constructor")]
    public GameObject builderPrefab;

    [Tooltip("Offset de spawn del constructor respecto al centro de la mecánica")]
    public Vector3 builderSpawnOffset = new Vector3(1.5f, 0f, 0f);

    [Header("Tiempos")]
    public float buildDuration = 2f;   // cuánto tarda en "construir"
    public float exitDuration = 1.5f; // cuánto tarda en irse

    [Header("Audio")]
    public AudioClip buildSound;
    private AudioSource _audioSource;

    [Header("VFX")]
    public GameObject buildVFX;    // partícula en loop mientras construye
    public GameObject completeVFX; // partícula que se reproduce al terminar

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private bool _isUnlocked = false;
    private bool _isBuilding = false;

    // ─────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────

    void Start()
    {


        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (!isUnlockable)
        {
            _isUnlocked = true;
            if (signObject != null) signObject.SetActive(false);
            if (mechanicRoot != null) mechanicRoot.SetActive(true);
            return;
        }

        // ✅ Validación: el letrero nunca debe ser hijo de mechanicRoot
        if (mechanicRoot != null && signObject != null
            && signObject.transform.IsChildOf(mechanicRoot.transform))
        {
            Debug.LogError($"[UnlockableMechanic] '{signObject.name}' es hijo de " +
                           $"'{mechanicRoot.name}'. El letrero debe ser hijo directo del root, " +
                           $"no de mechanicRoot.");
            return;
        }

        if (signObject != null) signObject.SetActive(true);
        if (mechanicRoot != null) mechanicRoot.SetActive(false);
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0.3f; // ← ajusta este valor a tu gusto
    }

    // ─────────────────────────────────────────────
    //  API pública
    // ─────────────────────────────────────────────

    /// <summary>
    /// Llamado desde PlayerController.OnInteract cuando el jugador
    /// interactúa con el letrero.
    /// </summary>
    public void TryUnlock()
    {
        if (_isUnlocked || _isBuilding) return;

        // Verificar requisito
        int helpedCount = filterByKitType
            ? HelpedCharactersData.GetHelpedByType(requiredKitType)
            : HelpedCharactersData.GetTotalHelped();

        if (helpedCount < requiredHelpedCount)
        {
            int faltantes = requiredHelpedCount - helpedCount;
            Debug.Log($"[UnlockableMechanic] Faltan {faltantes} persona(s) por ayudar.");
            // Aquí puedes mostrar un mensaje en UI si tienes uno
            return;
        }

        // ¡Requisito cumplido!
        StartCoroutine(UnlockSequence());
    }

    /// <summary>
    /// Devuelve si esta mecánica está lista para interactuar
    /// (es desbloqueable, no está ya desbloqueada, y no está construyendo).
    /// </summary>
    public bool IsInteractable() => isUnlockable && !_isUnlocked && !_isBuilding;

    // ─────────────────────────────────────────────
    //  Secuencia de desbloqueo
    // ─────────────────────────────────────────────

    private IEnumerator UnlockSequence()
    {
        _isBuilding = true;

        if (PlayerController.instance != null)
            PlayerController.instance.currentUnlockable = null;

        // ✅ Spawnear VFX de construcción en loop
        GameObject buildEffect = null;
        if (buildVFX != null)
            buildEffect = Instantiate(buildVFX, transform.position, Quaternion.identity);

        // Audio
        if (buildSound != null)
        {
            _audioSource.clip = buildSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        GameObject builder = null;
        Animator builderAnim = null;

        if (builderPrefab != null)
        {
            Vector3 spawnPos = transform.position + builderSpawnOffset;
            builder = Instantiate(builderPrefab, spawnPos, Quaternion.identity);
            builderAnim = builder.GetComponent<Animator>();

            Vector3 dir = transform.position - spawnPos;
            dir.y = 0f;
            if (dir != Vector3.zero)
                builder.transform.rotation = Quaternion.LookRotation(dir);

            if (builderAnim != null)
                builderAnim.SetTrigger("Build");
        }

        yield return new WaitForSeconds(buildDuration);

        // ✅ Destruir VFX de construcción
        if (buildEffect != null)
            Destroy(buildEffect);

        // ✅ Detener audio
        _audioSource.Stop();

        // ✅ Spawnear VFX de completado (se destruye solo cuando termina)
        if (completeVFX != null)
        {
            GameObject completeEffect = Instantiate(completeVFX, transform.position, Quaternion.identity);

            // Si el sistema de partículas tiene duración definida, se autodestruye
            ParticleSystem ps = completeEffect.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(completeEffect, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(completeEffect, 3f); // fallback por si no tiene ParticleSystem directo
        }

        if (mechanicRoot != null) mechanicRoot.SetActive(true);
        if (signObject != null) signObject.SetActive(false);

        _isUnlocked = true;

        if (builder != null)
        {
            if (builderAnim != null)
                builderAnim.SetTrigger("Exit");

            yield return new WaitForSeconds(exitDuration);
            Destroy(builder);
        }

        _isBuilding = false;
    }
}