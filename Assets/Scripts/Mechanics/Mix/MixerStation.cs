using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa una estación de mezcla que procesa MixableBowl.
/// Controla UI, animación de cuchilla y estados de mezcla.
/// </summary>
public class MixerStation : MonoBehaviour
{
    [Header("Mixer Configuration")]
    [SerializeField] private float mixingTime = 10f;
    [SerializeField] private float displayReadyTime = 3f;

    [Header("UI References")]
    [SerializeField] private GameObject mixerCanvas;
    [SerializeField] private Image progressBar;
    [SerializeField] private Image backgroundBar;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Bar Colors")]
    [SerializeField] private Color mixingColor = Color.cyan;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Placement Point")]
    [SerializeField] private Transform bowlPoint;

    [Header("Mixer Animation")]
    [SerializeField] private Transform mixerBlade;
    [SerializeField] private float rotationSpeed = 360f;

    private GameObject currentBowl;
    private MixableBowl mixableBowlData;
    private float currentTime;
    private float readyDisplayTimeCounter;
    private bool isMixing;
    private bool showingReady;
    private MixingStage currentStage;
    private Vector3 originalPosition;

    private enum MixingStage { Unmixed, Mixing, Ready }

    private void Start()
    {
        Debug.Log("🌀 MixerStation initialized");

        if (!CheckUIAssignments()) return;

        mixerCanvas.SetActive(false);
    }

    private void Update()
    {
        if (isMixing) UpdateMixing();
        if (showingReady) UpdateReadyDisplay();
        AnimateBlade();
    }

    #region Public Methods

    /// <summary>
    /// Coloca un bowl en la estación y comienza a mezclar.
    /// </summary>
    public bool PutBowlIn(GameObject bowl)
    {
        Debug.Log($"🌀 Trying to put '{bowl.name}' in mixer...");

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
    /// Retira el bowl de la estación y finaliza mezcla si corresponde.
    /// </summary>
    public GameObject TakeBowlOut()
    {
        if (currentBowl == null) return null;

        bool wasReady = (currentStage == MixingStage.Ready);
        StopMixing();

        GameObject bowl = currentBowl;
        bowl.transform.SetParent(null);
        bowl.transform.position = originalPosition;

        if (wasReady) mixableBowlData.MarkAsMixed();
        else Debug.Log("⚠️ Bowl retirado antes de terminar mezcla");

        EnablePhysics(bowl);
        mixerCanvas?.SetActive(false);
        ResetMixerState();

        
        DialogueSequenceRunner sequenceManager = FindFirstObjectByType<DialogueSequenceRunner>();
        if (sequenceManager.stepIndex==3)
        {
            //Continuar cinematica
            StartCoroutine(sequenceManager.ContinueSequence());
        }


        return bowl;
    }

    /// <summary>
    /// Indica si la estación está disponible para un bowl.
    /// </summary>
    public bool IsAvailable() => currentBowl == null;

    #endregion

    #region Private Methods

    private bool CheckUIAssignments()
    {
        bool allAssigned = true;
        if (mixerCanvas == null) { Debug.LogError("MixerCanvas NOT assigned!"); allAssigned = false; }
        if (progressBar == null) { Debug.LogError("ProgressBar NOT assigned!"); allAssigned = false; }
        if (stateText == null) { Debug.LogError("StateText NOT assigned!"); allAssigned = false; }
        if (bowlPoint == null) { Debug.LogError("BowlPoint NOT assigned!"); allAssigned = false; }
        return allAssigned;
    }

    private MixableBowl GetMixableBowl(GameObject bowl)
    {
        MixableBowl mixable = bowl.GetComponent<MixableBowl>();
        if (mixable == null && bowl.transform.parent != null)
            mixable = bowl.transform.parent.GetComponent<MixableBowl>();

        if (mixable == null) Debug.LogWarning($"❌ '{bowl.name}' doesn't have MixableBowl component");
        return mixable;
    }

    private bool VerifyMixable(MixableBowl mixable)
    {
        InteractableObject interactable = mixable.GetComponent<InteractableObject>();
        if (interactable == null || !interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            Debug.LogWarning($"❌ '{mixable.name}' is not mixable");
            return false;
        }
        return true;
    }

    private void SetupBowlInMixer(MixableBowl mixable)
    {
        currentBowl = mixable.gameObject;
        mixableBowlData = mixable;
        originalPosition = currentBowl.transform.position;

        Rigidbody rb = currentBowl.GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
                            rb.useGravity = false; 
                            rb.isKinematic = true; }

        currentBowl.transform.SetParent(bowlPoint);
        currentBowl.transform.localPosition = Vector3.zero;
        currentBowl.transform.localRotation = Quaternion.identity;

        mixerCanvas?.SetActive(true);
        progressBar.fillAmount = 0f;
    }

    private void StartMixing()
    {
        isMixing = true;
        currentTime = 0f;
        currentStage = MixingStage.Mixing;
        showingReady = false;
        readyDisplayTimeCounter = 0f;
    }

    private void StopMixing() => isMixing = false;

    private void ResetMixerState()
    {
        currentBowl = null;
        mixableBowlData = null;
        currentTime = 0f;
        currentStage = MixingStage.Unmixed;
        showingReady = false;
        readyDisplayTimeCounter = 0f;
    }

    private void EnablePhysics(GameObject bowl)
    {
        Rigidbody rb = bowl.GetComponent<Rigidbody>();
        if (rb != null) { rb.angularVelocity = Vector3.zero; rb.useGravity = true; rb.isKinematic = false; }
    }

    private void UpdateMixing()
    {
        currentTime += Time.deltaTime;
        float progress = Mathf.Clamp01(currentTime / mixingTime);

        if (currentTime < mixingTime)
        {
            currentStage = MixingStage.Mixing;
            progressBar.fillAmount = progress;
            progressBar.color = mixingColor;
            stateText.text = $"Mixing... {Mathf.CeilToInt(mixingTime - currentTime)}s";
        }
        else
        {
            if (currentStage == MixingStage.Mixing)
            {
                currentStage = MixingStage.Ready;
                mixableBowlData.MarkAsMixed();
                isMixing = false;
                showingReady = true;
                readyDisplayTimeCounter = 0f;
                Debug.Log("✅ Mixing complete! Take out the bowl.");
            }

            progressBar.fillAmount = 1f;
            progressBar.color = readyColor;
            stateText.text = "READY! Take out bowl";
        }
    }

    private void UpdateReadyDisplay()
    {
        readyDisplayTimeCounter += Time.deltaTime;
        if (readyDisplayTimeCounter >= displayReadyTime)
        {
            mixerCanvas?.SetActive(false);
            showingReady = false;
        }
    }

    private void AnimateBlade()
    {
        if (mixerBlade != null && currentStage == MixingStage.Mixing)
            mixerBlade.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 3f);

        if (bowlPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bowlPoint.position, Vector3.one * 0.5f);
        }
    }

    #endregion
}
