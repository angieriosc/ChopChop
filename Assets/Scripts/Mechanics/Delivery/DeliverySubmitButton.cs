using UnityEngine;
using System.Collections;


public class DeliverySubmitButton : MonoBehaviour
{
    [SerializeField] private PizzaDeliveryManager deliveryManager;
    [SerializeField] private CustomerManager customerManager;   
    [SerializeField] private GameObject Correct;
    [SerializeField] private GameObject Incorrect;
    [SerializeField] private GameObject DeliveryCanvas;
    
    [Header("Audio")]
    [SerializeField] private AudioSource correctAudio;
    [SerializeField] private AudioSource incorrectAudio;

    [Header("Camara")]
    [SerializeField] private Camera stationCamera;
    [SerializeField] private Camera playerCamera;


    public void OnEntregaPressed()
    {
        if (deliveryManager == null)
        {
            Debug.LogError("[Entrega] DeliveryManager no asignado.");
            return;
        }

        // Validar ronda actual
        if (deliveryManager.ValidateCurrentRound(out string msg))
        {
            ShowCorrectFor5Seconds();
            deliveryManager.AdvanceRound();
            if (customerManager != null)
            {
                customerManager.OnOrderDelivered();
            }

        }
        else
        {
            ShowIncorrectFor5Seconds();
            deliveryManager.AdvanceRound();
            if (customerManager != null)
            {
                customerManager.OnOrderDelivered();
            }
            
        }
    }
    public void ShowCorrectFor5Seconds()
    {
        StartCoroutine(ShowCorrectRoutine());
    }

    private IEnumerator ShowCorrectRoutine()
    {
        Correct.SetActive(true);    
        if (correctAudio != null)
        {
            correctAudio.Play();
        }
        yield return new WaitForSeconds(3.5f);  
        Correct.SetActive(false);   
        yield return  new WaitForSeconds(0.5f);
        if (DeliveryCanvas != null)
        {
            DeliveryCanvas.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            stationCamera.gameObject.SetActive(false);
        }
        DeliveryLock.IsLocked = false;
    }
    public void ShowIncorrectFor5Seconds()
    {
        StartCoroutine(ShowIncorrectRoutine());
    }
    private IEnumerator ShowIncorrectRoutine()
    {
        Incorrect.SetActive(true);    // Activar
        if (incorrectAudio != null)
        {
            incorrectAudio.Play();
        }
        yield return new WaitForSeconds(3.5f);  // Esperar 5 segundos
        Incorrect.SetActive(false);   // Desactivar
        yield return  new WaitForSeconds(0.5f);
        if (DeliveryCanvas != null)
        {
            DeliveryCanvas.SetActive(false); // Reabrir canvas de entrega
            playerCamera.gameObject.SetActive(true);
            stationCamera.gameObject.SetActive(false);
        }
        DialogueSequenceRunner sequenceManager = FindFirstObjectByType<DialogueSequenceRunner>();
        if (sequenceManager.stepIndex==8)
        {
            //Continuar cinematica
            StartCoroutine(sequenceManager.ContinueSequence());
        }
        DeliveryLock.IsLocked = false;
    }
}
