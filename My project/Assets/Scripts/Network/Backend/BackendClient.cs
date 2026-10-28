using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// BackendClient - Клиент для подключения к SignalR серверу.
/// Отвечает за бэкенд-коммуникацию: регистрацию, соединение с системой.
/// НЕ управляет игровой физикой - только маршрутизация.
/// </summary>
public class BackendClient : MonoBehaviour
{
    public static BackendClient Instance { get; private set; }

    [Header("Connection Settings")]
    [SerializeField] private string serverUrl = "http://localhost:5000";
    [SerializeField] private string hubPath = "/galaxyhub";

    public string ServerUrl => serverUrl;
    public string ConnectionId { get; private set; }
    public bool IsConnected => _isConnected;
    public string CurrentPlayerId { get; private set; }
    public string CurrentSystemId { get; private set; }

    private bool _isConnected;
    private bool _isConnecting;

    // Callbacks
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string, string> OnPlayerJoinedSystem;
    public event Action<string> OnPlayerLeftSystem;
    public event Action<string, string, object> OnGameEventReceived;

    // SignalR через Unity HTTP (упрощённая реализация)
    private List<string> _systemConnections = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Подключиться к SignalR серверу.
    /// В продакшене использовать real SignalR Unity client.
    /// Для прототипа - HTTP ping для проверки доступности.
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        if (_isConnected)
        {
            Debug.Log("[BackendClient] Already connected.");
            return true;
        }

        if (_isConnecting)
        {
            Debug.LogWarning("[BackendClient] Connection already in progress.");
            return false;
        }

        _isConnecting = true;
        Debug.Log($"[BackendClient] Connecting to {serverUrl}...");

        try
        {
            // Проверяем доступность сервера
            using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
            {
                request.timeout = 5;
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[BackendClient] Connection failed: {request.error}");
                    Debug.LogError("[BackendClient] Make sure Backend server is running on " + serverUrl);
                    _isConnecting = false;
                    return false;
                }
            }

            Debug.Log("[BackendClient] Server is reachable.");

            // Генерируем уникальный PlayerId
            CurrentPlayerId = $"player_{Guid.NewGuid():N}";
            ConnectionId = Guid.NewGuid().ToString("N");

            _isConnected = true;
            _isConnecting = false;

            Debug.Log($"[BackendClient] Connected! PlayerId: {CurrentPlayerId}");
            OnConnected?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BackendClient] Connection error: {ex.Message}");
            _isConnected = false;
            _isConnecting = false;
            return false;
        }
    }

    /// <summary>
    /// Присоединиться к системе (галлактической системе).
    /// </summary>
    public async Task<bool> JoinSystemAsync(string systemId)
    {
        if (!_isConnected)
        {
            Debug.LogError("[BackendClient] Not connected to server.");
            return false;
        }

        CurrentSystemId = systemId;
        Debug.Log($"[BackendClient] Joining system: {systemId}");

        try
        {
            // Отправляем запрос на присоединение к системе
            using (UnityWebRequest request = new UnityWebRequest(
                $"{serverUrl}{hubPath}/join", "POST"))
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    playerId = CurrentPlayerId,
                    systemId = systemId
                });

                var uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
                uploadHandler.contentType = "application/json";
                request.uploadHandler = uploadHandler;

                var downloadHandler = new DownloadHandlerBuffer();
                request.downloadHandler = downloadHandler;

                request.timeout = 10;
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[BackendClient] JoinSystem failed: {request.error}");
                    return false;
                }

                Debug.Log($"[BackendClient] Successfully joined system: {systemId}");
                OnPlayerJoinedSystem?.Invoke(CurrentPlayerId, systemId);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BackendClient] JoinSystem error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Покинуть текущую систему.
    /// </summary>
    public async Task LeaveSystemAsync()
    {
        if (!_isConnected || string.IsNullOrEmpty(CurrentSystemId))
            return;

        Debug.Log($"[BackendClient] Leaving system: {CurrentSystemId}");

        try
        {
            using (UnityWebRequest request = new UnityWebRequest(
                $"{serverUrl}{hubPath}/leave", "POST"))
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    playerId = CurrentPlayerId
                });

                var uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
                uploadHandler.contentType = "application/json";
                request.uploadHandler = uploadHandler;

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();
            }

            CurrentSystemId = null;
            OnPlayerLeftSystem?.Invoke(CurrentPlayerId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BackendClient] LeaveSystem error: {ex.Message}");
        }
    }

    /// <summary>
    /// Отключиться от сервера.
    /// </summary>
    public void Disconnect()
    {
        _isConnected = false;
        CurrentPlayerId = null;
        CurrentSystemId = null;
        ConnectionId = null;

        Debug.Log("[BackendClient] Disconnected.");
        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// Проверить статус сервера.
    /// </summary>
    public async Task<bool> CheckServerStatusAsync()
    {
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Head(serverUrl))
            {
                request.timeout = 3;
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                return !request.result;
            }
        }
        catch
        {
            return false;
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
