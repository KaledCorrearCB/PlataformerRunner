using UnityEngine;
using System.Collections;

public class UnlockableMechanic : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────

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
        if (!isUnlockable)
        {
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

        // 1 — Limpiar referencia del jugador para que no quede "atascado"
        if (PlayerController.instance != null)
            PlayerController.instance.currentUnlockable = null;

        // 2 — Spawnear constructor cerca de la mecánica
        GameObject builder = null;
        Animator builderAnim = null;

        if (builderPrefab != null)
        {
            Vector3 spawnPos = transform.position + builderSpawnOffset;
            builder = Instantiate(builderPrefab, spawnPos, Quaternion.identity);
            builderAnim = builder.GetComponent<Animator>();

            // Que mire hacia la mecánica
            Vector3 dir = (transform.position - spawnPos);
            dir.y = 0f;
            if (dir != Vector3.zero)
                builder.transform.rotation = Quaternion.LookRotation(dir);

            // Trigger animación de construcción
            if (builderAnim != null)
                builderAnim.SetTrigger("Build");
        }

        // 3 — Esperar la duración de construcción
        yield return new WaitForSeconds(buildDuration);

        // 4 — Activar mecánica y ocultar letrero
        if (mechanicRoot != null) mechanicRoot.SetActive(true);
        if (signObject != null) signObject.SetActive(false);

        _isUnlocked = true;
        Debug.Log($"[UnlockableMechanic] ¡{gameObject.name} desbloqueado!");

        // 5 — Animación de salida del constructor
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