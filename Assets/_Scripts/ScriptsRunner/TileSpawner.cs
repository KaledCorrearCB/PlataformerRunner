using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de generación procedural de tiles para Endless Runner.
/// Gestiona el spawn infinito de piezas de nivel y la optimización de memoria.
/// </summary>
public class TileSpawner : MonoBehaviour
{
    #region Serialized Fields

    [Header("Tile Prefabs")]
    [Tooltip("Prefab del tile base/inicial (siempre se genera primero)")]
    [SerializeField] private GameObject baseTilePrefab;

    [Tooltip("Lista de prefabs de tiles para generación aleatoria")]
    [SerializeField] private List<GameObject> tilePrefabs = new List<GameObject>();

    [Header("Spawning Settings")]
    [Tooltip("Número de tiles a generar al inicio del juego")]
    [SerializeField] private int initialTileCount = 5;

    [Tooltip("Número máximo de tiles activos en escena antes de eliminar los antiguos")]
    [SerializeField] private int maxActiveTiles = 8;

    [Header("Debug Settings")]
    [Tooltip("Mostrar mensajes de debug en la consola")]
    [SerializeField] private bool showDebugMessages = false;

    #endregion

    #region Private Fields

    // Lista que mantiene referencia a todos los tiles activos
    private List<GameObject> activeTiles = new List<GameObject>();

    // Referencia al último tile spawneado
    private GameObject lastTile;

    // Transform del último ExitPoint utilizado
    private Transform lastExitPoint;

    // Nombres de objetos hijo importantes
    private const string EXIT_POINT_NAME = "ExitPoint";

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeTileGeneration();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Inicializa el sistema de generación de tiles.
    /// </summary>
    private void InitializeTileGeneration()
    {
        // Validar que tenemos los prefabs necesarios
        if (!ValidatePrefabs())
        {
            Debug.LogError("[TileSpawner] Error: Faltan prefabs asignados. Verifica el Inspector.");
            return;
        }

        // Generar el pool inicial de tiles
        GenerateInitialTiles();

        if (showDebugMessages)
        {
            Debug.Log($"[TileSpawner] Inicialización completada. {activeTiles.Count} tiles generados.");
        }
    }

    /// <summary>
    /// Valida que todos los prefabs necesarios estén asignados.
    /// </summary>
    private bool ValidatePrefabs()
    {
        if (baseTilePrefab == null)
        {
            Debug.LogError("[TileSpawner] El Base Tile Prefab no está asignado.");
            return false;
        }

        if (tilePrefabs == null || tilePrefabs.Count == 0)
        {
            Debug.LogWarning("[TileSpawner] No hay tiles en la lista. Solo se generará el tile base.");
        }

        return true;
    }

    #endregion

    #region Tile Generation

    /// <summary>
    /// Genera el conjunto inicial de tiles al comenzar el juego.
    /// </summary>
    private void GenerateInitialTiles()
    {
        // Generar el primer tile (siempre el base)
        GameObject firstTile = SpawnBaseTile();

        if (firstTile == null)
        {
            Debug.LogError("[TileSpawner] No se pudo generar el tile base inicial.");
            return;
        }

        // Generar los tiles restantes de forma aleatoria
        for (int i = 1; i < initialTileCount; i++)
        {
            SpawnTile();
        }
    }

    /// <summary>
    /// Genera el tile base en el origen del TileSpawner.
    /// </summary>
    private GameObject SpawnBaseTile()
    {
        // Instanciar el tile base en la posición del spawner
        GameObject newTile = Instantiate(baseTilePrefab, transform.position, transform.rotation);
        newTile.name = $"Tile_Base_0";

        // Agregar a la lista de tiles activos
        activeTiles.Add(newTile);
        lastTile = newTile;

        // Buscar el ExitPoint del tile base
        lastExitPoint = FindExitPoint(newTile);

        if (lastExitPoint == null)
        {
            Debug.LogError($"[TileSpawner] El tile base '{baseTilePrefab.name}' no tiene un hijo llamado '{EXIT_POINT_NAME}'.");
        }

        return newTile;
    }

