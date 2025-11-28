using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PatienceManager : MonoBehaviour
{
    public static PatienceManager Instance { get; private set; }

    [Header("ARCHIVO DE DATOS")]
    [Tooltip("Arrastra aquí tu archivo .asset de configuración")]
    public PatienceData data; 

    // Variables internas
    private float currentPatience;
    private bool isGameOver = false;

    [Header("Referencias UI")]
    public Slider slider;
    public Image fillImage;
    public Animator angryIcon;
    public Animator happyIcon;

    [Header("Umbrales Visuales")]
    [Range(0f, 1f)] public float hurryUpThreshold = 0.15f;
    [Range(0f, 1f)] public float angryThreshold = 0.25f;
    [Range(0f, 1f)] public float happyThreshold = 0.8f;
    private bool isHurryUpActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Cargar paciencia inicial desde el archivo de datos
        if (data != null) currentPatience = data.maxPatience;
        else currentPatience = 100f;
    }

    private void Start()
    {
        if (slider != null) { slider.minValue = 0f; slider.maxValue = 1f; UpdateUI(); }
        
        // Bloqueo del ratón (Input)
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; cg.interactable = false;
    }

    private void Update()
    {
        // Desgaste Pasivo leyendo desde 'data'
        if (!isGameOver && currentPatience > 0 && data != null && data.passiveDecayRate > 0)
        {
            currentPatience -= data.passiveDecayRate * Time.deltaTime;
            CheckGameOver();
            UpdateUI();
        }
    }

    // FUNCIÓN PÚBLICA PARA RESTAR PUNTOS
    public void ApplyPenalty(PenaltyType type)
    {
        if (isGameOver || data == null) return;

        float damage = data.GetDamageAmount(type);
        Debug.Log($"<color=orange>Castigo:</color> {type} (-{damage})");

        currentPatience -= damage;
        CheckGameOver();
        UpdateUI();
    }

    private void CheckGameOver()
    {
        if (currentPatience <= 0)
        {
            currentPatience = 0;
            if (!isGameOver)
            {
                isGameOver = true;
                Debug.Log("¡GAME OVER!");
            }
        }
    }

    private void UpdateUI()
    {
        if (slider == null || data == null) return;

        float normalizedValue = currentPatience / data.maxPatience;
        slider.value = normalizedValue;

        if (fillImage != null)
        {
            if (normalizedValue < 0.5f) fillImage.color = Color.Lerp(Color.red, Color.yellow, normalizedValue / 0.5f);
            else fillImage.color = Color.Lerp(Color.yellow, Color.green, (normalizedValue - 0.5f) / 0.5f);
        }

        UpdateIcons(normalizedValue);
    }

    private void UpdateIcons(float value)
    {
        if (angryIcon == null || happyIcon == null) return;

        if (value <= hurryUpThreshold) {
             if (!isHurryUpActive) { angryIcon.SetTrigger("HurryUp"); isHurryUpActive = true; }
             angryIcon.speed = 1.5f; happyIcon.speed = 0f;
        } else if (value <= angryThreshold) {
             angryIcon.speed = 1f; happyIcon.speed = 0f; isHurryUpActive = false;
        } else if (value >= happyThreshold) {
             happyIcon.speed = 1f; angryIcon.speed = 0f; isHurryUpActive = false;
        } else {
             angryIcon.speed = 0f; happyIcon.speed = 0f; isHurryUpActive = false;
        }
    }
}