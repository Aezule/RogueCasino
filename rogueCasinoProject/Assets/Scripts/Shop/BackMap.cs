using UnityEngine;
using UnityEngine.SceneManagement;

public class BackMap : MonoBehaviour
{
    public void GoToMap()
    {
        SceneManager.LoadScene("Map");
    }
}