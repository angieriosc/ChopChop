using System.Collections;
using UnityEngine;
using TMPro;

public class TimerPulseEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _pulseDuration = 0.2f;
    [SerializeField] private float _pulseScale = 1.15f;
    
    private Vector3 _originalScale;
    private Coroutine _pulseCoroutine;
    
    private void Awake()
    {
        if (_timerText == null)
            _timerText = GetComponent<TextMeshProUGUI>();
        
        _originalScale = _timerText.transform.localScale;
    }
    
    public void PulseTimer()
    {
        if (_pulseCoroutine != null)
            StopCoroutine(_pulseCoroutine);
        
        _pulseCoroutine = StartCoroutine(PulseAnimation());
    }
    
    private IEnumerator PulseAnimation()
    {
        Vector3 targetScale = _originalScale * _pulseScale;
        
        // Crecer
        float elapsed = 0f;
        while (elapsed < _pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (_pulseDuration / 2f);
            _timerText.transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
            yield return null;
        }
        
        // Reducir
        elapsed = 0f;
        while (elapsed < _pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (_pulseDuration / 2f);
            _timerText.transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
            yield return null;
        }
        
        _timerText.transform.localScale = _originalScale;
    }
}