using UnityEngine;

public class DetectMouseClick : MonoBehaviour
{
    void Update()
    {
        // Detecta el clic del botón izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            // Crea un rayo desde la cámara a través del cursor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Realiza un raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Si el rayo golpea un objeto con un collider
                Debug.Log("Has hecho clic en el objeto: " + hit.collider.name);
            }
        }
    }
}