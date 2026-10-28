using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameNetClient - Клиент для Fish-Networking.
/// Управляет P2P сетевой симуляцией: физика, построение, спавн объектов.
/// Хост управляет состоянием мира, клиенты подключаются к хосту.
/// </summary>
public class GameNetClient : MonoBehaviour
{
    public static GameNetClient Instance { get; private set; }

    [Header("Network Settings")]
    [SerializeField] private string networkName = "GalaxySwitch";
    [SerializeField] private int maxPlayers = 100;

    [Header("Spawn Objects")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject worldObjectPrefab;

    public bool IsHost => _isHost;
    public bool IsClient => _isClient;
    public bool IsServer => _isHost || _isServer;
    public bool IsReady => _isReady;

    public int ConnectedPlayers => _connectedPlayers.Count;

    private bool _isHost;
    private bool _isClient;
    private bool _isServer;
    private bool _isReady;

    // Хранение подключённых игроков (только на хосте)
    private Dictionary<Guid, GameObject> _connectedPlayers = new();

    // Callbacks
    public event Action<bool> OnHostStatusChanged;
    public event Action<Guid, GameObject> OnPlayerConnected;
    public event Action<Guid> OnPlayerDisconnected;

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
    /// Создать хост (сервер).
    /// Хост управляет состоянием мира и физикой.
    /// </summary>
    public async Task<bool> CreateHostAsync()
    {
        Debug.Log("[GameNetClient] Creating host...");

        try
        {
            // В продакшене: инициализация Fish-Networking NetworkManager
            // Для прототипа - симуляция

            _isHost = true;
            _isServer = true;
            _isReady = true;

            Debug.Log($"[GameNetClient] Host created. Network: '{networkName}', Max players: {maxPlayers}");
            OnHostStatusChanged?.Invoke(true);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameNetClient] Host creation failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Подключиться как клиент к хосту.
    /// </summary>
    public async Task<bool> ConnectAsClientAsync(string hostAddress = "localhost")
    {
        Debug.Log($"[GameNetClient] Connecting as client to {hostAddress}...");

        try
        {
            // В продакшене: подключение через Fish-Networking
            // networkManager.Connect(hostAddress);

            _isClient = true;
            _isReady = true;

            Debug.Log($"[GameNetClient] Client connected to {hostAddress}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameNetClient] Client connection failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Спавн игрока в мире.
    /// </summary>
    public GameObject SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        var player = Instantiate(playerPrefab, position, rotation);
        var playerId = Guid.NewGuid();

        _connectedPlayers[playerId] = player;

        Debug.Log($"[GameNetClient] Player spawned at {position}. ID: {playerId}");
        OnPlayerConnected?.Invoke(playerId, player);

        return player;
    }

    /// <summary>
    /// Спавн объектов мира (здания, ресурсы и т.д.).
    /// </summary>
    public GameObject SpawnWorldObject(Vector3 position, Quaternion rotation = default)
    {
        if (worldObjectPrefab == null)
        {
            Debug.LogError("[GameNetClient] World object prefab is not assigned!");
            return null;
        }

        return Instantiate(worldObjectPrefab, position, rotation);
    }

    /// <summary>
    /// Удалить игрока из мира.
    /// </summary>
    public void RemovePlayer(Guid playerId)
    {
        if (_connectedPlayers.TryGetValue(playerId, out var player))
        {
            Destroy(player);
            _connectedPlayers.Remove(playerId);
            Debug.Log($"[GameNetClient] Player removed. ID: {playerId}");
            OnPlayerDisconnected?.Invoke(playerId);
        }
    }

    /// <summary>
    /// Получить список всех подключённых игроков.
    /// </summary>
    public List<KeyValuePair<Guid, GameObject>> GetConnectedPlayers()
    {
        return new List<KeyValuePair<Guid, GameObject>>(_connectedPlayers);
    }

    /// <summary>
    /// Остановить сетевую сессию.
    /// </summary>
    public void StopSession()
    {
        _isHost = false;
        _isClient = false;
        _isServer = false;
        _isReady = false;
        _connectedPlayers.Clear();

        Debug.Log("[GameNetClient] Session stopped.");
        OnHostStatusChanged?.Invoke(false);
    }

    private void OnApplicationQuit()
    {
        StopSession();
    }

    // Для отладки в Inspector
    private void OnDestroy()
    {
        StopSession();
    }
}
