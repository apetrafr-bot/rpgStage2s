using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    public void OnRespawnButtonClick()
    {
        if (playerHealth.Instance != null)
            playerHealth.Instance.OnRespawnButtonClick();
    }
}
