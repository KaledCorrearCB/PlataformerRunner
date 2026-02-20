using System.Collections;
using UnityEngine;

public class MovilePlatform : MonoBehaviour
{
    //Creamos una lista de puntos a los cuales se movera la plataforma
    public Transform[] points;

    //Variable que determina la velocidad a la cual se mueve la plataforma
    public float speed;

    // Posisicion actual
    int curPos = 0;
    // Posicion a la cual debe moverse
    int nextPos = 1;


    bool moveNext = true;
    public float timeToNext;




    public void Update()
    {
        if (moveNext)
        {
            transform.position = Vector3.MoveTowards(transform.position, points[nextPos].position, speed * Time.deltaTime);
        }


        if (Vector3.Distance(transform.position, points[nextPos].position) <= 0)
        {
            StartCoroutine(TimeToMove());
            curPos = nextPos;
            nextPos++;

            if (nextPos > points.Length - 1)
            {
                nextPos = 0;
            }
        }
    }


    // Co-rutina para que la plataforma espere en un sitio
    IEnumerator TimeToMove()
    {
        moveNext = false;
        yield return new WaitForSeconds(timeToNext);
        moveNext = true;


    }


}
