using UnityEngine;

[CreateAssetMenu(fileName = "PizzaRecipe", menuName = "ChopChop/Pizza Recipe", order = 0)]
public class PizzaRecipe : ScriptableObject
{
    public PizzaToppingManager.ToppingEntry[] entries;
}
