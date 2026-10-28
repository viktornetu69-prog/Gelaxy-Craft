using UnityEngine;

/// <summary>
/// NetworkInitializer - Инициализирует все сетевые системы при запуске.
/// Размещается на объекте в сцене 00_MainMenu.
/// </summary>
public class NetworkInitializer : MonoBehaviour
{
    [Header("Auto Connect")]
    [SerializeField] private bool autoConnectBackend = true;

    private void Start()
    {
        Debug.Log("[NetworkInitializer] Initializing network subsystems...");

        // Проверяем доступность бэкенда
        if (autoConnectBackend)
        {
            StartCoroutine(ConnectToBackend());
        }
    }

    private System.Collections.IEnumerator ConnectToBackend()
    {
        Debug.Log("[NetworkInitializer] Checking backend availability...");

        // Ждём 1 секунду для инициализации
        yield return new WaitForSeconds(1f);

        BackendClient.Instance.ConnectAsync();

        Debug.Log("[NetworkInitializer] Backend client initialized.");
    }

    private void OnDestroy()
    {
        BackendClient.Instance?.Disconnect();
    }
}
