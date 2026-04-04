// CharacterDetector.cs
// Mecánica de detección tipo "Eagle Vision" de Assassin's Creed.
//
// FUNCIONAMIENTO:
//   - El jugador presiona la tecla asignada (acción "Detect" en el Input System).
//   - Se escanea un área esférica (detectionRadius) buscando CharacterInNeed no ayudados.
//   - Si se encuentra alguno, aparece una flecha 3D apuntando hacia el más cercano
//     y se reproduce un AudioClip.
//   - Tras (arrowDuration) segundos, la flecha y el sonido se apagan solos.
//
// SETUP EN UNITY:
//   1. Añade este componente al GameObject del jugador (junto a PlayerController).
//   2. En "Arrow Prefab" arrastra un prefab de flecha 3D (p.ej. una flecha con un mesh).
//      La flecha debe apuntar hacia +Z en local space.
//   3. Opcional: asigna un AudioClip en "Detect Sound".
//   4. Añade la acción "Detect" en tu Input Actions Asset (p.ej. tecla Q o Tab).
//   5. En PlayerInput Component, asigna el callback OnDetect.
//
// INTEGRACIÓN CON PLAYERCONTROLLER:
//   - No toca PlayerController; es completamente independiente.
//   - Usa la misma capa de CharacterInNeed para la detección.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterDetector : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────
    [Header("Detección")]
    [Tooltip("Radio del área de escaneo en metros.")]
    public float detectionRadius = 20f;

    [Header("Flecha")]
    [Tooltip("Prefab de la flecha 3D. Debe apuntar en +Z en local space.")]
    public GameObject arrowPrefab;

    [Tooltip("Offset vertical sobre el jugador donde flota la flecha.")]
    public float arrowHeightOffset = 2.5f;

    [Tooltip("Cuántos segundos permanece visible la flecha.")]
    public float arrowDuration = 4f;

    [Header("Sonido")]
    [Tooltip("Sonido que se reproduce al detectar un personaje.")]
    public AudioClip detectSound;

    [Tooltip("Volumen del sonido de detección.")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Cooldown")]
    [Tooltip("Tiempo de espera entre usos de la habilidad (segundos).")]
    public float cooldown = 3f;

    // ─────────────────────────────────────────────
    //  Privados
    // ─────────────────────────────────────────────
    private GameObject _activeArrow;
    private AudioSource _audioSource;
    private Coroutine _hideCoroutine;
    private float _lastUsedTime = -999f;

    // ─────────────────────────────────────────────
    //  Unity
    // ─────────────────────────────────────────────
    private void Awake()
    {
        // AudioSource dedicado para el efecto de detección
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // sonido 2D (UI feel)
    }

    private void OnDestroy()
    {
        if (_activeArrow != null)
            Destroy(_activeArrow);
    }

    // ─────────────────────────────────────────────
    //  Input System callback
    //  Conecta desde el componente PlayerInput:
    //    Send Messages → OnDetect
    //  O desde un InputActionReference vía PlayerInput.onActionTriggered.
    // ─────────────────────────────────────────────
    public void OnDetect(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        TryDetect();
    }

    // También puede llamarse directamente si prefieres manejarlo en PlayerController
    public void TryDetect()
    {
        // Cooldown guard
        if (Time.time - _lastUsedTime < cooldown)
        {
            float remaining = cooldown - (Time.time - _lastUsedTime);
            Debug.Log($"[CharacterDetector] Habilidad en cooldown. Espera {remaining:F1}s.");
            return;
        }

        CharacterInNeed target = FindNearestCharacterInNeed();

        if (target == null)
        {
            Debug.Log("[CharacterDetector] No hay personajes en necesidad cerca.");
            return;
        }

        _lastUsedTime = Time.time;

        // Mostrar flecha
        ShowArrow(target.transform);

        // Reproducir sonido
        PlayDetectSound();

        Debug.Log($"[CharacterDetector] ¡Personaje detectado! → {target.characterName}");
    }

    // ─────────────────────────────────────────────
    //  Lógica interna
    // ─────────────────────────────────────────────

    /// <summary>
    /// Busca el CharacterInNeed no ayudado más cercano dentro del radio.
    /// </summary>
    private CharacterInNeed FindNearestCharacterInNeed()
    {
        // OverlapSphere detecta todos los colliders en el radio
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        CharacterInNeed nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            CharacterInNeed candidate = col.GetComponentInParent<CharacterInNeed>();
            if (candidate == null) continue;

            // Verificamos que no haya sido ayudado ya (campo privado → usamos el truco del tag o un getter)
            if (candidate.IsAlreadyHelped()) continue;

            float dist = Vector3.Distance(transform.position, candidate.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = candidate;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Instancia (o reutiliza) la flecha y la orienta hacia el objetivo.
    /// </summary>
    private void ShowArrow(Transform target)
    {
        // Cancelar ocultamiento anterior si existía
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        // Crear o reutilizar flecha
        if (_activeArrow == null)
        {
            if (arrowPrefab != null)
            {
                _activeArrow = Instantiate(arrowPrefab);
            }
            else
            {
                // Fallback: flecha primitiva si no se asigna prefab
                _activeArrow = CreateFallbackArrow();
            }
        }

        _activeArrow.SetActive(true);

        // Posicionar sobre el jugador
        _activeArrow.transform.position = transform.position + Vector3.up * arrowHeightOffset;

        // Iniciar coroutine que actualiza la rotación y luego la oculta
        _hideCoroutine = StartCoroutine(AnimateAndHideArrow(target));
    }

    /// <summary>
    /// Durante arrowDuration segundos rota la flecha hacia el objetivo cada frame,
    /// luego la desactiva.
    /// </summary>
    private IEnumerator AnimateAndHideArrow(Transform target)
    {
        float elapsed = 0f;

        while (elapsed < arrowDuration)
        {
            if (_activeArrow == null) yield break;

            // Posición siempre encima del jugador
            _activeArrow.transform.position = transform.position + Vector3.up * arrowHeightOffset;

            // Dirección horizontal hacia el objetivo
            if (target != null)
            {
                Vector3 dir = target.position - _activeArrow.transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    _activeArrow.transform.rotation = Quaternion.Slerp(
                        _activeArrow.transform.rotation,
                        targetRot,
                        15f * Time.deltaTime
                    );
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ocultar al terminar el tiempo
        HideArrow();
    }

    private void HideArrow()
    {
        if (_activeArrow != null)
            _activeArrow.SetActive(false);
    }

    private void PlayDetectSound()
    {
        if (detectSound == null || _audioSource == null) return;

        _audioSource.Stop();
        _audioSource.clip = detectSound;
        _audioSource.volume = soundVolume;
        _audioSource.Play();

        // Detener el sonido cuando expire la duración de la flecha
        StartCoroutine(StopSoundAfter(arrowDuration));
    }

    private IEnumerator StopSoundAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Stop();
    }

    // ─────────────────────────────────────────────
    //  Flecha de fallback (primitiva) — se usa si no
    //  se asigna ningún prefab en el inspector.
    // ─────────────────────────────────────────────
    private GameObject CreateFallbackArrow()
    {
        GameObject root = new GameObject("DetectionArrow_Fallback");

        // Cuerpo de la flecha (cilindro horizontal)
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.transform.SetParent(root.transform);
        shaft.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
        Destroy(shaft.GetComponent<Collider>());

        // Punta (cono = cápsula estirada)
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.transform.SetParent(root.transform);
        tip.transform.localPosition = new Vector3(0f, 0f, 1.1f);
        tip.transform.localScale = new Vector3(0.18f, 0.18f, 0.35f);
        Destroy(tip.GetComponent<Collider>());

        // Color amarillo vistoso
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.9f, 0f); // amarillo
        shaft.GetComponent<Renderer>().material = mat;
        tip.GetComponent<Renderer>().material = mat;

        return root;
    }

    // ─────────────────────────────────────────────
    //  Gizmos — visualiza el radio en el editor
    // ─────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}