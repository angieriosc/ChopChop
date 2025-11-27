using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public enum GameOverReason
{
    TimeUp,
    InsufficientFunds
}

/// <summary>
/// Muestra la pantalla de Game Over con animaciones estilo Paper Mario.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private Image backgroundOverlay;
    
    [Header("Curtains Animation")]
    [SerializeField] private RectTransform leftCurtain;
    [SerializeField] private RectTransform rightCurtain;
    [SerializeField] private float curtainAnimationDuration = 1.5f;
    [SerializeField] private AnimationCurve curtainCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Game Over Letters")]
    [SerializeField] private RectTransform[] gameOverLetters;
    [SerializeField] private float letterDropDuration = 0.8f;
    [SerializeField] private float letterDropDelay = 0.1f;
    [SerializeField] private float letterBounceHeight = 200f;
    [SerializeField] private AnimationCurve letterDropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Spotlight")]
    [SerializeField] private Image spotlight;
    [SerializeField] private float spotlightFadeDuration = 1f;
    [SerializeField] private AnimationCurve spotlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Character Models")]
    [SerializeField] private GameObject[] squirrelModels;
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private Camera character3DCamera;
    [SerializeField] private RawImage characterDisplay;
    [SerializeField] private float characterAppearDelay = 1.5f;
    [SerializeField] private string sadAnimationTrigger = "Sad";
    
    // ✅ NUEVO: Array de escalas individuales para cada modelo
    [Header("Individual Character Scales")]
    [Tooltip("Escala individual para cada modelo de ardilla. Debe tener el mismo tamaño que squirrelModels")]
    [SerializeField] private float[] characterScaleMultipliers = new float[] { 3f, 3f };
    
    [SerializeField] private float characterSeparation = 2f;
    [SerializeField] private LayerMask characterLayer;
    
    [Header("Messages")]
    [SerializeField] private string timeUpTitle = "GAME OVER";
    [SerializeField] private string timeUpMessage = "¡Se acabó el tiempo!";
    [SerializeField] private string noMoneyTitle = "GAME OVER";
    [SerializeField] private string noMoneyMessage = "¡Sin dinero suficiente!";
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip curtainSound;
    [SerializeField] private AudioClip letterDropSound;
    [SerializeField] private bool loopGameOverMusic = true;
    [SerializeField] private float gameOverMusicVolume = 0.7f;
    
    [Header("Letter Pulse Animation")]
    [SerializeField] private bool enableLetterPulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float pulseDelay = 0.05f;
    [SerializeField] private float verticalMovement = 5f;
    
    [Header("UI to Hide")]
    [SerializeField] private GameObject[] uiElementsToHide;
    
    public static GameOverScreen Instance { get; private set; }
    
    private AudioSource audioSource;
    private AudioSource gameOverMusicSource;
    private bool[] originalUIStates;
    private GameObject[] spawnedCharacters;
    private Coroutine letterPulseCoroutine;
    private Vector2[] originalLetterPositions;

    public Transform modelRoot; // el punto central de la escena 3D
    public Vector3 cameraOffset = new Vector3(0, 1.2f, -3f);
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // ✅ Validar que el array de escalas coincida con el de modelos
        ValidateScaleArray();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        GameObject musicObj = new GameObject("GameOverMusic");
        musicObj.transform.SetParent(transform);
        gameOverMusicSource = musicObj.AddComponent<AudioSource>();
        gameOverMusicSource.loop = loopGameOverMusic;
        gameOverMusicSource.volume = gameOverMusicVolume;
        gameOverMusicSource.playOnAwake = false;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (uiElementsToHide != null && uiElementsToHide.Length > 0)
        {
            originalUIStates = new bool[uiElementsToHide.Length];
            for (int i = 0; i < uiElementsToHide.Length; i++)
            {
                if (uiElementsToHide[i] != null)
                {
                    originalUIStates[i] = uiElementsToHide[i].activeSelf;
                }
            }
        }
        character3DCamera.aspect = 16f / 9f; // Mantener siempre mismo frustum

        SaveOriginalLetterPositions();
        SetupInitialStates();
    }
    
    void LateUpdate()
    {
        if (character3DCamera == null || modelRoot == null) return;

        character3DCamera.transform.position = 
            modelRoot.position + cameraOffset;

        character3DCamera.transform.LookAt(modelRoot);
    }

    /// <summary>
    /// ✅ Valida y ajusta el array de escalas para que coincida con el número de modelos
    /// </summary>
    private void ValidateScaleArray()
    {
        if (squirrelModels == null || squirrelModels.Length == 0)
        {
            return;
        }
        
        if (characterScaleMultipliers == null || characterScaleMultipliers.Length != squirrelModels.Length)
        {
            Debug.LogWarning($"⚠ El array de escalas ({(characterScaleMultipliers?.Length ?? 0)}) no coincide con el de modelos ({squirrelModels.Length}). Ajustando...");
            
            float[] newScales = new float[squirrelModels.Length];
            for (int i = 0; i < newScales.Length; i++)
            {
                // Si existe un valor previo, usarlo; si no, usar 3f por defecto
                newScales[i] = (characterScaleMultipliers != null && i < characterScaleMultipliers.Length) 
                    ? characterScaleMultipliers[i] 
                    : 3f;
            }
            characterScaleMultipliers = newScales;
            
            Debug.Log($"✅ Array de escalas ajustado a {characterScaleMultipliers.Length} elementos");
        }
    }
    
    private void SaveOriginalLetterPositions()
    {
        if (gameOverLetters != null && gameOverLetters.Length > 0)
        {
            originalLetterPositions = new Vector2[gameOverLetters.Length];
            for (int i = 0; i < gameOverLetters.Length; i++)
            {
                if (gameOverLetters[i] != null)
                {
                    originalLetterPositions[i] = gameOverLetters[i].anchoredPosition;
                }
            }
        }
    }
    
    private void SetupInitialStates()
    {
        if (leftCurtain != null)
        {
            leftCurtain.anchoredPosition = new Vector2(-leftCurtain.rect.width, 0);
        }
        
        if (rightCurtain != null)
        {
            rightCurtain.anchoredPosition = new Vector2(rightCurtain.rect.width, 0);
        }
        
        if (gameOverLetters != null && originalLetterPositions != null)
        {
            for (int i = 0; i < gameOverLetters.Length; i++)
            {
                if (gameOverLetters[i] != null)
                {
                    Vector2 pos = originalLetterPositions[i];
                    pos.y += letterBounceHeight;
                    gameOverLetters[i].anchoredPosition = pos;
                    gameOverLetters[i].gameObject.SetActive(false);
                }
            }
        }
        
        if (spotlight != null)
        {
            Color color = spotlight.color;
            color.a = 0f;
            spotlight.color = color;
            spotlight.gameObject.SetActive(false);
        }
        
        if (backgroundOverlay != null)
        {
            Color color = backgroundOverlay.color;
            color.a = 0f;
            backgroundOverlay.color = color;
            backgroundOverlay.gameObject.SetActive(false);
        }
    }
    
    public void ShowGameOver(GameOverReason reason)
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("❌ GameOverScreen: No se puede mostrar, panel no asignado");
            return;
        }
        
        Debug.Log($"💀 Game Over: {reason}");
        
        StopAllBackgroundMusic();
        Time.timeScale = 0f;
        HideOtherUIElements();
        SetupMessage(reason);
        ResetLetterPositions();
        
        gameOverPanel.SetActive(true);
        StartCoroutine(GameOverAnimationSequence());
    }
    
    private void ResetLetterPositions()
    {
        if (gameOverLetters != null && originalLetterPositions != null)
        {
            for (int i = 0; i < gameOverLetters.Length; i++)
            {
                if (gameOverLetters[i] != null)
                {
                    Vector2 pos = originalLetterPositions[i];
                    pos.y += letterBounceHeight;
                    gameOverLetters[i].anchoredPosition = pos;
                    gameOverLetters[i].gameObject.SetActive(false);
                    gameOverLetters[i].localScale = Vector3.one;
                    gameOverLetters[i].rotation = Quaternion.identity;
                }
            }
        }
    }
    
    private void StopAllBackgroundMusic()
    {
        Debug.Log("🔇 Iniciando detención de audio...");
        
        if (SupermarketAudioManager.Instance != null)
        {
            SupermarketAudioManager.Instance.StopAllMusic();
            Debug.Log("✅ SupermarketAudioManager.StopAllMusic() ejecutado");
        }
        else
        {
            Debug.LogWarning("⚠ SupermarketAudioManager.Instance es null");
        }
        
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);
        Debug.Log($"🔍 Encontrados {allAudioSources.Length} AudioSources en la escena");
        
        int stoppedCount = 0;
        foreach (AudioSource source in allAudioSources)
        {
            if (source != audioSource && source != gameOverMusicSource)
            {
                bool wasPlaying = source.isPlaying;
                source.Stop();
                source.mute = true;
                source.enabled = false;
                
                if (wasPlaying)
                {
                    stoppedCount++;
                    Debug.Log($"🔇 Detenido: {source.gameObject.name} - Clip: {(source.clip != null ? source.clip.name : "null")}");
                }
            }
        }
        
        Debug.Log($"✅ {stoppedCount} AudioSources detenidos");
        
        if (stoppedCount == 0)
        {
            Debug.LogWarning("⚠ No se detuvo ningún audio, usando AudioKiller...");
            AudioKiller.Instance.KillAllAudioExcept(audioSource, gameOverMusicSource);
        }
        
        Debug.Log("🔇 Proceso de detención de audio completado");
    }
    
    private IEnumerator GameOverAnimationSequence()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInOverlay(0.5f, 0.9f));
        }
        
        if (curtainSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(curtainSound);
        }
        
        yield return StartCoroutine(AnimateCurtains());
        yield return new WaitForSecondsRealtime(0.3f);
        yield return StartCoroutine(AnimateGameOverLetters());
        
        if (gameOverSound != null && gameOverMusicSource != null)
        {
            gameOverMusicSource.clip = gameOverSound;
            gameOverMusicSource.Play();
            Debug.Log("🎵 Música de Game Over iniciada");
        }
        
        if (enableLetterPulse && gameOverLetters != null && gameOverLetters.Length > 0)
        {
            letterPulseCoroutine = StartCoroutine(PulseLetters());
        }
        
        yield return StartCoroutine(AnimateSpotlight());
        yield return StartCoroutine(SpawnCharacters());
        yield return new WaitForSecondsRealtime(0.5f);
        
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateText(messageText.GetComponent<RectTransform>(), 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        if (retryButton != null)
        {
            retryButton.SetActive(true);
            yield return StartCoroutine(AnimateText(retryButton.GetComponent<RectTransform>(), 0.3f));
        }
    }
    
    private IEnumerator PulseLetters()
    {
        if (gameOverLetters == null || gameOverLetters.Length == 0 || originalLetterPositions == null) yield break;
        
        Vector3[] originalScales = new Vector3[gameOverLetters.Length];
        
        for (int i = 0; i < gameOverLetters.Length; i++)
        {
            if (gameOverLetters[i] != null)
            {
                originalScales[i] = Vector3.one;
            }
        }
        
        float timeOffset = 0f;
        
        while (true)
        {
            timeOffset += Time.unscaledDeltaTime * pulseSpeed;
            
            for (int i = 0; i < gameOverLetters.Length; i++)
            {
                if (gameOverLetters[i] != null)
                {
                    float wave = Mathf.Sin(timeOffset + (i * pulseDelay * 10f));
                    float scaleMultiplier = 1f + (wave * pulseAmount);
                    
                    gameOverLetters[i].localScale = originalScales[i] * scaleMultiplier;
                    
                    Vector2 pos = originalLetterPositions[i];
                    pos.y = originalLetterPositions[i].y + (wave * verticalMovement);
                    gameOverLetters[i].anchoredPosition = pos;
                }
            }
            
            yield return null;
        }
    }
    
    private IEnumerator AnimateCurtains()
    {
        float elapsed = 0f;
        Vector2 leftStart = leftCurtain != null ? leftCurtain.anchoredPosition : Vector2.zero;
        Vector2 rightStart = rightCurtain != null ? rightCurtain.anchoredPosition : Vector2.zero;
        Vector2 leftEnd = new Vector2(0, 0);
        Vector2 rightEnd = new Vector2(0, 0);
        
        while (elapsed < curtainAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / curtainAnimationDuration;
            float curveValue = curtainCurve.Evaluate(progress);
            
            if (leftCurtain != null)
            {
                leftCurtain.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, curveValue);
            }
            
            if (rightCurtain != null)
            {
                rightCurtain.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, curveValue);
            }
            
            yield return null;
        }
    }
    
    private IEnumerator AnimateGameOverLetters()
    {
        if (gameOverLetters == null || gameOverLetters.Length == 0)
        {
            yield break;
        }
        
        for (int i = 0; i < gameOverLetters.Length; i++)
        {
            if (gameOverLetters[i] != null)
            {
                gameOverLetters[i].gameObject.SetActive(true);
                StartCoroutine(DropLetter(gameOverLetters[i], i));
                
                if (letterDropSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(letterDropSound, 0.5f);
                }
                
                yield return new WaitForSecondsRealtime(letterDropDelay);
            }
        }
        
        yield return new WaitForSecondsRealtime(letterDropDuration);
    }
    
    private IEnumerator DropLetter(RectTransform letter, int index)
    {
        float elapsed = 0f;
        Vector2 startPos = letter.anchoredPosition;
        Vector2 targetPos = originalLetterPositions[index];
        
        while (elapsed < letterDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / letterDropDuration;
            float curveValue = letterDropCurve.Evaluate(progress);
            
            letter.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            letter.rotation = Quaternion.Euler(0, 0, Mathf.Sin(progress * Mathf.PI * 2) * 10f);
            
            yield return null;
        }
        
        letter.anchoredPosition = targetPos;
        letter.rotation = Quaternion.identity;
    }
    
    private IEnumerator AnimateSpotlight()
    {
        if (spotlight == null) yield break;
        
        spotlight.gameObject.SetActive(true);
        float elapsed = 0f;
        Color startColor = spotlight.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = 1f;
        
        spotlight.color = startColor;
        
        while (elapsed < spotlightFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / spotlightFadeDuration;
            float curveValue = spotlightCurve.Evaluate(progress);
            
            spotlight.color = Color.Lerp(startColor, endColor, curveValue);
            
            yield return null;
        }
        
        spotlight.color = endColor;
    }
    
    /// <summary>
    /// ✅ MEJORADO: Spawn de personajes con escalas individuales
    /// </summary>
    private IEnumerator SpawnCharacters()
    {
        if (squirrelModels == null || squirrelModels.Length == 0 || characterSpawnPoint == null)
        {
            Debug.LogWarning("⚠ No hay modelos de ardilla o spawn point asignado");
            yield break;
        }
        
        if (character3DCamera != null)
        {
            character3DCamera.gameObject.SetActive(true);
            Debug.Log("📷 Cámara 3D de personajes activada");
        }
        
        yield return new WaitForSecondsRealtime(characterAppearDelay);
        
        spawnedCharacters = new GameObject[squirrelModels.Length];
        
        for (int i = 0; i < squirrelModels.Length; i++)
        {
            if (squirrelModels[i] != null)
            {
                Vector3 spawnPos = characterSpawnPoint.position;
                
                if (squirrelModels.Length > 1)
                {
                    float offset = (i - (squirrelModels.Length - 1) / 2f) * characterSeparation;
                    spawnPos += Vector3.right * offset;
                }
                
                GameObject character = Instantiate(squirrelModels[i], spawnPos, characterSpawnPoint.rotation);
                spawnedCharacters[i] = character;
                
                SetLayerRecursively(character, LayerMask.NameToLayer("VictoryCharacters"));
                
                Debug.Log($"✅ Ardilla {i} spawneada en posición: {spawnPos}");
                
                // ✅ USAR ESCALA INDIVIDUAL PARA CADA MODELO
                Vector3 originalScale = squirrelModels[i].transform.localScale;
                float individualScale = characterScaleMultipliers[i];
                Vector3 targetScale = originalScale * individualScale;
                
                Debug.Log($"   📏 Escala original: {originalScale}, Multiplicador: {individualScale}, Escala final: {targetScale}");
                
                character.transform.localScale = Vector3.zero;
                
                StartCoroutine(AnimateCharacterAppear(character.transform, targetScale));
                
                Animator animator = character.GetComponent<Animator>();
                
                if (animator != null)
                {
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    
                    Debug.Log($"🎭 Animator encontrado en ardilla {i}");
                    Debug.Log($"   - GameObject: {animator.gameObject.name}");
                    Debug.Log($"   - Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL")}");
                    Debug.Log($"   - Enabled: {animator.enabled}");
                    Debug.Log($"   - UpdateMode: {animator.updateMode}");
                    Debug.Log($"   - HasController: {animator.runtimeAnimatorController != null}");
                    
                    if (animator.runtimeAnimatorController == null)
                    {
                        Debug.LogError($"❌ El Animator de ardilla {i} NO TIENE Controller asignado!");
                        yield return new WaitForSecondsRealtime(0.2f);
                        continue;
                    }
                    
                    if (!string.IsNullOrEmpty(sadAnimationTrigger))
                    {
                        bool triggerExists = false;
                        foreach (var param in animator.parameters)
                        {
                            if (param.name == sadAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                            {
                                triggerExists = true;
                                break;
                            }
                        }
                        
                        if (triggerExists)
                        {
                            animator.SetTrigger(sadAnimationTrigger);
                            Debug.Log($"✅ Trigger '{sadAnimationTrigger}' activado en ardilla {i}");
                        }
                        else
                        {
                            Debug.LogError($"❌ Trigger '{sadAnimationTrigger}' NO EXISTE en el Animator de ardilla {i}");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"❌ La ardilla {i} NO TIENE componente Animator!");
                }
                
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    private IEnumerator AnimateCharacterAppear(Transform character, Vector3 targetScale)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            
            character.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            
            yield return null;
        }
        
        character.localScale = targetScale;
    }
    
    private IEnumerator FadeInOverlay(float duration, float targetAlpha)
    {
        if (backgroundOverlay == null) yield break;
        
        float elapsed = 0f;
        Color startColor = backgroundOverlay.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = targetAlpha;
        
        backgroundOverlay.color = startColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            backgroundOverlay.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        
        backgroundOverlay.color = endColor;
    }
    
    private IEnumerator AnimateText(RectTransform textRect, float duration)
    {
        if (textRect == null) yield break;
        
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            
            textRect.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            yield return null;
        }
        
        textRect.localScale = endScale;
    }
    
    private void SetupMessage(GameOverReason reason)
    {
        switch (reason)
        {
            case GameOverReason.TimeUp:
                if (titleText != null) titleText.text = timeUpTitle;
                if (messageText != null) messageText.text = timeUpMessage;
                break;
                
            case GameOverReason.InsufficientFunds:
                if (titleText != null) titleText.text = noMoneyTitle;
                if (messageText != null) messageText.text = noMoneyMessage;
                break;
        }
        
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);
    }
    
    private void HideOtherUIElements()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.HideTimer();
        }
        
        if (InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.HidePromptImmediate();
        }
        
        if (uiElementsToHide != null && uiElementsToHide.Length > 0)
        {
            for (int i = 0; i < uiElementsToHide.Length; i++)
            {
                if (uiElementsToHide[i] != null)
                {
                    uiElementsToHide[i].SetActive(false);
                }
            }
        }
    }
    
    public void RetryLevel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        
        if (letterPulseCoroutine != null)
        {
            StopCoroutine(letterPulseCoroutine);
        }
        
        StopAllCoroutines();
        
        if (gameOverMusicSource != null)
        {
            gameOverMusicSource.Stop();
        }
        
        if (character3DCamera != null)
        {
            character3DCamera.gameObject.SetActive(false);
        }
        
        if (spawnedCharacters != null)
        {
            foreach (var character in spawnedCharacters)
            {
                if (character != null)
                {
                    Destroy(character);
                }
            }
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal...");
        
        if (gameOverMusicSource != null)
        {
            gameOverMusicSource.Stop();
        }
        
        if (character3DCamera != null)
        {
            character3DCamera.gameObject.SetActive(false);
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}