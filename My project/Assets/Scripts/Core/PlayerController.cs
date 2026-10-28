using UnityEngine;

/// <summary>
/// PlayerController - Базовый контроллер игрока.
/// Управляет движением и взаимодействием.
/// Сетевой: только клиент показывает свою позицию.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Input")]
    [SerializeField] private bool isNetworked = false;

    private Vector3 inputVector;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isNetworked)
            return; // Сетевой режим - управление через сеть

        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        inputVector = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputVector.magnitude > 0.01f)
        {
            // Движение вперёд
            transform.Translate(inputVector * moveSpeed * Time.deltaTime, Space.World);

            // Поворот в направлении движения
            float targetAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Сетевая функция движения (вызывается через RPC).
    /// </summary>
    public void NetworkMove(Vector3 direction)
    {
        Vector3 pos = transform.position;
        pos += direction * moveSpeed * Time.deltaTime;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация хитбокса
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
