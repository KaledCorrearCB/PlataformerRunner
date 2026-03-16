using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(VirtualMouseInput))]
public class ControladorCursorUI : MonoBehaviour
{
    public Image imagenCursor;
    public Sprite spriteMando;
    public Sprite spriteRaton;

    private VirtualMouseInput virtualMouse;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool usandoMando = false;

    void Awake()
    {
        virtualMouse = GetComponent<VirtualMouseInput>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Desaparecer el cursor de Windows de inmediato
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    void Start() => CambiarModo(false); // Iniciar siempre en modo ratón

    void Update()
    {
        // 1. Detectar si el ratón se mueve físicamente
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.magnitude > 0.1f && usandoMando)
            {
                CambiarModo(false);
            }
        }

        // 2. Detectar si el stick del mando se mueve
        if (Gamepad.current != null)
        {
            Vector2 stickDelta = Gamepad.current.leftStick.ReadValue();
            if (stickDelta.magnitude > 0.2f && !usandoMando)
            {
                CambiarModo(true);
            }
        }

        // 3. SEGUIMIENTO (Solo si NO es mando, para evitar el 'rebote')
        if (!usandoMando && Mouse.current != null)
        {
            rectTransform.position = Mouse.current.position.ReadValue();
        }

        ClampearPosicion();
    }

    private void ClampearPosicion()
    {
        Vector3 pos = rectTransform.localPosition;
        Vector2 size = canvas.GetComponent<RectTransform>().sizeDelta / 2;
        // Mantiene el cursor siempre dentro de la pantalla
        pos.x = Mathf.Clamp(pos.x, -size.x, size.x);
        pos.y = Mathf.Clamp(pos.y, -size.y, size.y);
        rectTransform.localPosition = pos;
    }

    private void CambiarModo(bool mando)
    {
        usandoMando = mando;
        if (imagenCursor != null)
        {
            imagenCursor.sprite = usandoMando ? spriteMando : spriteRaton;
            rectTransform.localScale = usandoMando ? Vector3.one : new Vector3(0.5f, 0.5f, 1f);
        }
        // DESACTIVAMOS el componente VirtualMouse si usamos el ratón físico para evitar conflictos
        virtualMouse.enabled = mando;
        Cursor.visible = false;
    }
}