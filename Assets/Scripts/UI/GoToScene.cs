using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{

    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
}
