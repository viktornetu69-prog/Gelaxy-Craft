using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuUI - Главный экран меню игры.
/// Обрабатывает подключение к бэкенду и запуск сетевого режима.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button connectButton;
    [SerializeField] private Button createHostButton;
    [SerializeField] private Button connectClientButton;
    [SerializeField] private Text statusText;

    private void Start()
    {
        GameManager.Instance.InitializeGame();

        if (connectButton != null)
            connectButton.onClick.AddListener(OnConnectClicked);

        if (createHostButton != null)
            createHostButton.onClick.AddListener(OnCreateHostClicked);

        if (connectClientButton != null)
            connectClientButton.onClick.AddListener(OnConnectClientClicked);

        UpdateStatus("Ожидание подключения...");
    }

    private async void OnConnectClicked()
    {
        Debug.Log("[MainMenu] Connect to backend clicked.");
        UpdateStatus("Подключение к серверу...");

        var connected = await BackendClient.Instance.ConnectAsync();
        if (connected)
        {
            UpdateStatus($"Подключено! PlayerId: {BackendClient.Instance.CurrentPlayerId}");
        }
        else
        {
            UpdateStatus("Ошибка подключения. Проверьте сервер.");
        }
    }

    private async void OnCreateHostClicked()
    {
        Debug.Log("[MainMenu] Create host clicked.");

        // Сначала подключаемся к бэкенду
        var backendConnected = await BackendClient.Instance.ConnectAsync();
        if (!backendConnected)
        {
            UpdateStatus("Ошибка подключения к бэкенду!");
            return;
        }

        UpdateStatus("Создание хоста...");

        // Создаём хост
        var hostCreated = await GameNetClient.Instance.CreateHostAsync();
        if (hostCreated)
        {
            UpdateStatus("Хост создан! Игровой мир инициализирован.");

            // Присоединяемся к системе
            var joined = await BackendClient.Instance.JoinSystemAsync("main_system");
            if (joined)
            {
                UpdateStatus("Готово! Вы в галактике.");
                // Загружаем игровой мир
                await UnityEngine.AddressableAssets.Addressables.LoadSceneAsync("01_GameWorld");
            }
        }
    }

    private async void OnConnectClientClicked()
    {
        Debug.Log("[MainMenu] Connect as client clicked.");

        // Подключаемся к бэкенду
        var backendConnected = await BackendClient.Instance.ConnectAsync();
        if (!backendConnected)
        {
            UpdateStatus("Ошибка подключения к бэкенду!");
            return;
        }

        UpdateStatus("Подключение как клиент...");

        // Подключаемся как клиент
        var clientConnected = await GameNetClient.Instance.ConnectAsClientAsync();
        if (clientConnected)
        {
            UpdateStatus("Подключено как клиент!");

            // Присоединяемся к системе хоста
            await BackendClient.Instance.JoinSystemAsync("main_system");
            UpdateStatus("Готово! Вы в галактике как клиент.");
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"[MainMenuUI] {message}");
    }
}
