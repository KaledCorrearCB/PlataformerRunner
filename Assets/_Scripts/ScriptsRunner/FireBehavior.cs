using UnityEngine;
using System.Collections;

public class FireBehavior : MonoBehaviour
{
    [Header("Ajustes de Apagado")]
    public float extinguishSpeed = 1.5f; // Segundos que tarda en apagarse
    private bool isExtinguishing = false;

    // Referencia opcional si usas partículas
    private ParticleSystem particles;

    void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
    }

    public void StartExtinguishing()
    {
        if (!isExtinguishing)
        {
            StartCoroutine(ExtinguishRoutine());
        }
    }

    private IEnumerator ExtinguishRoutine()
    {
        isExtinguishing = true;
        float timer = 0;
        Vector3 initialScale = transform.localScale;

        // Si tienes partículas, dejamos de emitir nuevas
        if (particles != null)
        {
            var emission = particles.emission;
            emission.enabled = false;
        }

        while (timer < extinguishSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer / extinguishSpeed;

            // Reducción gradual de escala
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);

            yield return null;
        }

        Destroy(gameObject);
    }
}