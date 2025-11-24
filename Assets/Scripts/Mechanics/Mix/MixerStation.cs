using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la lógica de la Estación de Mezcla.
/// Maneja la interacción de machacar botón (Button Mashing), la animación de las aspas,
/// el feedback visual (UI) y la protección de input (Cooldown) al finalizar.
/// </summary>
public class MixerStation : MonoBehaviour
{
    [Header("Manual Mixing Settings")]
    [Tooltip("Tecla requerida para interactuar con la mezcla.")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Tooltip("Cantidad de veces que el jugador debe presionar la tecla para completar la mezcla.")]
    [SerializeField] private int tapsRequired = 30;
    
    [Tooltip("Tiempo que permanece el mensaje de 'Ready' antes de ocultar la UI.")]
    [SerializeField] private float displayReadyTime = 3f;

    [Header("Interaction Check")]
    [Tooltip("Distancia máxima a la que el jugador puede estar para operar la máquina.")]
    [SerializeField] private float interactionRange = 3f;

    [Header("UI References")]
    [SerializeField] private GameObject mixerCanvas;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Bar Colors")]
    [SerializeField] private Color mixingColor = Color.cyan;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Placement Point")]
    [SerializeField] private Transform bowlPoint;

    [Header("Mixer Animation")]
    [SerializeField] private Transform mixerBladeTransform;
    [SerializeField] private GameObject mixerBladeVisual;
    [SerializeField] private float bladeSpeed = 1000f;

    // --- VARIABLES INTERNAS ---
    private Transform playerTransform;
    private float currentBladeVelocity = 0f;
    private float currentSpinAngle = 0f;

    private GameObject currentBowl;
    private MixableBowl mixableBowlData;
    private int currentTaps;
    private float readyDisplayTimeCounter;
    private bool isMixing;
    private bool showingReady;
    private MixingStage currentStage;
    private Vector3 originalPosition;
    private enum MixingStage { Unmixed, Mixing, Ready }

    /// <summary>
    /// Indica si la estación está actualmente en proceso de mezcla.
    /// Útil para bloquear interacciones externas (como recoger el bowl antes de tiempo).
    /// </summary>
    public bool IsMixingInProgress => isMixing;

    private void Start()
    {
        if (!CheckUIAssignments()) return;

        /// Buscar referencia al jugador
        PlayerPickup player = FindFirstObjectByType<PlayerPickup>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("MixerStation: No se encontró PlayerPickup en la escena. La validación de rango podría fallar.");
        }

