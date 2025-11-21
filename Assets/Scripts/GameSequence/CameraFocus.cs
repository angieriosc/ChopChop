using System.Collections;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    [Header("Referencias")]
    public CameraController cameraController; // Tu sistema actual
    public Camera cinematicCamera;            // Cámara especial para focus
    public float focusSpeed = 3f;

    private Camera previousCamera;
    private Vector3 originalPos;
    private Quaternion originalRot;

    /// <summary>
    /// Enfoca al objetivo moviendo la cámara cinemática y activándola.
    /// </summary>
    public void FocusOn(Transform target, Vector3 offset)
    {
        StopAllCoroutines();
        StartCoroutine(FocusRoutine(target, offset));
    }

    /// <summary>
    /// Regresa de forma suave a la cámara anterior.
    /// </summary>
    public void ReturnToPrevious()
    {
        StopAllCoroutines();
        StartCoroutine(ReturnRoutine());
    }

    IEnumerator FocusRoutine(Transform target, Vector3 offset)
    {
        if (cameraController == null || cinematicCamera == null)
            yield break;

        // Guardar cámara actual
        previousCamera = cameraController.GetActiveCamera();

        // Activar cámara cinemática
        cameraController.ActivateCamera(cinematicCamera);

        // Guardar transform original
        originalPos = cinematicCamera.transform.position;
        originalRot = cinematicCamera.transform.rotation;

        Vector3 finalPos = target.position + offset;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * focusSpeed;

            cinematicCamera.transform.position = 
                Vector3.Lerp(originalPos, finalPos, t);

            cinematicCamera.transform.LookAt(target);

            yield return null;
        }
    }

    IEnumerator ReturnRoutine()
    {
        if (previousCamera == null)
            yield break;

        Vector3 startPos = cinematicCamera.transform.position;
        Quaternion startRot = cinematicCamera.transform.rotation;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * focusSpeed;

            cinematicCamera.transform.position =
                Vector3.Lerp(startPos, originalPos, t);

            cinematicCamera.transform.rotation =
                Quaternion.Lerp(startRot, originalRot, t);

            yield return null;
        }

        cameraController.ActivateCamera(previousCamera);
    }
}
