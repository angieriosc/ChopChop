using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueSequenceRunner : MonoBehaviour
{
    public List<Transform> targets;
    [TextArea] public List<string> dialogues;

    public CubeMover mover;
    public DialogueSystem dialogue;
    public CameraFocus cameraFocus;

    private int stepIndex = 0;

    void Start()
    {
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
            // Enfocar al cubo con la cámara cinemática
            cameraFocus.FocusOn(mover.transform, new Vector3(4f, 4.5f, 0f));

            // Mostrar diálogo
            dialogue.ShowDialogue(text);

            StartCoroutine(ContinueAfterDialogue());
        };

        mover.MoveTo();
    }

    IEnumerator ContinueAfterDialogue()
    {
        // Espera antes del siguiente paso
        yield return new WaitForSeconds(2f);

        // Regresar a la cámara anterior
        cameraFocus.ReturnToPrevious();

        stepIndex++;
        RunStep(stepIndex);
    }
}

