using UnityEngine;
using UnityEngine.UI;

public class SliderColorChanger : MonoBehaviour
{
    [Header("UI References")]
    public Slider slider;           // Assign your UI Slider
    public Image fillImage;         // Assign the Fill Area > Fill image

    [Header("Animated Icons")]
    public Animator angryIcon;      // Assign the Animator of the Angry icon
    public Animator happyIcon;      // Assign the Animator of the Happy icon

    [Header("Emotion Thresholds")]
    [Range(0f, 1f)] public float hurryUpThreshold = 0.15f;
    [Range(0f, 1f)] public float angryThreshold = 0.25f;
    [Range(0f, 1f)] public float happyThreshold = 0.8f;

    private bool isHurryUpActive = false;

    private void Start()
    {
        if (slider != null)
        {
            // Default start at 0.5
            slider.value = 1f;
            UpdateUI(slider.value);

            // Listen for changes
            slider.onValueChanged.AddListener(UpdateUI);
        }
    }

    private void UpdateUI(float value)
    {
        // --- Color transition ---
        if (fillImage != null)
        {
            Color color;
            if (value < 0.5f)
                color = Color.Lerp(Color.red, Color.yellow, value / 0.5f);
            else
                color = Color.Lerp(Color.yellow, Color.green, (value - 0.5f) / 0.5f);

            fillImage.color = color;
        }

        // --- Icon animation logic ---
        if (angryIcon != null && happyIcon != null)
        {
            // HURRY UP MODE
            if (value <= hurryUpThreshold)
            {
                // Activate HurryUp animation
                if (!isHurryUpActive)
                {
                    angryIcon.SetTrigger("HurryUp"); // make sure your Animator has a "HurryUp" trigger
                    isHurryUpActive = true;
                }

                angryIcon.speed = 1.5f; // faster animation speed
                happyIcon.speed = 0f;
            }
            // NORMAL ANGRY
            else if (value <= angryThreshold)
            {
                angryIcon.speed = 1f;
                happyIcon.speed = 0f;
                isHurryUpActive = false;
            }
            // HAPPY
            else if (value >= happyThreshold)
            {
                happyIcon.speed = 1f;
                angryIcon.speed = 0f;
                isHurryUpActive = false;
            }
            // NEUTRAL
            else
            {
                angryIcon.speed = 0f;
                happyIcon.speed = 0f;
                isHurryUpActive = false;
            }
        }
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(UpdateUI);
    }
}