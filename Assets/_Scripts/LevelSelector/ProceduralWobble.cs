using System.Collections;
using UnityEngine;

public class ProceduralWobble : MonoBehaviour
{
    [Header("Ajustes de Feeling (Script A)")]
    [Tooltip("Arboles: 0.2 | Arbustos: 0.05")]
    public float intensity = 0.2f;
    public float speed = 15f;
    public float damping = 5f;

    private Vector3 originalScale;
    private Coroutine currentWobble;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void TriggerWobble()
    {
        if (currentWobble != null) StopCoroutine(currentWobble);
        currentWobble = StartCoroutine(WobbleRoutine());
    }

    private IEnumerator WobbleRoutine()
    {
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * damping;

            // Usamos la onda del Script A
            float oscillation = Mathf.Cos(time * speed);
            float decay = 1f - time;
            float offset = intensity * oscillation * decay;

            // Aplicamos la matematica exacta del Script A que te gusto:
            // X y Z aumentan, Y disminuye (o viceversa) segun el offset
            Vector3 targetScale = new Vector3(
                originalScale.x + (offset * originalScale.x),
                originalScale.y - (offset * originalScale.y),
                originalScale.z + (offset * originalScale.z)
            );

            // PROTECCION PARA ARBUSTOS: Evita que la escala sea negativa o cero
            targetScale.x = Mathf.Max(targetScale.x, 0.01f);
            targetScale.y = Mathf.Max(targetScale.y, 0.01f);
            targetScale.z = Mathf.Max(targetScale.z, 0.01f);

            transform.localScale = targetScale;
            yield return null;
        }

        transform.localScale = originalScale;
        currentWobble = null;
    }
}