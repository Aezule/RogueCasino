using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Nettoyer l'état sauvegardé pour générer une nouvelle map
        MapState.ClearSavedState();
        if (Inventory.Instance != null)
        {
            Inventory.Instance.ResetTarots();

        }
        SceneManager.LoadScene("Map");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QuitGame");
    }
}