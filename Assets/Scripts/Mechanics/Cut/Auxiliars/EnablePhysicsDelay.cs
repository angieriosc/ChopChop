using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnablePhysicsDelay : MonoBehaviour
{
    [Header("Configuración de Retraso")]
    [Tooltip("Tiempo en segundos que esperará el script antes de activar la física.")]
    [SerializeField] private float delayTime = 0.5f;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.isKinematic = true;
        }

        StartCoroutine(EnablePhysicsCo());
    }

    private IEnumerator EnablePhysicsCo()
    {
        yield return new WaitForSeconds(delayTime);

        if (_rb != null)
        {
            _rb.isKinematic = false; 
        }
    }
}