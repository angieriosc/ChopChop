using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Muestra una pantalla de felicitaciones estilo Paper Mario cuando se completa la compra.
/// </summary>
public class CompletionScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private Image backgroundOverlay;
    
    [Header("Curtains Animation")]
    [SerializeField] private RectTransform leftCurtain;
    [SerializeField] private RectTransform rightCurtain;
    [SerializeField] private float curtainAnimationDuration = 1.5f;
    [SerializeField] private AnimationCurve curtainCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Victory Letters")]
    [SerializeField] private RectTransform[] victoryLetters;
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
    [SerializeField] private string celebrationAnimationTrigger = "Happy";
    
    // ✅ NUEVO: Array de escalas individuales para cada modelo
    [Header("Individual Character Scales")]
    [Tooltip("Escala individual para cada modelo de ardilla. Debe tener el mismo tamaño que squirrelModels")]
    [SerializeField] private float[] characterScaleMultipliers = new float[] { 3f, 3f };
    
    [SerializeField] private float characterSeparation = 2f;
    [SerializeField] private LayerMask characterLayer;
    
    [Header("Messages")]
    [SerializeField] private string victoryTitle = "¡VICTORIA!";
    [SerializeField] private string victoryMessage = "¡Compra completada con éxito!";
    
    [Header("Audio")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip curtainSound;
    [SerializeField] private AudioClip letterDropSound;
    [SerializeField] private AudioClip celebrationSound;
    [SerializeField] private bool loopVictoryMusic = true;
    [SerializeField] private float victoryMusicVolume = 0.7f;
    
    [Header("Letter Pulse Animation")]
    [SerializeField] private bool enableLetterPulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float pulseDelay = 0.05f;
    [SerializeField] private float verticalMovement = 5f;
    
    [Header("Particles")]
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private Transform particleSpawnPoint;
    
    [Header("UI to Hide")]
    [SerializeField] private GameObject[] uiElementsToHide;
    
    public static CompletionScreen Instance { get; private set; }
    
    private AudioSource audioSource;
    private AudioSource victoryMusicSource;
    private bool[] originalUIStates;
    private GameObject[] spawnedCharacters;
    private Coroutine letterPulseCoroutine;
    private GameObject spawnedParticles;
    private Vector2[] originalLetterPositions;
    
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
        
        GameObject musicObj = new GameObject("VictoryMusic");
        musicObj.transform.SetParent(transform);
        victoryMusicSource = musicObj.AddComponent<AudioSource>();
        victoryMusicSource.loop = loopVictoryMusic;
        victoryMusicSource.volume = victoryMusicVolume;
        victoryMusicSource.playOnAwake = false;
        
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
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
        
        SaveOriginalLetterPositions();
        SetupInitialStates();
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
        if (victoryLetters != null && victoryLetters.Length > 0)
        {
            originalLetterPositions = new Vector2[victoryLetters.Length];
            for (int i = 0; i < victoryLetters.Length; i++)
            {
                if (victoryLetters[i] != null)
                {
                    originalLetterPositions[i] = victoryLetters[i].anchoredPosition;
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
        
        if (victoryLetters != null && originalLetterPositions != null)
        {
            for (int i = 0; i < victoryLetters.Length; i++)
            {
                if (victoryLetters[i] != null)
                {
                    Vector2 pos = originalLetterPositions[i];
                    pos.y += letterBounceHeight;
                    victoryLetters[i].anchoredPosition = pos;
                    victoryLetters[i].gameObject.SetActive(false);
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
    
    public void ShowCompletion()
    {
        if (completionPanel == null)
        {
            Debug.LogError("❌ CompletionScreen: No se puede mostrar, panel no asignado");
            return;
        }
        
        Debug.Log("🎉 ¡VICTORIA!");
        
        StopAllBackgroundMusic();
        Time.timeScale = 0f;
        HideOtherUIElements();
        SetupMessage();
        ResetLetterPositions();
        
        completionPanel.SetActive(true);
        StartCoroutine(VictoryAnimationSequence());
    }
    
    private void ResetLetterPositions()
    {
        if (victoryLetters != null && originalLetterPositions != null)
        {
            for (int i = 0; i < victoryLetters.Length; i++)
            {
                if (victoryLetters[i] != null)
                {
                    Vector2 pos = originalLetterPositions[i];
                    pos.y += letterBounceHeight;
                    victoryLetters[i].anchoredPosition = pos;
                    victoryLetters[i].gameObject.SetActive(false);
                    victoryLetters[i].localScale = Vector3.one;
                    victoryLetters[i].rotation = Quaternion.identity;
                }
            }
        }
    }
    
    private void StopAllBackgroundMusic()
    {
        Debug.Log("🔇 Iniciando detención de audio (Victoria)...");
        
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
            if (source != audioSource && source != victoryMusicSource)
            {
                bool wasPlaying = source.isPlaying;
                source.Stop();
                source.mute = true;
                source.enabled = false;
                
                if (wasPlaying)
                {
                    stoppedCount++;
                    Debug.Log($"🔇 Detenido: {source.gameObject.name}");
                }
            }
        }
        
        Debug.Log($"✅ {stoppedCount} AudioSources detenidos");
        
        if (stoppedCount == 0)
        {
            Debug.LogWarning("⚠ No se detuvo ningún audio, usando AudioKiller...");
            AudioKiller.Instance.KillAllAudioExcept(audioSource, victoryMusicSource);
        }
        
        Debug.Log("🔇 Proceso de detención de audio completado");
    }
    
    private IEnumerator VictoryAnimationSequence()
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
        yield return StartCoroutine(AnimateVictoryLetters());
        
        if (victorySound != null && victoryMusicSource != null)
        {
            victoryMusicSource.clip = victorySound;
            victoryMusicSource.Play();
            Debug.Log("🎵 Música de Victoria iniciada");
        }
        
        if (celebrationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(celebrationSound);
        }
        
        if (enableLetterPulse && victoryLetters != null && victoryLetters.Length > 0)
        {
            letterPulseCoroutine = StartCoroutine(PulseLetters());
        }
        
        yield return StartCoroutine(AnimateSpotlight());
        
        if (particleEffectPrefab != null)
        {
            Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : Vector3.zero;
            spawnedParticles = Instantiate(particleEffectPrefab, spawnPos, Quaternion.identity);
        }
        
        yield return StartCoroutine(SpawnCharacters());
        yield return new WaitForSecondsRealtime(0.5f);
        
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateText(messageText.GetComponent<RectTransform>(), 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        if (continueButton != null)
        {
            continueButton.SetActive(true);
            yield return StartCoroutine(AnimateText(continueButton.GetComponent<RectTransform>(), 0.3f));
        }
    }
    
    private IEnumerator PulseLetters()
    {
        if (victoryLetters == null || victoryLetters.Length == 0 || originalLetterPositions == null) yield break;
        
        Vector3[] originalScales = new Vector3[victoryLetters.Length];
        
        for (int i = 0; i < victoryLetters.Length; i++)
        {
            if (victoryLetters[i] != null)
            {
                originalScales[i] = Vector3.one;
            }
        }
        
        float timeOffset = 0f;
        
        while (true)
        {
            timeOffset += Time.unscaledDeltaTime * pulseSpeed;
            
            for (int i = 0; i < victoryLetters.Length; i++)
            {
                if (victoryLetters[i] != null)
                {
                    float wave = Mathf.Sin(timeOffset + (i * pulseDelay * 10f));
                    float scaleMultiplier = 1f + (wave * pulseAmount);
                    
                    victoryLetters[i].localScale = originalScales[i] * scaleMultiplier;
                    
                    Vector2 pos = originalLetterPositions[i];
                    pos.y = originalLetterPositions[i].y + (wave * verticalMovement);
                    victoryLetters[i].anchoredPosition = pos;
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
    
    private IEnumerator AnimateVictoryLetters()
    {
        if (victoryLetters == null || victoryLetters.Length == 0)
        {
            yield break;
        }
        
        for (int i = 0; i < victoryLetters.Length; i++)
        {
            if (victoryLetters[i] != null)
            {
                victoryLetters[i].gameObject.SetActive(true);
                StartCoroutine(DropLetter(victoryLetters[i], i));
                
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
                if (animator == null)
                {
                    animator = character.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        Debug.Log($"🔍 Animator encontrado en child de ardilla {i}: {animator.gameObject.name}");
                    }
                }
                
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
                    
                    if (!string.IsNullOrEmpty(celebrationAnimationTrigger))
                    {
                        bool triggerExists = false;
                        foreach (var param in animator.parameters)
                        {
                            if (param.name == celebrationAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                            {
                                triggerExists = true;
                                break;
                            }
                        }
                        
                        if (triggerExists)
                        {
                            animator.SetTrigger(celebrationAnimationTrigger);
                            Debug.Log($"✅ Trigger '{celebrationAnimationTrigger}' activado en ardilla {i}");
                        }
                        else
                        {
                            Debug.LogError($"❌ Trigger '{celebrationAnimationTrigger}' NO EXISTE en el Animator de ardilla {i}");
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
    
    private void SetupMessage()
    {
        if (titleText != null) titleText.text = victoryTitle;
        if (messageText != null) messageText.text = victoryMessage;
        
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
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
    
    public void HideCompletion()
    {
        if (letterPulseCoroutine != null)
        {
            StopCoroutine(letterPulseCoroutine);
        }
        
        StopAllCoroutines();
        
        if (victoryMusicSource != null)
        {
            victoryMusicSource.Stop();
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
        
        if (spawnedParticles != null)
        {
            Destroy(spawnedParticles);
        }
        
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }
    
    public void ContinueGame()
    {
        Debug.Log("➡ Continuando...");
        
        if (letterPulseCoroutine != null)
        {
            StopCoroutine(letterPulseCoroutine);
        }
        
        StopAllCoroutines();
        
        if (victoryMusicSource != null)
        {
            victoryMusicSource.Stop();
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
        
        if (spawnedParticles != null)
        {
            Destroy(spawnedParticles);
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("CinmeaticChopChop 2");
    }
    
    public void RestartLevel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        
        if (letterPulseCoroutine != null)
        {
            StopCoroutine(letterPulseCoroutine);
        }
        
        StopAllCoroutines();
        
        if (victoryMusicSource != null)
        {
            victoryMusicSource.Stop();
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
        
        if (spawnedParticles != null)
        {
            Destroy(spawnedParticles);
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}