using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    public SimpleCinematic cinematic;
    public GameObject objectToSpawn1;
    public GameObject objectToSpawn2;
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    void Start()
    {
        // Escuchar cuando la cámara termine el recorrido
        cinematic.OnCinematicEnd += HandleCinematicEnd;
    }

    void HandleCinematicEnd()
    {
        // Hacer spawn de objetos
        Instantiate(objectToSpawn1, spawnPoint1.position, spawnPoint1.rotation);
        Instantiate(objectToSpawn2, spawnPoint2.position, spawnPoint2.rotation);

        // Aquí puedes iniciar diálogos también:
        // DialogueSystem.Instance.StartDialogue("introCutscene");

        // O activar el control del jugador:
        // PlayerController.Instance.EnableControl();
    }
}
