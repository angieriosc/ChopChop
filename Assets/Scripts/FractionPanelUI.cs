using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Genera botones de fracciones (1/2, 1/3, 1/4, 1/5, 1/6)
/// y divide el recipiente activo según la fracción seleccionada.
/// </summary>
public class FractionPanelUI : MonoBehaviour
{
    [Header("Prefab del botón de fracción")]
    [SerializeField] private GameObject buttonPrefab;

    [Header("Contenedor de botones en la UI")]
    [SerializeField] private Transform buttonContainer;

    [Header("Script que realiza la división")]
    [SerializeField] private IngredientDivider ingredientDivider;

    [Header("Script que lleva el conteo de tazas seleccionadas")]
    [SerializeField] private CupTracker cupTracker;

    // Lista con la etiqueta y el número de partes correspondientes
    private readonly List<(string label, int parts)> fractions =
        new() { ("1/2", 2), ("1/3", 3), ("1/4", 4), ("1/5", 5), ("1/6", 6) };

    void Start()
    {
        foreach (var (label, parts) in fractions)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = label;

            // Determinar fracción según el número de partes
            CupTracker.CupFraction fraction = parts switch
            {
                2 => CupTracker.CupFraction.Half,
                3 => CupTracker.CupFraction.Third,
                4 => CupTracker.CupFraction.Quarter,
                5 => CupTracker.CupFraction.Fifth,
                6 => CupTracker.CupFraction.Sixth,
                _ => CupTracker.CupFraction.Half
            };

            // Agregar listeners al botón
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                bool cupOk = cupTracker.SelectCup(fraction);
                if (cupOk)
                {
                    ingredientDivider.DivideIntoCups(parts);
                }
            });
        }
    }
}

