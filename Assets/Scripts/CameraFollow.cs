using UnityEngine;

public class CameraFollow: MonoBehaviour
{
    public Transform player; // Ссылка на объект игрока
    public Vector3 offset;   // Смещение камеры относительно игрока
    public float smoothSpeed = 0.125f; // Скорость сглаживания движения

    void LateUpdate()
    {
        // Целевая позиция камеры
        Vector3 targetPosition = player.position + offset;

        // Плавное движение камеры к целевой позиции
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        // Обновляем позицию камеры
        transform.position = smoothedPosition;

        // Фиксируем угол камеры, чтобы она не вращалась
        transform.rotation = Quaternion.Euler(35, -55, 0); // Задайте нужный угол
    }
}
