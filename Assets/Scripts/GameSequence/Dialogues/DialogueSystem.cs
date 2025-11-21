using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public float textSpeed = 0.04f;
    private bool isShowing;

    public void ShowDialogue(string message)
    {
        if (isShowing) return;

        dialoguePanel.SetActive(true);
        StartCoroutine(TypeText(message));
        isShowing = true;
    }

    IEnumerator TypeText(string msg)
    {
        dialogueText.text = "";
        foreach (char c in msg)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(1.5f);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        isShowing = false;
    }
}
