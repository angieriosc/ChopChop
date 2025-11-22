using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Razones de Game Over.
/// </summary>
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
    [SerializeField] private RectTransform[] gameOverLetters; // Cada letra individual
    [SerializeField] private float letterDropDuration = 0.8f;
    [SerializeField] private float letterDropDelay = 0.1f; // Delay entre cada letra
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
    [SerializeField] private string sadAnimationTrigger = "Sad"; // Nombre del trigger de animación triste
    
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
    [SerializeField] private float pulseAmount = 0.15f; // 15% de escala
    [SerializeField] private float pulseDelay = 0.05f; // Delay entre cada letra
    [SerializeField] private float verticalMovement = 5f; // ✅ NUEVO: Movimiento vertical en píxeles
    
    [Header("UI to Hide")]
    [SerializeField] private GameObject[] uiElementsToHide;
    
    public static GameOverScreen Instance { get; private set; }
    
    private AudioSource audioSource;
    private AudioSource gameOverMusicSource;
    private bool[] originalUIStates;
    private GameObject[] spawnedCharacters;
    private Coroutine letterPulseCoroutine;
    
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
        
        // AudioSource separado para la música de Game Over
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
        
        // Configurar elementos iniciales
        SetupInitialStates();
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
        
        // Ocultar letras (arriba de la pantalla)
        if (gameOverLetters != null)
        {
            foreach (var letter in gameOverLetters)
            {
                if (letter != null)
                {
                    Vector2 pos = letter.anchoredPosition;
                    pos.y += letterBounceHeight;
                    letter.anchoredPosition = pos;
                    letter.gameObject.SetActive(false);
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
    /// Muestra la pantalla de Game Over con la animación completa.
    /// </summary>
    public void ShowGameOver(GameOverReason reason)
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("❌ GameOverScreen: No se puede mostrar, panel no asignado");
            return;
        }
        
        Debug.Log($"💀 Game Over: {reason}");
        
        // ✅ DETENER TODA LA MÚSICA DEL SUPERMERCADO
        StopAllBackgroundMusic();
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Ocultar otros UI
        HideOtherUIElements();
        
        // Configurar mensaje
        SetupMessage(reason);
        
        // Mostrar panel
        gameOverPanel.SetActive(true);
        
        // Iniciar secuencia de animación
        StartCoroutine(GameOverAnimationSequence());
    }
    
    /// <summary>
    /// Detiene toda la música de fondo del juego.
    /// </summary>
    private void StopAllBackgroundMusic()
    {
        Debug.Log("🔇 Iniciando detención de audio...");
        
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
        
        // Método 3: Usar AudioKiller como última medida (nuclear option)
        if (stoppedCount == 0)
        {
            Debug.LogWarning("⚠️ No se detuvo ningún audio, usando AudioKiller...");
            AudioKiller.Instance.KillAllAudioExcept(audioSource, gameOverMusicSource);
        }
        
        Debug.Log("🔇 Proceso de detención de audio completado");
    }
    
    /// <summary>
    /// Secuencia completa de animación estilo Paper Mario.
    /// </summary>
    private IEnumerator GameOverAnimationSequence()
    {
        // 1. Fade in del fondo negro
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
        
        // 4. Letras de "GAME OVER" cayendo
        yield return StartCoroutine(AnimateGameOverLetters());
        
        // 5. ✅ REPRODUCIR MÚSICA DE GAME OVER (loop)
        if (gameOverSound != null && gameOverMusicSource != null)
        {
            gameOverMusicSource.clip = gameOverSound;
            gameOverMusicSource.Play();
            Debug.Log("🎵 Música de Game Over iniciada");
        }
        
        // 6. ✅ INICIAR ANIMACIÓN DE PULSO EN LAS LETRAS
        if (enableLetterPulse && gameOverLetters != null && gameOverLetters.Length > 0)
        {
            letterPulseCoroutine = StartCoroutine(PulseLetters());
        }
        
        // 7. Spotlight apareciendo
        yield return StartCoroutine(AnimateSpotlight());
        
        // 8. Ardillas apareciendo con animación triste
        yield return StartCoroutine(SpawnCharacters());
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // 9. Mostrar mensaje y botón
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
    
    /// <summary>
    /// Hace que las letras pulsen continuamente con efecto de onda.
    /// </summary>
    private IEnumerator PulseLetters()
    {
        if (gameOverLetters == null || gameOverLetters.Length == 0) yield break;
        
        // Guardar escalas y posiciones originales
        Vector3[] originalScales = new Vector3[gameOverLetters.Length];
        Vector2[] originalPositions = new Vector2[gameOverLetters.Length];
        
        for (int i = 0; i < gameOverLetters.Length; i++)
        {
            if (gameOverLetters[i] != null)
            {
                originalScales[i] = gameOverLetters[i].localScale;
                originalPositions[i] = gameOverLetters[i].anchoredPosition;
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
                    // Cada letra tiene un offset en la onda
                    float wave = Mathf.Sin(timeOffset + (i * pulseDelay * 10f));
                    
                    // Calcular escala con pulso
                    float scaleMultiplier = 1f + (wave * pulseAmount);
                    
                    // Aplicar escala
                    gameOverLetters[i].localScale = originalScales[i] * scaleMultiplier;
                    
                    // ✅ MOVIMIENTO VERTICAL REDUCIDO (de 10 a 5 píxeles)
                    Vector2 pos = originalPositions[i];
                    pos.y = originalPositions[i].y + (wave * 5f); // Solo 5 píxeles arriba/abajo
                    gameOverLetters[i].anchoredPosition = pos;
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
    /// Anima las letras de GAME OVER cayendo una por una.
    /// </summary>
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
                StartCoroutine(DropLetter(gameOverLetters[i]));
                
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
    /// Hace caer una letra individual con bounce.
    /// </summary>
    private IEnumerator DropLetter(RectTransform letter)
    {
        float elapsed = 0f;
        Vector2 startPos = letter.anchoredPosition;
        Vector2 targetPos = startPos;
        targetPos.y -= letterBounceHeight;
        
        // Guardar posición target para el pulso
        Vector2 finalPos = targetPos;
        
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
        
        letter.anchoredPosition = finalPos;
        letter.rotation = Quaternion.identity;
        
        // ✅ Guardar la posición Y original para el efecto de pulso
        // El pulso necesita saber la posición base
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
    /// Hace aparecer los personajes de ardilla con animación triste.
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
                
                // Instanciar
                GameObject character = Instantiate(squirrelModels[i], spawnPos, characterSpawnPoint.rotation, gameOverPanel.transform);
                spawnedCharacters[i] = character;
                
                // Escala inicial pequeña
                character.transform.localScale = Vector3.zero;
                
                // Animar aparición
                StartCoroutine(AnimateCharacterAppear(character.transform));
                
                // Activar animación triste
                Animator animator = character.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(sadAnimationTrigger))
                {
                    animator.SetTrigger(sadAnimationTrigger);
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
    /// Configura el mensaje según la razón del Game Over.
    /// </summary>
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
        
        // Ocultar inicialmente
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (retryButton != null) retryButton.SetActive(false);
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
    /// Reinicia el nivel actual.
    /// </summary>
    public void RetryLevel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        
        // Detener animaciones
        if (letterPulseCoroutine != null)
        {
            StopCoroutine(letterPulseCoroutine);
        }
        
        StopAllCoroutines();
        
        // Detener música de Game Over
        if (gameOverMusicSource != null)
        {
            gameOverMusicSource.Stop();
        }
        
        // Limpiar personajes spawneados
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
        
        // Restaurar timeScale
        Time.timeScale = 1f;
        
        // Recargar la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Va al menú principal.
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Volviendo al menú principal...");
        
        // Detener música
        if (gameOverMusicSource != null)
        {
            gameOverMusicSource.Stop();
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}