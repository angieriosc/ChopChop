using UnityEngine;
using System.Collections;


public class DeliverySubmitButton : MonoBehaviour
{
    [SerializeField] private PizzaDeliveryManager deliveryManager;
    [SerializeField] private CustomerManager customerManager;   
    [SerializeField] private GameObject Correct;
    [SerializeField] private GameObject Incorrect;
    
    [Header("Audio")]
    [SerializeField] private AudioSource correctAudio;
    [SerializeField] private AudioSource incorrectAudio;


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

            // Agregar, sonido, sumar puntos.....
        }
        else
        {
            ShowIncorrectFor5Seconds();
            deliveryManager.AdvanceRound();
            if (customerManager != null)
            {
                customerManager.OnOrderDelivered();
            }
            // Agregar sonido error, etc.
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
    }
}
