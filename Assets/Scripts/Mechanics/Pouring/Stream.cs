using System.Collections;
using NUnit.Framework;
using UnityEngine;
/*
 * Stream.cs
 * 
 * Original code by Andrew (MIT License, 2024)
 * Modified by Kevin Josué Martínez Leyva (2025)
 * 
 * Licensed under the MIT License.
 */

public class Stream : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private ParticleSystem splashParticle = null;

    private Coroutine pourRoutine = null; 
    private Vector3 targetPosition = Vector3.zero;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticle = GetComponentInChildren<ParticleSystem>(); 
    }

    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }

    public void BeginStream()
    {
        StartCoroutine(UpdateParticle());
        pourRoutine = StartCoroutine(BeginStreamCoroutine());
    }

    private IEnumerator BeginStreamCoroutine()
    {
        while (gameObject.activeSelf)
        {
            targetPosition = FIndEndPoint();

            MoveToPosition(0, transform.position);
            AnimateToPosition(1, targetPosition);

            yield return null;
        }
    }

    public void End()
    {
        StopCoroutine(pourRoutine);
        pourRoutine = StartCoroutine(EndPour());
    }

    public IEnumerator EndPour()
    {
        while (!HasReachedPosition(0, targetPosition))
        {
            AnimateToPosition(0, targetPosition);
            AnimateToPosition(1, targetPosition);

            yield return null;
        }
        Destroy(gameObject);
    }

    private Vector3 FIndEndPoint()
    {
        RaycastHit hit;
        // Dirección local de la botella
        Ray ray = new Ray(transform.position, -transform.up);

        if (Physics.Raycast(ray, out hit, 2.0f))
        {
            return hit.point;
        }

        // Si no golpea nada, proyecta 2 unidades hacia abajo
        return ray.GetPoint(2.0f);
    }

    private void MoveToPosition(int index, Vector3 position)
    {
        lineRenderer.SetPosition(index, position);
    }

    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * 10f);
        lineRenderer.SetPosition(index, newPosition);
    }

    private bool HasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        return currentPosition== targetPosition;
    }

    private IEnumerator UpdateParticle()
    {
        while (gameObject.activeSelf)
        {
            splashParticle.gameObject.transform.position = targetPosition;

            bool isHitting = HasReachedPosition(1, targetPosition);
            splashParticle.gameObject.SetActive(isHitting);

            yield return null;
        }
    }
}
