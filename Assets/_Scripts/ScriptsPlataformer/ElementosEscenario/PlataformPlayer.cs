using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlatformPlayer : MonoBehaviour
{
    private CharacterController controller;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            RaycastHit hit;

            // Raycast simple hacia abajo (más estable que SphereCast aquí)
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                if (hit.collider.CompareTag("Plataform"))
                {
                    if (currentPlatform != hit.collider.transform)
                    {
                        currentPlatform = hit.collider.transform;
                        lastPlatformPosition = currentPlatform.position;
                    }

                    Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;

                    if (platformDelta != Vector3.zero)
                    {
                        controller.Move(platformDelta);
                    }

                    lastPlatformPosition = currentPlatform.position;
                }
                else
                {
                    currentPlatform = null;
                }
            }
        }
        else
        {
            currentPlatform = null;
        }
    }
}