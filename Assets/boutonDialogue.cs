using UnityEngine;

/// <summary>
/// A placer sur le bouton "Suivant" du dialogue.
/// Appelle GameManager.PassDialogue() via la reference statique,
/// ce qui fonctionne dans toutes les scenes.
/// </summary>
public class boutonDialogue : MonoBehaviour
{
    public void PasserDialogue()
    {
        GameManager.PassDialogue();
    }
}
