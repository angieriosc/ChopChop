using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema de reto de divisiones para activar el horno.
/// </summary>
public class DivisionChallenge : MonoBehaviour
{
    [Header("Referencias UI - Panel Principal")]
    [SerializeField] private GameObject _challengePanel;
    [SerializeField] private TextMeshProUGUI _introText;
    [SerializeField] private GameObject _questionPanel;
    
    [Header("Referencias UI - Pregunta")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _questionText;
    [SerializeField] private Button[] _answerButtons; // 3 botones
    [SerializeField] private TextMeshProUGUI[] _answerTexts; // Textos de los 3 botones
    
    [Header("Referencias UI - Progreso")]
    [SerializeField] private Image _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText; // "2/5"
    [SerializeField] private TextMeshProUGUI _bonusText; // "x3 Bonus!"
    
    [Header("Referencias UI - Feedback")]
    [SerializeField] private FeedbackIconHelper[] _buttonFeedbackIcons; // 3 helpers de feedback
    
    [Header("Configuración del Reto")]
    [SerializeField] private int _totalQuestions = 5;
    [SerializeField] private float _timePerQuestion = 8f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _correctSound;
    [SerializeField] private AudioClip _incorrectSound;
    [SerializeField] private AudioClip _challengeCompleteSound;
    private AudioSource _audioSource;
    
    // Variables privadas
    private int _currentCorrectAnswers = 0;
    private int _currentBonus = 1;
    private float _currentTimer = 0f;
    private bool _isTimerRunning = false;
    private bool _waitingForFeedback = false;
    
    private int _correctAnswer;
    private int _selectedAnswerIndex = -1;
    
    // Evento que se dispara cuando se completa el reto
    public event Action<int> OnChallengeCompleted; // Pasa el bonus final
    public event Action OnChallengeFailed;
    
    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        
        // Ocultar todo al inicio
        if (_challengePanel != null) _challengePanel.SetActive(false);
        if (_questionPanel != null) _questionPanel.SetActive(false);
        HideFeedbackIcons();
    }
    
    /// <summary>
    /// Inicia el reto de divisiones.
    /// </summary>
    public void StartChallenge()
    {
        _currentCorrectAnswers = 0;
        _currentBonus = 1;
        _currentTimer = 0f;
        
        _challengePanel.SetActive(true);
        _questionPanel.SetActive(false);
        
        // Mostrar texto introductorio
        if (_introText != null)
        {
            _introText.text = "¡Es hora del reto!\n\nDebes responder 5 divisiones correctamente para activar el horno y comenzar a cocinar.";
            _introText.gameObject.SetActive(true);
        }
        
        UpdateProgressUI();
        UpdateBonusUI();
        
        // Después de 3 segundos, iniciar las preguntas
        StartCoroutine(StartQuestionsAfterDelay(3f));
    }
    