        mixerCanvas.SetActive(false);
        if (mixerBladeVisual != null) mixerBladeVisual.SetActive(false);
    }

    private void Update()
    {
        if (isMixing) HandleManualMixing();
        if (showingReady) UpdateReadyDisplay();

        AnimateBlade();
    }

    #region Public Methods

    /// <summary>
    /// Intenta colocar un bowl en la estación.
    /// Valida si el objeto es mezclable, si tiene ingredientes correctos y si la estación está libre.
    /// </summary>
    /// <param name="bowl">El objeto que el jugador intenta colocar.</param>
    /// <returns>True si el bowl fue aceptado e inició la mezcla.</returns>
    public bool PutBowlIn(GameObject bowl)
    {
        MixableBowl mixable = GetMixableBowl(bowl);
        if (mixable == null) return false;
        if (!VerifyMixable(mixable)) return false;
        if (mixable.IsMixed()) return false;
        if (!mixable.HasCorrectIngredients()) return false;

        SetupBowlInMixer(mixable);
        StartMixing();
        return true;
    }

    /// <summary>
    /// Retira el bowl de la estación, restablece las físicas y marca el producto como mezclado si se completó el proceso.
    /// También gestiona la continuación de secuencias de diálogo si corresponde.
    /// </summary>
    /// <returns>El GameObject del bowl que fue retirado.</returns>
    public GameObject TakeBowlOut()
    {
        if (currentBowl == null) return null;

        bool wasReady = (currentStage == MixingStage.Ready);
        StopMixing();

        GameObject bowl = currentBowl;
        bowl.transform.SetParent(null);
        bowl.transform.position = originalPosition;

        if (wasReady) mixableBowlData.MarkAsMixed();

        EnablePhysics(bowl);
        mixerCanvas?.SetActive(false);
        ResetMixerState();

        // Integración con el sistema de diálogos/tutorial
        DialogueSequenceRunner sequenceManager = FindFirstObjectByType<DialogueSequenceRunner>();
        if (sequenceManager != null && sequenceManager.stepIndex == 3)
        {
            StartCoroutine(sequenceManager.ContinueSequence());
        }

        return bowl;
    }

    /// <summary>
    /// Verifica si la estación no tiene ningún bowl asignado actualmente.
    /// </summary>
    public bool IsAvailable() => currentBowl == null;

    #endregion

    #region Private Methods

    /// <summary>
    /// Detecta la entrada del jugador para incrementar el progreso de mezcla e impulsar visualmente la cuchilla.
    /// </summary>
    private void HandleManualMixing()
    {
        // 1. Verificamos distancia antes de aceptar input
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > interactionRange)
            {
                // El jugador está muy lejos, ignoramos el input
                return;
            }
        }

        // 2. Procesamos el input
        if (Input.GetKeyDown(interactKey))
        {
            currentTaps++;
            currentBladeVelocity = bladeSpeed; // Impulso visual
            UpdateProgressUI();

            if (currentTaps >= tapsRequired) FinishMixing();
        }
    }

    /// <summary>
    /// Actualiza la barra de progreso y el texto de estado en la UI.
    /// </summary>
    private void UpdateProgressUI()
    {
        float progress = (float)currentTaps / tapsRequired;
        progressBar.fillAmount = progress;
        progressBar.color = mixingColor;
    }

    /// <summary>
    /// Finaliza el proceso de mezcla, actualiza la UI a estado "Listo" y activa un Cooldown de input.
    /// </summary>
    private void FinishMixing()
    {
        currentStage = MixingStage.Ready;
        mixableBowlData.MarkAsMixed();
        isMixing = false;
        showingReady = true;
        readyDisplayTimeCounter = 0f;

        progressBar.fillAmount = 1f;
        progressBar.color = readyColor;

        InputCooldown.TriggerCooldown(2.0f);
    }

    /// <summary>
    /// Configura el bowl dentro de la estación, ajustando su posición, físicas y referencias
    /// </summary>
    private void SetupBowlInMixer(MixableBowl mixable)
    {
        currentBowl = mixable.gameObject;
        mixableBowlData = mixable;
        originalPosition = currentBowl.transform.position;

        Rigidbody rb = currentBowl.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        currentBowl.transform.SetParent(bowlPoint);
        currentBowl.transform.localPosition = Vector3.zero;
        currentBowl.transform.localRotation = Quaternion.identity;

        mixerCanvas?.SetActive(true);
        if (mixerBladeVisual != null) mixerBladeVisual.SetActive(true);
        progressBar.fillAmount = 0f;
    }

    /// <summary>
    /// Inicia el proceso de mezcla, restableciendo variables internas y actualizando la UI
    /// <summary>
    private void StartMixing()
    {
        isMixing = true;
        currentTaps = 0;
        currentStage = MixingStage.Mixing;
        showingReady = false;
        stateText.text = $"Presiona '{interactKey}' repetidas veces para mezclar";
    }

    /// <summary>
    /// Detiene el proceso de mezcla sin marcar el bowl como mezclado.
    /// </summary>
    private void StopMixing() => isMixing = false;

    /// <summary>
    /// Restablece el estado interno de la estación para prepararse para la próxima mezcla.
    /// </summary>
    private void ResetMixerState()
    {
        currentBowl = null;
        mixableBowlData = null;
        currentTaps = 0;
        currentStage = MixingStage.Unmixed;
        showingReady = false;
        if (mixerBladeVisual != null) mixerBladeVisual.SetActive(false);
    }

    /// <summary>
    /// Actualiza el temporizador para ocultar la UI de "Listo" después de un tiempo definido.
    /// </summary>
    private void UpdateReadyDisplay()
    {
        readyDisplayTimeCounter += Time.deltaTime;
        if (readyDisplayTimeCounter >= displayReadyTime)
        {
            mixerCanvas?.SetActive(false);
            showingReady = false;
        }
    }

    /// <summary>
    /// Aplica rotación visual a las aspas basada en la velocidad actual e inercia.
    /// </summary>
    private void AnimateBlade()
    {
        if (mixerBladeTransform == null) return;

        if (currentBladeVelocity > 0.1f)
        {
            currentSpinAngle += currentBladeVelocity * Time.deltaTime;
            currentSpinAngle %= 360f;

            mixerBladeTransform.localRotation = Quaternion.Euler(0f, currentSpinAngle, -90f);
            currentBladeVelocity = Mathf.Lerp(currentBladeVelocity, 0, Time.deltaTime * 5f);
        }
    }

    private bool CheckUIAssignments()
    {
        if (mixerCanvas == null || progressBar == null || stateText == null || bowlPoint == null)
        {
            Debug.LogError("MixerStation: Faltan referencias de UI en el Inspector.");
            return false;
        }
        return true;
    }

    private MixableBowl GetMixableBowl(GameObject bowl)
    {
        MixableBowl mixable = bowl.GetComponent<MixableBowl>();
        if (mixable == null && bowl.transform.parent != null)
            mixable = bowl.transform.parent.GetComponent<MixableBowl>();
        return mixable;
    }

    private bool VerifyMixable(MixableBowl mixable)
    {
        InteractableObject interactable = mixable.GetComponent<InteractableObject>();
        return interactable != null && interactable.HasCapability(ObjectCapabilities.Mixable);
    }

    private void EnablePhysics(GameObject bowl)
    {
        Rigidbody rb = bowl.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    #endregion
}