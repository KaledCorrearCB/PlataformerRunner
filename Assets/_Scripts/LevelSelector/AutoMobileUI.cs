using UnityEngine;

public class AutoMobileUI : MonoBehaviour
{
    void Start()
    {
        // Si el juego NO está compilado para Android ni para iOS, apaga este Canvas
#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }
}