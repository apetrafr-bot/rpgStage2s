using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public string sceneCible = "MainScene";

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            if (pausePanel != null)
                pausePanel.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    public void Reprendre()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChargerScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneCible);
    }

    public void QuitterJeu()
    {
        Application.Quit();
    }
}
