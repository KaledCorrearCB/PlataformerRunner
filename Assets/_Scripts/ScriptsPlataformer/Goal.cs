using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private string levelSelectorScene = "LevelSelector";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.CompleteLevel(levelSelectorScene);
        }
    }
}