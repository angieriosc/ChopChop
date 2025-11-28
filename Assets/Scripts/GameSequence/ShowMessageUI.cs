using UnityEngine;
using TMPro;
using System.Collections;

public class ShowMessageUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;          // Panel con imagen
    public TMP_Text messageText;      // Texto dentro del panel

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowMessageForSeconds(string message, float seconds = 8f)
    {
        StartCoroutine(ShowRoutine(message, seconds));
    }

    public IEnumerator ShowRoutine(string message, float seconds)
    {
        panel.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(seconds);

        panel.SetActive(false);
    }
}
