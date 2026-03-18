using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(VirtualMouseInput))]
public class ControladorCursorUI : MonoBehaviour
{
    public Image imagenCursor;

    private VirtualMouseInput virtualMouse;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool usandoMando = false;

    void Awake()
    {
        virtualMouse = GetComponent<VirtualMouseInput>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Aseguramos que el mouse del sistema sea visible y libre inicialmente
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // Detección de movimiento del ratón físico
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.magnitude > 0.1f && usandoMando)
            {
                CambiarModo(false);
            }
        }

        // Detección de movimiento del stick del mando
        if (Gamepad.current != null)
        {
            Vector2 stickDelta = Gamepad.current.leftStick.ReadValue();
            if (stickDelta.magnitude > 0.2f && !usandoMando)
            {
                CambiarModo(true);
            }
        }

        // Sincronización: Si no usamos mando, el cursor virtual sigue al ratón real
        if (!usandoMando && Mouse.current != null)
        {
            transform.position = Mouse.current.position.ReadValue();
        }

        ClampearPosicion();
    }

    private void CambiarModo(bool modoMando)
    {
        usandoMando = modoMando;

        if (modoMando)
        {
            Cursor.visible = false;
            // Activamos el input del VirtualMouse solo para el mando
            virtualMouse.enabled = true;
        }
        else
        {
            Cursor.visible = true;
            // Desactivamos el VirtualMouse para que no pelee con el ratón físico
            virtualMouse.enabled = false;
        }
    }

    private void ClampearPosicion()
    {
        if (canvas == null) return;

        Vector3 pos = rectTransform.localPosition;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 limit = canvasRect.sizeDelta / 2f;

        pos.x = Mathf.Clamp(pos.x, -limit.x, limit.x);
        pos.y = Mathf.Clamp(pos.y, -limit.y, limit.y);
        rectTransform.localPosition = pos;
    }
}