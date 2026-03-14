using UnityEngine;

public class WorldUnlockManager : MonoBehaviour
{
    void Start()
    {
        // Al empezar el juego, buscamos todo lo que debería estar desbloqueado
        GameObject[] todos = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in todos)
        {
            // Buscamos si el objeto tiene el componente PetUnlocker
            PetUnlocker unlocker = go.GetComponent<PetUnlocker>();
            if (unlocker != null)
            {
                bool comprado = PlayerPrefs.GetInt(unlocker.itemName, 0) == 1;
                go.SetActive(comprado);
            }
        }
    }
}