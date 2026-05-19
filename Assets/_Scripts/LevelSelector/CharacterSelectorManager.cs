using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class CharacterSelectorManager : MonoBehaviour
{
    [Header("Referencias 3D")]
    public Transform puntoAparicion;
    public GameObject[] personajesPrefabs;

    [Header("Referencias de Estado")]
    public Camera camaraPrincipal;
    public Transform refCamaraMenu;
    public Transform refCamaraJuego;
    public MonoBehaviour scriptSeguimientoCamara;
    public GameObject UI_MenuSelector;
    public GameObject carpasUtileria;

    [Header("Jugador Real")]
    public GameObject jugadorReal;

    [Header("Rotación de Modelo")]
    public float velocidadRotacionRaton = 0.5f;
    public float velocidadRotacionMando = 150f;

    private GameObject personajeActual;
    private int indiceActual = 0;
    private bool enMenu = true;
    private GameObject ultimoBotonSeleccionado;

    void Start()
    {
        enMenu = true;
        if (scriptSeguimientoCamara != null) scriptSeguimientoCamara.enabled = false;
        UI_MenuSelector.SetActive(true);
        if (carpasUtileria != null) carpasUtileria.SetActive(true);
        if (jugadorReal != null) jugadorReal.SetActive(false);
        if (personajesPrefabs.Length > 0) MostrarPersonaje(indiceActual);
    }

    void Update()
    {
        // 1. Lógica de Rotación
        if (enMenu && personajeActual != null)
        {
            float rotacionX = 0f;
            if (Gamepad.current != null)
                rotacionX = Gamepad.current.leftStick.x.ReadValue() * velocidadRotacionMando * Time.deltaTime;

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                rotacionX = Mouse.current.delta.x.ReadValue() * velocidadRotacionRaton;

            if (rotacionX != 0f) personajeActual.transform.Rotate(Vector3.up, -rotacionX, Space.World);
        }

        // 2. Gestión de Foco para Mando
        if (enMenu && Gamepad.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (ultimoBotonSeleccionado != null) EventSystem.current.SetSelectedGameObject(ultimoBotonSeleccionado);
            }
            else
            {
                ultimoBotonSeleccionado = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

    void LateUpdate()
    {
        if (enMenu)
        {
            if (scriptSeguimientoCamara != null && scriptSeguimientoCamara.enabled) scriptSeguimientoCamara.enabled = false;
            if (camaraPrincipal != null && refCamaraMenu != null)
            {
                camaraPrincipal.transform.position = refCamaraMenu.position;
                camaraPrincipal.transform.rotation = refCamaraMenu.rotation;
            }
        }
    }

    public void SiguientePersonaje()
    {
        if (personajesPrefabs.Length == 0) return;
        indiceActual = (indiceActual + 1) % personajesPrefabs.Length;
        MostrarPersonaje(indiceActual);
    }

    public void PersonajeAnterior()
    {
        if (personajesPrefabs.Length == 0) return;
        indiceActual--;
        if (indiceActual < 0) indiceActual = personajesPrefabs.Length - 1;
        MostrarPersonaje(indiceActual);
    }

    private void MostrarPersonaje(int indice)
    {
        if (personajeActual != null) Destroy(personajeActual);
        personajeActual = Instantiate(personajesPrefabs[indice], puntoAparicion.position, puntoAparicion.rotation);
        personajeActual.transform.SetParent(puntoAparicion);
    }

    public void IniciarJuego()
    {
        enMenu = false;
        UI_MenuSelector.SetActive(false);
        if (carpasUtileria != null) carpasUtileria.SetActive(false);
        if (personajeActual != null) Destroy(personajeActual);
        if (jugadorReal != null)
        {
            jugadorReal.SetActive(true);
            jugadorReal.transform.position = puntoAparicion.position;
            jugadorReal.transform.rotation = puntoAparicion.rotation;
        }
        StartCoroutine(MoverCamaraJuego());
    }

    private IEnumerator MoverCamaraJuego()
    {
        if (scriptSeguimientoCamara != null) scriptSeguimientoCamara.enabled = false;
        float duracion = 1.5f;
        float tiempo = 0f;
        Vector3 posInicial = camaraPrincipal.transform.position;
        Quaternion rotInicial = camaraPrincipal.transform.rotation;
        while (tiempo < duracion)
        {
            camaraPrincipal.transform.position = Vector3.Lerp(posInicial, refCamaraJuego.position, tiempo / duracion);
            camaraPrincipal.transform.rotation = Quaternion.Lerp(rotInicial, refCamaraJuego.rotation, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }
        camaraPrincipal.transform.position = refCamaraJuego.position;
        camaraPrincipal.transform.rotation = refCamaraJuego.rotation;
        if (scriptSeguimientoCamara != null) scriptSeguimientoCamara.enabled = true;
    }
}