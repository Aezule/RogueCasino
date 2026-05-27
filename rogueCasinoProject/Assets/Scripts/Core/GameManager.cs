using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int playerCurrentHP;
    public int playerMaxHP;
    public bool hpInitialized = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}