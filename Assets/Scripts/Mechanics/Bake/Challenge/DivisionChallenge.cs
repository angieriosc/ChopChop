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
    [SerializeField] private Button[] _answerButtons;
    [SerializeField] private TextMeshProUGUI[] _answerTexts;
    [SerializeField] private ButtonAnimator[] _buttonAnimators;
    [SerializeField] private TimerPulseEffect _timerPulseEffect;
    
    [Header("Referencias UI - Progreso")]
    [SerializeField] private Image _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _bonusText;
    
    [Header("Referencias UI - Feedback")]
    [SerializeField] private FeedbackIconHelper[] _buttonFeedbackIcons;
    
    [Header("Configuración del Reto")]
    [SerializeField] private int _totalQuestions = 5;
    [SerializeField] private float _timePerQuestion = 8f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _correctSound;
    [SerializeField] private AudioClip _incorrectSound;
    [SerializeField] private AudioClip _challengeCompleteSound;
    [SerializeField] private AudioClip _tickSound;
    private AudioSource _audioSource;
    
    [Header("UI a ocultar durante el reto")]
    [SerializeField] private GameObject[] _uiElementsToHide; // Arrastra aquí los paneles que quieres ocultar
    
    // Guardar estado previo de los elementos UI
    private Dictionary<GameObject, bool> _previousUIStates = new Dictionary<GameObject, bool>();
    
    // Variables privadas
    private int _currentCorrectAnswers = 0;
    private int _currentBonus = 1;
    private float _currentTimer = 0f;
    private bool _isTimerRunning = false;
    private bool _waitingForFeedback = false;
    private float _lastSecondCheck = 0f;
    
    private int _correctAnswer;
    private int _selectedAnswerIndex = -1;
    
    // Eventos
    public event Action<int> OnChallengeCompleted;
    public event Action OnChallengeFailed;
    
    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        
        if (_challengePanel != null) _challengePanel.SetActive(false);
        if (_questionPanel != null) _questionPanel.SetActive(false);
        
        // NO ocultar los iconos aquí para evitar problemas de inicialización
        // Se ocultarán cuando se genere la primera pregunta
    }
    
    /// <summary>
    /// Inicia el reto de divisiones.
    /// </summary>
    public void StartChallenge()
    {
        _currentCorrectAnswers = 0;
        _currentBonus = 1;
        _currentTimer = 0f;
        
        // Guardar y ocultar otros elementos UI
        HideOtherUIElements();
        
        _challengePanel.SetActive(true);
        _questionPanel.SetActive(false);
        
        if (_introText != null)
        {
            _introText.text = "ES HORA DE RETO\n\nDebes responder 5 divisiones correctamente para activar el horno y comenzar a cocinar.";
            _introText.gameObject.SetActive(true);
        }
        
        UpdateProgressUI();
        UpdateBonusUI();
        
        StartCoroutine(StartQuestionsAfterDelay(5f));
    }
    
    /// <summary>
    /// Oculta los elementos UI especificados y guarda su estado previo.
    /// </summary>
    private void HideOtherUIElements()
    {
        _previousUIStates.Clear();
        
        if (_uiElementsToHide == null || _uiElementsToHide.Length == 0) return;
        
        foreach (GameObject uiElement in _uiElementsToHide)
        {
            if (uiElement != null)
            {
                // Guardar estado actual
                _previousUIStates[uiElement] = uiElement.activeSelf;
                // Ocultar
                uiElement.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Restaura el estado previo de los elementos UI.
    /// </summary>
    private void RestoreUIElements()
    {
        foreach (var kvp in _previousUIStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetActive(kvp.Value);
            }
        }
        
        _previousUIStates.Clear();
    }
    
    private IEnumerator StartQuestionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (_introText != null) _introText.gameObject.SetActive(false);
        _questionPanel.SetActive(true);
        
        // Ocultar los iconos de feedback antes de la primera pregunta
        HideFeedbackIcons();
        
        // Esperar un frame para asegurar que todo está inicializado
        yield return null;
        
        GenerateNewQuestion();
    }
    
    private void GenerateNewQuestion()
    {
        // Asegurarse de ocultar feedback de la pregunta anterior
        // usando Invoke para dar tiempo al sistema
        if (_currentCorrectAnswers > 0)
        {
            HideFeedbackIcons();
        }
        
        _waitingForFeedback = false;
        _selectedAnswerIndex = -1;
        
        if (_buttonAnimators != null)
        {
            foreach (ButtonAnimator animator in _buttonAnimators)
            {
                if (animator != null) animator.ResetAnimation();
            }
        }
        
        foreach (Button btn in _answerButtons)
        {
            btn.interactable = true;
        }
        
        int divisor = UnityEngine.Random.Range(2, 10);
        int quotient = UnityEngine.Random.Range(2, 10);
        int dividend = divisor * quotient;
        
        _correctAnswer = quotient;
        
        if (_questionText != null)
        {
            _questionText.text = $"{dividend} ÷ {divisor} = ?";
        }
        
        List<int> answers = new List<int> { _correctAnswer };
        
        while (answers.Count < 3)
        {
            int wrongAnswer = UnityEngine.Random.Range(1, 15);
            if (!answers.Contains(wrongAnswer))
            {
                answers.Add(wrongAnswer);
            }
        }
        
        ShuffleList(answers);
        
        for (int i = 0; i < _answerButtons.Length && i < answers.Count; i++)
        {
            int answerValue = answers[i];
            _answerTexts[i].text = answerValue.ToString();
            
            _answerButtons[i].onClick.RemoveAllListeners();
            int index = i;
            _answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answerValue));
        }
        
        _currentTimer = _timePerQuestion;
        _isTimerRunning = true;
        _lastSecondCheck = Mathf.CeilToInt(_currentTimer);
    }
    
    private void Update()
    {
        if (_isTimerRunning && !_waitingForFeedback)
        {
            _currentTimer -= Time.deltaTime;
            
            if (_timerText != null)
            {
                int secondsLeft = Mathf.CeilToInt(_currentTimer);
                _timerText.text = $"{secondsLeft}";
                
                if (_currentTimer > 0f)
                {
                    int currentSecond = Mathf.CeilToInt(_currentTimer);
                    
                    if (currentSecond != _lastSecondCheck)
                    {
                        _lastSecondCheck = currentSecond;
                        
                        PlaySound(_tickSound);
                        
                        if (_timerPulseEffect != null)
                        {
                            _timerPulseEffect.PulseTimer();
                        }
                    }
                    
                    if (_currentTimer <= 4f)
                    {
                        _timerText.color = Color.red;
                    }
                    else
                    {
                        _timerText.color = Color.yellow;
                    }
                }
            }
            
            if (_currentTimer <= 0f)
            {
                _isTimerRunning = false;
                OnTimeUp();
            }
        }
    }
    
    private void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (_waitingForFeedback) return;
        
        _isTimerRunning = false;
        _waitingForFeedback = true;
        _selectedAnswerIndex = buttonIndex;
        
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
    
    private void HandleCorrectAnswer(int buttonIndex)
    {
        if (_buttonAnimators != null && buttonIndex < _buttonAnimators.Length && _buttonAnimators[buttonIndex] != null)
        {
            _buttonAnimators[buttonIndex].AnimateCorrect();
        }
        
        PlaySound(_correctSound);
        ShowFeedbackIcon(buttonIndex, true);
        
        _currentCorrectAnswers++;
        _currentBonus++;
        
        UpdateProgressUI();
        UpdateBonusUI();
        
        if (_currentCorrectAnswers >= _totalQuestions)
        {
            StartCoroutine(CompleteChallenge());
        }
        else
        {
            StartCoroutine(NextQuestionAfterDelay(1.5f));
        }
    }
    
    private void HandleIncorrectAnswer(int buttonIndex)
    {
        if (_buttonAnimators != null && buttonIndex < _buttonAnimators.Length && _buttonAnimators[buttonIndex] != null)
        {
            _buttonAnimators[buttonIndex].AnimateIncorrect();
        }
        
        PlaySound(_incorrectSound);
        
        // Mostrar tacha en el botón incorrecto seleccionado
        ShowFeedbackIcon(buttonIndex, false);
        
        _currentBonus = 1;
        UpdateBonusUI();
        
        // Mostrar la respuesta correcta inmediatamente
        StartCoroutine(ShowCorrectAnswerAfterDelay(0.3f));
    }
    
    private IEnumerator ShowCorrectAnswerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
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
    
    private void OnTimeUp()
    {
        _waitingForFeedback = true;
        
        foreach (Button btn in _answerButtons)
        {
            btn.interactable = false;
        }
        
        _currentBonus = 1;
        UpdateBonusUI();
        
        StartCoroutine(ShowCorrectAnswerAfterDelay(0.5f));
    }
    
    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GenerateNewQuestion();
    }
    
    private IEnumerator CompleteChallenge()
    {
        yield return new WaitForSeconds(1.5f);
        
        PlaySound(_challengeCompleteSound);
        
        if (_introText != null)
        {
            _introText.gameObject.SetActive(true);
            _introText.text = $"RETO COMPLETADO \n\nBonus Final: x{_currentBonus}\n\n El horno está listo para cocinar";
        }
        
        _questionPanel.SetActive(false);
        
        yield return new WaitForSeconds(2.5f);
        
        // Restaurar UI antes de cerrar
        RestoreUIElements();
        
        _challengePanel.SetActive(false);
        OnChallengeCompleted?.Invoke(_currentBonus);
    }
    
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
    
    private void UpdateBonusUI()
    {
        if (_bonusText != null)
        {
            if (_currentBonus > 1)
            {
                _bonusText.gameObject.SetActive(true);
                _bonusText.text = $"x{_currentBonus} Bonus";
            }
            else
            {
                _bonusText.gameObject.SetActive(false);
            }
        }
    }
    
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