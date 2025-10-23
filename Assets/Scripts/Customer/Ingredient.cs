using UnityEngine;

[System.Serializable]
public class Ingredient
{
    public string ingredientName;
    public Sprite ingredientIcon;
    public Sprite ingredientSlicedIcon;
    
    public Ingredient(string name, Sprite icon)
    {
        ingredientName = name;
        ingredientIcon = icon;
    }
}