using UnityEngine;

public class loadScene : MonoBehaviour
{

    public void OnSceneLoad(string nameScene)
    { // Load the scene with the specified name
            UnityEngine.SceneManagement.SceneManager.LoadScene(nameScene);

    }
    // methode qui fait quitter le jeu
        public void OnQuitGame()
        {
            Application.Quit();
    }
}

