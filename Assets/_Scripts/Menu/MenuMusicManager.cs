using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    [Header("Configuración")]
    public AudioClip menuMusic;
    [Range(0f, 1f)] public float volume = 0.4f;
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.5f;

    [Header("Escenas de juego (sin música)")]
    public string[] gameSceneNames; // ej: "PlatformBase", "Level1"

    private AudioSource _audioSource;
    private static MenuMusicManager _instance;

    void Awake()
    {
        // Singleton — que sobreviva entre menús
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = menuMusic;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0f;
    }

    void Start()
    {
        _audioSource.Play();
        StartCoroutine(FadeIn());

        // Escuchar cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si la escena cargada es de juego, fade out y destruir
        foreach (string gameName in gameSceneNames)
        {
            if (scene.name == gameName)
            {
                StartCoroutine(FadeOutAndDestroy());
                return;
            }
        }

        // Si es otro menú, asegurarse de que siga sonando
        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
            StartCoroutine(FadeIn());
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeInDuration);
            yield return null;
        }
        _audioSource.volume = volume;
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float timer = 0f;
        float startVolume = _audioSource.volume;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }
        _instance = null;
        Destroy(gameObject);
    }
}