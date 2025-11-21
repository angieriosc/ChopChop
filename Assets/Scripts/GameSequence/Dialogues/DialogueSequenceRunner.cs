using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueSequenceRunner : MonoBehaviour
{
    public List<Transform> targets;
    public List<Transform> pointerPositions;

    [TextArea] public List<string> dialogues;

    public CubeMover mover;
    public DialogueSystem dialogue;
    public CameraFocus cameraFocus;

    public int stepIndex = 0;
    private bool waitingForInput = false;

    public GameObject continuePanel;

    public GameObject pointer;

    private GameObject currentPointer;


    void Start()
    {
        if (continuePanel != null)
            continuePanel.SetActive(false);
        RunStep(stepIndex);
        
    }

    void RunStep(int index)
    {
        if (index >= targets.Count) return;

        Transform target = targets[index];
        string text = dialogues[index];

        mover.target = target.gameObject;

        mover.OnReachedTarget = () =>
        {
            // Cámara cinemática
            cameraFocus.FocusOn(mover.transform, new Vector3(4f, 4.5f, 0f));

            // Mostrar diálogo
            dialogue.ShowDialogue(text);

            // Activar panel de "Presiona E"
            if (continuePanel != null)
                continuePanel.SetActive(true);

            DialogueLock.IsLocked = true;

            waitingForInput = true;

        };

        mover.MoveTo();
    }


    void Update()
    {
        if (waitingForInput && Input.GetKeyDown(KeyCode.E))
        {
            waitingForInput = false;

            if (continuePanel != null)
                continuePanel.SetActive(false); // Apagar panel
            ManageInput();
        }
    }

    void CreatePointer(int index)
    {
        if (pointer != null && pointerPositions != null && index < pointerPositions.Count)
        {
            // Si existe uno previo, destruirlo
            if (currentPointer != null)
                Destroy(currentPointer);

            // Crear uno nuevo
            currentPointer = Instantiate(
                pointer,
                pointerPositions[index].position,
                pointerPositions[index].rotation
            );
        }
    }

    public void DestroyPointer()
    {
        if (currentPointer != null)
            Destroy(currentPointer);
    }

    void ManageInput()
    {
        CreatePointer(stepIndex);

        dialogue.HideDialogue();

        // Regresar la cámara
        cameraFocus.ReturnToPrevious();

        InputCooldown.BlockNextE = true;
        DialogueLock.IsLocked = false;
    }

    public IEnumerator ContinueSequence()
    {
        stepIndex++;
        RunStep(stepIndex);

        yield return null;
    }
}
