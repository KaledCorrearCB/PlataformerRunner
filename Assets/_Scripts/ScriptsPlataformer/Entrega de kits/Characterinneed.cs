// CharacterInNeed.cs
// Versión actualizada — agrega IsAlreadyHelped() para que CharacterDetector
// pueda filtrar personajes ya rescatados sin acceder al campo privado.
//
// SETUP EN UNITY:
//   1. Selecciona el Empty "Personaje 1" (o 2 o 3).
//   2. Add Component → CharacterInNeed.
//   3. Elige el KitType que ese personaje necesita.
//   4. Asegúrate de que el hijo Cilindro tiene un Collider con Is Trigger ✓.
//   5. Repite para Personaje 2 y 3.

using UnityEngine;
using System.Collections;

public class CharacterInNeed : MonoBehaviour
{
    [Header("Configuración del Personaje")]
    public KitType requiredKit;
    public string characterName = "Persona";

    private bool _alreadyHelped = false;

    [Header("Animación")]
    [SerializeField] private Animator _animator;

    [Header("Kit Visual")]
    public GameObject kitPrefab;          // arrastra el prefab del kit aquí
    public float floatHeight = 2f;        // altura sobre el personaje
    public float rotationSpeed = 90f;     // grados por segundo

    private GameObject _kitInstance;      // referencia interna

    // *** NUEVO — getter público para CharacterDetector ***
    /// <summary>Devuelve true si este personaje ya fue ayudado y no debe detectarse.</summary>
    /// 
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    public bool IsAlreadyHelped() => _alreadyHelped;

    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyHelped) return;
        if (!other.CompareTag("Player")) return;

        PlayerController.instance.currentCharacterInNeed = this;
        ShowKitIndicator();
        Debug.Log($"[{characterName}] Jugador cerca. Necesita: {requiredKit}. Presiona E.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlayerController.instance.currentCharacterInNeed == this)
            PlayerController.instance.currentCharacterInNeed = null;

        HideKitIndicator();
    }

    /// <summary>
    /// Llamado desde PlayerController.OnSelect() cuando el jugador presiona E.
    /// </summary>
    public void TryDeliverKit()
    {
        if (_alreadyHelped) return;

        if (KitInventory.Instance == null)
        {
            Debug.LogWarning("[CharacterInNeed] No hay KitInventory en la escena.");
            return;
        }

        bool success = KitInventory.Instance.SpendKit(requiredKit);
        if (success)
        {
            _alreadyHelped = true;

            HideKitIndicator();
            // *** ANIMACIÓN FELIZ ***
            _animator.SetBool("Ayudado", true);
            StartCoroutine(HappyThenDisappear());

            if (PlayerController.instance.currentCharacterInNeed == this)
                PlayerController.instance.currentCharacterInNeed = null;

            HelpedCharactersData.RegisterHelped(requiredKit);
            SessionManager.Instance?.RegisterHelped(requiredKit);
            Debug.Log($"[{characterName}] ¡Ayudado con {requiredKit}! " +
                      $"Total ayudados: {HelpedCharactersData.GetTotalHelped()}");

            // Opcional — descomenta la que prefieras:
            // gameObject.SetActive(false);
            // transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"[{characterName}] El jugador no tiene {requiredKit} disponible.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    private IEnumerator HappyThenDisappear()
    {

        yield return null;
        yield return null;

        float animLength = 1.5f;
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
                if (clip.name == "Feliz") { animLength = clip.length; break; }
        }
        yield return new WaitForSeconds(animLength);

        // Usamos el PADRE para la animación de escala
        Transform root = transform;
        Vector3 originalScale = root.localScale;
        Vector3 originalPosition = root.position;
        Vector3 bigScale = originalScale * 1.4f;
        float t = 0f;

        // Agrandarse
        while (t < 1f)
        {
            t += Time.deltaTime / 0.3f;
            root.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }

        // Encogerse subiendo
        t = 0f;
        Vector3 riseTarget = originalPosition + Vector3.up * 1.5f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.4f;
            root.localScale = Vector3.Lerp(bigScale, Vector3.zero, t);
            root.position = Vector3.Lerp(originalPosition, riseTarget, t);
            yield return null;
        }

        Destroy(root.gameObject); // ← destruye todo el personaje
    }

    private void ShowKitIndicator()
    {
        if (kitPrefab == null) return;
        if (_kitInstance != null) return; // ya está visible

        Vector3 spawnPos = transform.position + Vector3.up * floatHeight;
        _kitInstance = Instantiate(kitPrefab, spawnPos, Quaternion.identity);
        _kitInstance.transform.SetParent(transform); // sigue al personaje

        foreach (Renderer rend in _kitInstance.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in rend.materials)
            {
                // Activa el modo transparente
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                // Ajusta la opacidad (0 = invisible, 1 = sólido)
                Color c = mat.color;
                c.a = 0.95f; // ← cambia este valor a tu gusto
                mat.color = c;
            }
        }
    }

    private void HideKitIndicator()
    {
        if (_kitInstance == null) return;
        Destroy(_kitInstance);
        _kitInstance = null;
    }

    private void Update()
    {
        if (_kitInstance == null) return;
        _kitInstance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }


}