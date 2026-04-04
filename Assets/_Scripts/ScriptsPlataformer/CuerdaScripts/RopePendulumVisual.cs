using UnityEngine;

public class RopePendulumVisual : MonoBehaviour
{
    [Header("Referencias")]
    public Transform anchorPoint;
    public Transform ropeModel;
    public GrapplingRope grapplingRope;

    private Quaternion restRotation;

    void Awake()
    {
        if (ropeModel != null)
            restRotation = ropeModel.rotation;
    }

    void LateUpdate()
    {
        if (!grapplingRope.IsSwinging) return;

        Vector3 directionToPlayer = grapplingRope.PlayerPosition - anchorPoint.position;

        // Si el punto naranja ya está arriba:
        // Probamos con '-direction' porque la cuerda debe colgar HACIA el jugador
        ropeModel.up = -directionToPlayer.normalized;
    }

}