    private IEnumerator StartQuestionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_introText != null) _introText.gameObject.SetActive(false);
        _questionPanel.SetActive(true);
        
        GenerateNewQuestion();
    }
    
    /// <summary>
    /// Genera una nueva pregunta de división.
    /// </summary>
    private void GenerateNewQuestion()
    {
        HideFeedbackIcons();
        _waitingForFeedback = false;
        _selectedAnswerIndex = -1;
        
        // Activar todos los botones
        foreach (Button btn in _answerButtons)
        {
            btn.interactable = true;
        }
        
        // Generar división aleatoria
        int divisor = UnityEngine.Random.Range(2, 10); // 2-9
        int quotient = UnityEngine.Random.Range(2, 10); // 2-9
        int dividend = divisor * quotient; // Asegura división exacta
        
        _correctAnswer = quotient;
        
        // Mostrar pregunta
        if (_questionText != null)
        {
            _questionText.text = $"{dividend} ÷ {divisor} = ?";
        }
        
        // Generar respuestas (1 correcta, 2 incorrectas)
        List<int> answers = new List<int> { _correctAnswer };
        
        // Generar respuestas incorrectas
        while (answers.Count < 3)
        {
            int wrongAnswer = UnityEngine.Random.Range(1, 15);
            if (!answers.Contains(wrongAnswer))
            {
                answers.Add(wrongAnswer);
            }
        }
        
        // Mezclar respuestas
        ShuffleList(answers);
        
        // Asignar respuestas a botones
        for (int i = 0; i < _answerButtons.Length && i < answers.Count; i++)
        {
            int answerValue = answers[i];
            _answerTexts[i].text = answerValue.ToString();
            
            // Remover listeners previos y agregar nuevo
            _answerButtons[i].onClick.RemoveAllListeners();
            int index = i; // Capturar índice para el closure
            _answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answerValue));
        }
        
        // Iniciar temporizador
        _currentTimer = _timePerQuestion;
        _isTimerRunning = true;
    }
    
    private void Update()
    {
        if (_isTimerRunning && !_waitingForFeedback)
        {
            _currentTimer -= Time.deltaTime;
            
            if (_timerText != null)
            {
                _timerText.text = $"Tiempo: {Mathf.CeilToInt(_currentTimer)}s";
            }
            
            // Si se acaba el tiempo
            if (_currentTimer <= 0f)
            {
                _isTimerRunning = false;
                OnTimeUp();
            }
        }
    }
    
    /// <summary>
    /// Maneja la selección de una respuesta.
    /// </summary>
    private void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (_waitingForFeedback) return;
        
        _isTimerRunning = false;
        _waitingForFeedback = true;
        _selectedAnswerIndex = buttonIndex;
        
        // Desactivar todos los botones
        foreach (Button btn in _answerButtons)
        {
            btn.interactable = false;
        }
        
        bool isCorrect = selectedAnswer == _correctAnswer;
        
        if (isCorrect)
        {
            HandleCorrectAnswer(buttonIndex);
        }
        else
        {
            HandleIncorrectAnswer(buttonIndex);
        }
    }
    
    /// <summary>
    /// Maneja una respuesta correcta.
    /// </summary>
    private void HandleCorrectAnswer(int buttonIndex)
    {
        // Reproducir sonido correcto
        PlaySound(_correctSound);
        
        // Mostrar palomita en el botón seleccionado
        ShowFeedbackIcon(buttonIndex, true);
        
        // Incrementar respuestas correctas y bonus
        _currentCorrectAnswers++;
        _currentBonus++;
        
        UpdateProgressUI();
        UpdateBonusUI();
        
        // Verificar si completó el reto
        if (_currentCorrectAnswers >= _totalQuestions)
        {
            StartCoroutine(CompleteChallenge());
        }
        else
        {
            StartCoroutine(NextQuestionAfterDelay(1.5f));
        }
    }
    
    /// <summary>
    /// Maneja una respuesta incorrecta.
    /// </summary>
    private void HandleIncorrectAnswer(int buttonIndex)
    {
        // Reproducir sonido incorrecto
        PlaySound(_incorrectSound);
        
        // Mostrar tacha en el botón seleccionado
        ShowFeedbackIcon(buttonIndex, false);
        
        // Resetear bonus
        _currentBonus = 1;
        UpdateBonusUI();
        
        // Encontrar y mostrar la respuesta correcta
        StartCoroutine(ShowCorrectAnswerAfterDelay(0.5f));
    }
    
    /// <summary>
    /// Muestra la respuesta correcta después de un error.
    /// </summary>
    private IEnumerator ShowCorrectAnswerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Encontrar el botón con la respuesta correcta
        for (int i = 0; i < _answerTexts.Length; i++)
        {
            if (int.TryParse(_answerTexts[i].text, out int value))
            {
                if (value == _correctAnswer)
                {
                    ShowFeedbackIcon(i, true);
                    break;
                }
            }
        }
        
        yield return new WaitForSeconds(1.5f);
        GenerateNewQuestion();
    }
    
    /// <summary>
    /// Maneja cuando se acaba el tiempo.
    /// </summary>
    private void OnTimeUp()
    {
        _waitingForFeedback = true;
        
        // Desactivar botones
        foreach (Button btn in _answerButtons)
        {
            btn.interactable = false;
        }
        
        // Resetear bonus
        _currentBonus = 1;
        UpdateBonusUI();
        
        // Mostrar respuesta correcta
        StartCoroutine(ShowCorrectAnswerAfterDelay(0.5f));
    }
    
    /// <summary>
    /// Pasa a la siguiente pregunta.
    /// </summary>
    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GenerateNewQuestion();
    }
    
    /// <summary>
    /// Completa el reto exitosamente.
    /// </summary>
    private IEnumerator CompleteChallenge()
    {
        yield return new WaitForSeconds(1.5f);
        
        PlaySound(_challengeCompleteSound);
        
        if (_introText != null)
        {
            _introText.gameObject.SetActive(true);
            _introText.text = $"¡Reto Completado!\n\nBonus Final: x{_currentBonus}\n\n¡El horno está listo para cocinar!";
        }
        
        _questionPanel.SetActive(false);
        
        yield return new WaitForSeconds(2.5f);
        
        // Cerrar panel y notificar
        _challengePanel.SetActive(false);
        OnChallengeCompleted?.Invoke(_currentBonus);
    }
    
    /// <summary>
    /// Actualiza la UI de progreso.
    /// </summary>
    private void UpdateProgressUI()
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = (float)_currentCorrectAnswers / _totalQuestions;
        }
        
        if (_progressText != null)
        {
            _progressText.text = $"{_currentCorrectAnswers}/{_totalQuestions}";
        }
    }
    
    /// <summary>
    /// Actualiza la UI del bonus.
    /// </summary>
    private void UpdateBonusUI()
    {
        if (_bonusText != null)
        {
            if (_currentBonus > 1)
            {
                _bonusText.gameObject.SetActive(true);
                _bonusText.text = $"x{_currentBonus} Bonus!";
            }
            else
            {
                _bonusText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Muestra iconos de feedback (palomita o tacha).
    /// </summary>
    private void ShowFeedbackIcon(int buttonIndex, bool isCorrect)
    {
        if (_buttonFeedbackIcons == null || buttonIndex >= _buttonFeedbackIcons.Length) return;
        
        FeedbackIconHelper helper = _buttonFeedbackIcons[buttonIndex];
        if (helper != null)
        {
            if (isCorrect)
                helper.ShowCorrect();
            else
                helper.ShowIncorrect();
        }
    }
    
    /// <summary>
    /// Oculta todos los iconos de feedback.
    /// </summary>
    private void HideFeedbackIcons()
    {
        if (_buttonFeedbackIcons != null)
        {
            foreach (FeedbackIconHelper helper in _buttonFeedbackIcons)
            {
                if (helper != null) helper.Hide();
            }
        }
    }
    
    // Métodos auxiliares
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}