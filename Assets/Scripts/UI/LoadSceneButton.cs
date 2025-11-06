using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    public string sceneName; 
    // Call this function from the button OnClick event
    public void changeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
