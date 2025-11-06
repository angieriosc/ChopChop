using UnityEngine;

/// <summary>
/// Contiene las referencias necesarias para identificar la raíz de la pizza,
/// su collider de superficie y el centro para cálculo de radio o placement.
/// </summary>
public class PizzaIdentifiers : MonoBehaviour
{
    [SerializeField] private Transform _pizzaRoot;
    [SerializeField] private Collider _pizzaSurface;
    [SerializeField] private Transform _pizzaCenter;

    public Transform PizzaRoot => _pizzaRoot;
    public Collider PizzaSurface => _pizzaSurface;
    public Transform PizzaCenter => _pizzaCenter;
}
