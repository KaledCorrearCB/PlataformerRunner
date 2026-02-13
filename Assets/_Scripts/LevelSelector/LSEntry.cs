using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LSEntry : MonoBehaviour
{
    public string levelName;
    public GameObject mapPointActive;
    public GameObject mapPointInactive;

    bool playerInside;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            mapPointActive.SetActive(true);
            mapPointInactive.SetActive(false);

            other.GetComponent<PlayerController>().currentLevelNode = this;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            mapPointActive.SetActive(false);
            mapPointInactive.SetActive(true);

            if (other.GetComponent<PlayerController>().currentLevelNode == this)
                other.GetComponent<PlayerController>().currentLevelNode = null;
        }
    }

    public void LoadLevel()
    {
        if (!playerInside) return;
        StartCoroutine(loadLevelCo());
    }

    IEnumerator loadLevelCo()
    {
        FindAnyObjectByType<PlayerController>().stopMoving = true;
        UIController.instance.FadeToBlack();

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(levelName);
    }
}