    /// <summary>
    /// Genera un nuevo tile aleatorio en la posición del último ExitPoint.
    /// Esta función es llamada públicamente cuando el jugador avanza.
    /// </summary>
    public void SpawnTile()
    {
        // Verificar que tenemos un ExitPoint válido
        if (lastExitPoint == null)
        {
            Debug.LogError("[TileSpawner] No hay ExitPoint válido para generar el siguiente tile.");
            return;
        }

        // Seleccionar un tile aleatorio de la lista
        GameObject selectedPrefab = GetRandomTilePrefab();

        if (selectedPrefab == null)
        {
            Debug.LogError("[TileSpawner] No se pudo seleccionar un tile válido.");
            return;
        }

        // Instanciar el nuevo tile en la posición y rotación del ExitPoint
        GameObject newTile = Instantiate(
            selectedPrefab,
            lastExitPoint.position,
            lastExitPoint.rotation
        );

        // Nombrar el tile para facilitar el debug
        newTile.name = $"{selectedPrefab.name}_{activeTiles.Count}";

        // Agregar a la lista de tiles activos
        activeTiles.Add(newTile);
        lastTile = newTile;

        // Buscar el ExitPoint del nuevo tile
        lastExitPoint = FindExitPoint(newTile);

        if (lastExitPoint == null)
        {
            Debug.LogWarning($"[TileSpawner] El tile '{selectedPrefab.name}' no tiene un ExitPoint. La generación podría detenerse.");
        }

        // Optimizar memoria eliminando tiles antiguos
        OptimizeActiveTiles();

        if (showDebugMessages)
        {
            Debug.Log($"[TileSpawner] Tile spawneado: {newTile.name} | Total activos: {activeTiles.Count}");
        }
    }

    #endregion

    #region Tile Selection

    /// <summary>
    /// Selecciona un tile aleatorio de la lista de prefabs.
    /// </summary>
    private GameObject GetRandomTilePrefab()
    {
        if (tilePrefabs == null || tilePrefabs.Count == 0)
        {
            // Si no hay tiles en la lista, usar el tile base
            return baseTilePrefab;
        }

        // Seleccionar un índice aleatorio
        int randomIndex = Random.Range(0, tilePrefabs.Count);
        GameObject selectedTile = tilePrefabs[randomIndex];

        // Validar que el prefab no sea nulo
        if (selectedTile == null)
        {
            Debug.LogWarning($"[TileSpawner] El tile en el índice {randomIndex} es null. Usando tile base.");
            return baseTilePrefab;
        }

        return selectedTile;
    }

    #endregion

    #region Tile Management

    /// <summary>
    /// Busca el Transform del ExitPoint en un tile.
    /// </summary>
    private Transform FindExitPoint(GameObject tile)
    {
        if (tile == null)
            return null;

        // Buscar el hijo llamado "ExitPoint"
        Transform exitPoint = tile.transform.Find(EXIT_POINT_NAME);

        if (exitPoint == null)
        {
            // Buscar recursivamente en todos los hijos
            exitPoint = FindExitPointRecursive(tile.transform);
        }

        return exitPoint;
    }

    /// <summary>
    /// Búsqueda recursiva del ExitPoint en la jerarquía del tile.
    /// </summary>
    private Transform FindExitPointRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == EXIT_POINT_NAME)
                return child;

            Transform found = FindExitPointRecursive(child);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Elimina tiles antiguos cuando se supera el límite máximo.
    /// </summary>
    private void OptimizeActiveTiles()
    {
        // Verificar si hay más tiles activos que el límite permitido
        while (activeTiles.Count > maxActiveTiles)
        {
            // Obtener el tile más antiguo (el primero de la lista)
            GameObject oldestTile = activeTiles[0];

            // Remover de la lista
            activeTiles.RemoveAt(0);

            // Destruir el GameObject
            if (oldestTile != null)
            {
                if (showDebugMessages)
                {
                    Debug.Log($"[TileSpawner] Eliminando tile antiguo: {oldestTile.name}");
                }

                Destroy(oldestTile);
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Obtiene el número de tiles activos en escena.
    /// </summary>
    public int GetActiveTileCount()
    {
        return activeTiles.Count;
    }

    /// <summary>
    /// Obtiene una referencia al último tile generado.
    /// </summary>
    public GameObject GetLastTile()
    {
        return lastTile;
    }

    /// <summary>
    /// Reinicia el sistema de generación de tiles.
    /// Útil para reiniciar el nivel.
    /// </summary>
    public void ResetTileGeneration()
    {
        // Destruir todos los tiles activos
        foreach (GameObject tile in activeTiles)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }

        // Limpiar la lista
        activeTiles.Clear();
        lastTile = null;
        lastExitPoint = null;

        // Reinicializar
        InitializeTileGeneration();

        if (showDebugMessages)
        {
            Debug.Log("[TileSpawner] Sistema de tiles reiniciado.");
        }
    }

    /// <summary>
    /// Fuerza la generación de múltiples tiles.
    /// Útil para debug o situaciones especiales.
    /// </summary>
    public void SpawnMultipleTiles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnTile();
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Visualizar el ExitPoint del último tile
        if (lastExitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastExitPoint.position, 0.5f);
            Gizmos.DrawRay(lastExitPoint.position, lastExitPoint.forward * 2f);
        }
    }
#endif

    #endregion
}