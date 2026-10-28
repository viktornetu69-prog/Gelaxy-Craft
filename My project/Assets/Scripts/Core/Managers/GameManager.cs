using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Главный менеджер игры.
/// Единственный экземпляр (Singleton).
/// Управляет состояниями игры и загрузкой сцен.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameLoaded { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Загружаем GameWorld если это не главное меню
        if (SceneManager.GetActiveScene().name != "00_MainMenu")
        {
            InitializeGame();
        }
    }

    public void InitializeGame()
    {
        Debug.Log("[GameManager] Initializing game...");
        IsGameLoaded = true;
    }

    public void LoadGameWorld()
    {
        Debug.Log("[GameManager] Loading GameWorld scene...");
        SceneManager.LoadScene("01_GameWorld");
    }

    public void LoadMainMenu()
    {
        Debug.Log("[GameManager] Loading Main Menu...");
        SceneManager.LoadScene("00_MainMenu");
    }
}
