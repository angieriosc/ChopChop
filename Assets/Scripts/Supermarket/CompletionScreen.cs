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
    [SerializeField] private RectTransform[] victoryLetters; // Letras individuales de "VICTORIA" o "YOU WIN"
    [SerializeField] private float letterDropDuration = 0.8f;
    [SerializeField] private float letterDropDelay = 0.1f;
    [SerializeField] private float letterBounceHeight = 200f;
    [SerializeField] private AnimationCurve letterDropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Spotlight")]
    [SerializeField] private Image spotlight;
    [SerializeField] private float spotlightFadeDuration = 1f;
    [SerializeField] private AnimationCurve spotlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Character Models")]
    [SerializeField] private GameObject[] squirrelModels; // Tus 2 modelos de ardillas
    [SerializeField] private Transform characterSpawnPoint;
    [SerializeField] private float characterAppearDelay = 1.5f;
    [SerializeField] private string celebrationAnimationTrigger = "Happy"; // Animación de celebración
    
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
    
    // ✅ NUEVO: Guardar posiciones originales de las letras
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
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // AudioSource separado para la música de victoria
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
        
        // Guardar estados de UI
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
        
        // ✅ GUARDAR POSICIONES ORIGINALES DE LAS LETRAS
        SaveOriginalLetterPositions();
        
        // Configurar estados iniciales
        SetupInitialStates();
    }
    
    /// <summary>
    /// ✅ NUEVO: Guarda las posiciones originales de las letras antes de cualquier animación
    /// </summary>
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
    
    /// <summary>
    /// Configura los estados iniciales de todos los elementos animados.
    /// </summary>
    private void SetupInitialStates()
    {
        // Ocultar cortinas (fuera de la pantalla)
        if (leftCurtain != null)
        {
            leftCurtain.anchoredPosition = new Vector2(-leftCurtain.rect.width, 0);
        }
        
        if (rightCurtain != null)
        {
            rightCurtain.anchoredPosition = new Vector2(rightCurtain.rect.width, 0);
        }
        
        // ✅ CORREGIDO: Usar las posiciones originales guardadas
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
        
        // Ocultar spotlight
        if (spotlight != null)
        {
            Color color = spotlight.color;
            color.a = 0f;
            spotlight.color = color;
            spotlight.gameObject.SetActive(false);
        }
        
        // Ocultar overlay
        if (backgroundOverlay != null)
        {
            Color color = backgroundOverlay.color;
            color.a = 0f;
            backgroundOverlay.color = color;
            backgroundOverlay.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de victoria con la animación completa.
    /// </summary>
    public void ShowCompletion()
    {
        if (completionPanel == null)
        {
            Debug.LogError("❌ CompletionScreen: No se puede mostrar, panel no asignado");
            return;
        }
        
        Debug.Log("🎉 ¡VICTORIA!");
        
        // Detener toda la música del supermercado
        StopAllBackgroundMusic();
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Ocultar otros UI
        HideOtherUIElements();
        
        // Configurar mensaje
        SetupMessage();
        
        // ✅ RESTABLECER POSICIONES ANTES DE MOSTRAR
        ResetLetterPositions();
        
        // Mostrar panel
        completionPanel.SetActive(true);
        
        // Iniciar secuencia de animación
        StartCoroutine(VictoryAnimationSequence());
    }
    
    /// <summary>
    /// ✅ NUEVO: Restablece las letras a sus posiciones iniciales (arriba)
    /// </summary>
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
                    victoryLetters[i].localScale = Vector3.one; // ✅ Reset escala también
                    victoryLetters[i].rotation = Quaternion.identity; // ✅ Reset rotación
                }
            }
        }
    }
    
    /// <summary>
    /// Detiene toda la música de fondo del juego.
    /// </summary>
    private void StopAllBackgroundMusic()
    {
        Debug.Log("🔇 Iniciando detención de audio (Victoria)...");
        
        // Método 1: Detener música del supermercado
        if (SupermarketAudioManager.Instance != null)
        {
            SupermarketAudioManager.Instance.StopAllMusic();
            Debug.Log("✅ SupermarketAudioManager.StopAllMusic() ejecutado");
        }
        else
        {
            Debug.LogWarning("⚠️ SupermarketAudioManager.Instance es null");
        }
        
        // Método 2: Buscar y detener TODOS los AudioSource en la escena
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);
        Debug.Log($"🔍 Encontrados {allAudioSources.Length} AudioSources en la escena");
        
        int stoppedCount = 0;
        foreach (AudioSource source in allAudioSources)
        {
            // No detener los AudioSource de este script
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
        
        // Método 3: Usar AudioKiller como última medida
        if (stoppedCount == 0)
        {
            Debug.LogWarning("⚠️ No se detuvo ningún audio, usando AudioKiller...");
            AudioKiller.Instance.KillAllAudioExcept(audioSource, victoryMusicSource);
        }
        
        Debug.Log("🔇 Proceso de detención de audio completado");
    }
    
    /// <summary>
    /// Secuencia completa de animación estilo Paper Mario para victoria.
    /// </summary>
    private IEnumerator VictoryAnimationSequence()
    {
        // 1. Fade in del fondo dorado/celebración
        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInOverlay(0.5f, 0.9f));
        }
        
        // 2. Sonido de cortinas
        if (curtainSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(curtainSound);
        }
        
        // 3. Animación de cortinas entrando
        yield return StartCoroutine(AnimateCurtains());
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        // 4. Letras de "VICTORIA" o "YOU WIN" cayendo
        yield return StartCoroutine(AnimateVictoryLetters());
        
        // 5. Reproducir música de victoria (loop)
        if (victorySound != null && victoryMusicSource != null)
        {
            victoryMusicSource.clip = victorySound;
            victoryMusicSource.Play();
            Debug.Log("🎵 Música de Victoria iniciada");
        }
        
        // 6. Sonido extra de celebración
        if (celebrationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(celebrationSound);
        }
        
        // 7. Iniciar animación de pulso en las letras
        if (enableLetterPulse && victoryLetters != null && victoryLetters.Length > 0)
        {
            letterPulseCoroutine = StartCoroutine(PulseLetters());
        }
        
        // 8. Spotlight apareciendo
        yield return StartCoroutine(AnimateSpotlight());
        
        // 9. Crear efecto de partículas
        if (particleEffectPrefab != null)
        {
            Vector3 spawnPos = particleSpawnPoint != null ? particleSpawnPoint.position : Vector3.zero;
            // ✅ CORREGIDO: No usar completionPanel.transform como padre
            spawnedParticles = Instantiate(particleEffectPrefab, spawnPos, Quaternion.identity);
        }
        
        // 10. Ardillas apareciendo con animación feliz
        yield return StartCoroutine(SpawnCharacters());
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // 11. Mostrar mensaje y botón
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
    
    /// <summary>
    /// Hace que las letras pulsen continuamente con efecto de onda.
    /// </summary>
    private IEnumerator PulseLetters()
    {
        if (victoryLetters == null || victoryLetters.Length == 0 || originalLetterPositions == null) yield break;
        
        // ✅ Guardar escalas originales (debe ser Vector3.one después de la animación)
        Vector3[] originalScales = new Vector3[victoryLetters.Length];
        
        for (int i = 0; i < victoryLetters.Length; i++)
        {
            if (victoryLetters[i] != null)
            {
                originalScales[i] = Vector3.one; // ✅ Siempre usar escala 1
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
                    // Cada letra tiene un offset en la onda
                    float wave = Mathf.Sin(timeOffset + (i * pulseDelay * 10f));
                    
                    // Calcular escala con pulso
                    float scaleMultiplier = 1f + (wave * pulseAmount);
                    
                    // Aplicar escala
                    victoryLetters[i].localScale = originalScales[i] * scaleMultiplier;
                    
                    // ✅ Movimiento vertical (igual que GameOver)
                    Vector2 pos = originalLetterPositions[i];
                    pos.y = originalLetterPositions[i].y + (wave * verticalMovement);
                    victoryLetters[i].anchoredPosition = pos;
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Anima las cortinas entrando desde los lados.
    /// </summary>
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
    
    /// <summary>
    /// Anima las letras de VICTORIA/YOU WIN cayendo una por una.
    /// </summary>
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
                
                // Sonido de letra cayendo
                if (letterDropSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(letterDropSound, 0.5f);
                }
                
                yield return new WaitForSecondsRealtime(letterDropDelay);
            }
        }
        
        // Esperar a que termine la última letra
        yield return new WaitForSecondsRealtime(letterDropDuration);
    }
    
    /// <summary>
    /// ✅ CORREGIDO: Hace caer una letra individual con bounce usando posición original
    /// </summary>
    private IEnumerator DropLetter(RectTransform letter, int index)
    {
        float elapsed = 0f;
        Vector2 startPos = letter.anchoredPosition;
        Vector2 targetPos = originalLetterPositions[index]; // ✅ Usar posición original guardada
        
        while (elapsed < letterDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / letterDropDuration;
            float curveValue = letterDropCurve.Evaluate(progress);
            
            letter.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            
            // Rotación mientras cae
            letter.rotation = Quaternion.Euler(0, 0, Mathf.Sin(progress * Mathf.PI * 2) * 10f);
            
            yield return null;
        }
        
        letter.anchoredPosition = targetPos;
        letter.rotation = Quaternion.identity;
    }
    
    /// <summary>
    /// Anima el spotlight apareciendo.
    /// </summary>
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
    /// Hace aparecer los personajes de ardilla con animación feliz.
    /// </summary>
    private IEnumerator SpawnCharacters()
    {
        if (squirrelModels == null || squirrelModels.Length == 0 || characterSpawnPoint == null)
        {
            yield break;
        }
        
        yield return new WaitForSecondsRealtime(characterAppearDelay);
        
        spawnedCharacters = new GameObject[squirrelModels.Length];
        
        for (int i = 0; i < squirrelModels.Length; i++)
        {
            if (squirrelModels[i] != null)
            {
                // Calcular posición (separar si son múltiples)
                Vector3 spawnPos = characterSpawnPoint.position;
                if (squirrelModels.Length > 1)
                {
                    float offset = (i - (squirrelModels.Length - 1) / 2f) * 1.5f;
                    spawnPos += characterSpawnPoint.right * offset;
                }
                
                // ✅ CORREGIDO: No usar completionPanel.transform como padre
                GameObject character = Instantiate(squirrelModels[i], spawnPos, characterSpawnPoint.rotation);
                spawnedCharacters[i] = character;
                
                // Escala inicial pequeña
                character.transform.localScale = Vector3.zero;
                
                // Animar aparición
                StartCoroutine(AnimateCharacterAppear(character.transform));
                
                // Activar animación feliz
                Animator animator = character.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(celebrationAnimationTrigger))
                {
                    animator.SetTrigger(celebrationAnimationTrigger);
                }
                
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }
    
    /// <summary>
    /// Anima la aparición de un personaje.
    /// </summary>
    private IEnumerator AnimateCharacterAppear(Transform character)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            
            character.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            
            yield return null;
        }
        
        character.localScale = targetScale;
    }
    
    /// <summary>
    /// Fade in del overlay de fondo.
    /// </summary>
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
    
    /// <summary>
    /// Anima un texto apareciendo con escala.
    /// </summary>
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
    
    /// <summary>
    /// Configura el mensaje de victoria.
    /// </summary>
    private void SetupMessage()
    {
        if (titleText != null) titleText.text = victoryTitle;
        if (messageText != null) messageText.text = victoryMessage;
        
        // Ocultar inicialmente
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
    }
    
    /// <summary>
    /// Oculta otros elementos de UI.
    /// </summary>
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
    
    /// <summary>
    /// Oculta la pantalla de victoria.
    /// </summary>
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
    
    /// <summary>
    /// Continúa al siguiente nivel o menú.
    /// </summary>
    public void ContinueGame()
    {
        Debug.Log("➡️ Continuando...");
        
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
        
        // Aquí puedes cargar el siguiente nivel o volver al menú
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Reinicia el nivel actual.
    /// </summary>
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