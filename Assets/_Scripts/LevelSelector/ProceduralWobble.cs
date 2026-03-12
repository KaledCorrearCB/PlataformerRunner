using System.Collections;
using UnityEngine;

public class ProceduralWobble : MonoBehaviour
{
    [Header("Ajustes de Gelatina")]
    [Tooltip("Que tanto se estira/contrae. Valores entre 0.1 y 0.5 son buenos.")]
    public float wobbleIntensity = 0.3f; // Intensidad inicial
    [Tooltip("Que tan rapido bota. Mayor = mas rapido.")]
    public float wobbleSpeed = 15f;    // Frecuencia
    [Tooltip("Que tan rapido para de botar. Mayor = para antes.")]
    public float wobbleDamping = 4f;   // Amortiguación

    private Vector3 originalScale;     // Escala de reposo
    private Coroutine currentWobble;   // Referencia a la corrutina activa

    private void Start()
    {
        // 1. Guardamos la escala original para tener una base
        originalScale = transform.localScale;
    }

    // 2. Esta funcion sera llamada por el script del jugador
    public void TriggerWobble()
    {
        // Si ya esta wobbling, paramos la corrutina vieja para iniciar una nueva limpia
        if (currentWobble != null)
        {
            StopCoroutine(currentWobble);
        }

        // Iniciamos el efecto
        currentWobble = StartCoroutine(WobbleRoutine());
    }

    // 3. La matemágica del efecto jelly (corrutina)
    private IEnumerator WobbleRoutine()
    {
        float time = 0f;

        // El bucle sigue mientras el wobble sea visible
        while (time < 1f)
        {
            time += Time.deltaTime * wobbleDamping;

            // Calculamos un valor oscilante que decrece con el tiempo
            // Usamos Coseno para que empiece con la maxima fuerza en t=0
            float sinWave = Mathf.Cos(time * wobbleSpeed);

            // Factor de caida: va de 1 a 0
            float decay = 1f - time;

            // Escala modificada: EscalaOriginal + (Intensidad * Onda * Caida)
            // Esto hace que el objeto baje un poco (en Y) y se estire (en X/Z) 
            // como si se aplastara al chocar.
            Vector3 wobbleScale = new Vector3(
                originalScale.x + (originalScale.x * wobbleIntensity * sinWave * decay),
                originalScale.y - (originalScale.y * wobbleIntensity * sinWave * decay), // Invertimos Y
                originalScale.z + (originalScale.z * wobbleIntensity * sinWave * decay)
            );

            // Aplicamos la nueva escala
            transform.localScale = wobbleScale;

            yield return null; // Esperamos al siguiente frame
        }

        // 4. Al terminar, forzamos la escala original para no tener errores acumulados
        transform.localScale = originalScale;
        currentWobble = null;
    }
}