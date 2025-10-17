using UnityEngine;

public class RecipeUITester : MonoBehaviour
{
    public RecipeUIManager recipeUIManager;
    public RecipeData testRecipe;
    public KeyCode testKey = KeyCode.T;
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log("🧪 === INICIANDO TEST DE UI ===");
            
            if (recipeUIManager == null)
            {
                Debug.LogError("❌ RecipeUIManager NO asignado!");
                return;
            }
            
            if (testRecipe == null)
            {
                Debug.LogError("❌ Test Recipe NO asignada!");
                return;
            }
            
            Debug.Log("✅ Mostrando receta de prueba: " + testRecipe.recipeName);
            recipeUIManager.DisplayRecipe(testRecipe);
        }
    }
}