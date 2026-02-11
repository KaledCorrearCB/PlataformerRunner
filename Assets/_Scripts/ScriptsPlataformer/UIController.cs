using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    public Image fadeScreem;
    private bool isFandingToBlack, isFadingFromBlack;
    public float fadingTime;

    public void Awake()
    {
        
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isFandingToBlack)
        {
            fadeScreem.color = new Color(fadeScreem.color.r, fadeScreem.color.g, fadeScreem.color.b, Mathf.MoveTowards(fadeScreem.color.a, 1f, fadingTime * Time.deltaTime) );
        }

        if(isFadingFromBlack)
        {
            fadeScreem.color = new Color(fadeScreem.color.r, fadeScreem.color.g, fadeScreem.color.b, Mathf.MoveTowards(fadeScreem.color.a, 0f, fadingTime * Time.deltaTime));
        }
    }

    public void FadeToBlack()
    {
        isFandingToBlack = true;
        isFadingFromBlack = false;
    }

    public void FadeFromBlack()
    {
        isFandingToBlack = false;
        isFadingFromBlack = true;
    }
}
