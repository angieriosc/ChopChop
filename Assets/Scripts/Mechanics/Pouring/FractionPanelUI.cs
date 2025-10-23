using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Crea botones de fracciones (1/2, 1/3, 1/4, 1/5, 1/6)
/// y gestiona la división del recipiente activo según la fracción.
/// </summary>
public class FractionPanelUI : MonoBehaviour
{
    [Header("Prefab del botón de fracción")]
    [SerializeField] private GameObject buttonPrefab;

    [Header("Contenedor visual de los botones")]
    [SerializeField] private Transform buttonContainer;

    [Header("Script que realiza la división")]
    [SerializeField] private IngredientDivider ingredientDivider;

    [Header("Controlador de selección de tazas")]
    [SerializeField] private CupTracker cupTracker;

    /// <summary>
    /// Lista de fracciones disponibles y su número de partes.
    /// </summary>
    private readonly List<(string label, int parts)> fractions = new()
    {
        ("1/2", 2), ("1/3", 3), ("1/4", 4), ("1/5", 5), ("1/6", 6)
    };

    /// <summary>
    /// Genera dinámicamente los botones de fracción y asigna sus acciones.
    /// </summary>
    private void Start()
    {
        foreach (var (label, parts) in fractions)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = label;

            CupTracker.CupFraction fraction = parts switch
            {
                2 => CupTracker.CupFraction.Half,
                3 => CupTracker.CupFraction.Third,
                4 => CupTracker.CupFraction.Quarter,
                5 => CupTracker.CupFraction.Fifth,
                6 => CupTracker.CupFraction.Sixth,
                _ => CupTracker.CupFraction.Half
            };

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                bool isCupValid = cupTracker.SelectCup(fraction);
                if (isCupValid)
                    ingredientDivider.DivideIntoCups(parts);
            });
        }
    }
}
