using UnityEngine;

public class CameraFollow: MonoBehaviour
{
    public Transform player; // ������ �� ������ ������
    public Vector3 offset;   // �������� ������ ������������ ������
    public float smoothSpeed = 0.125f; // �������� ����������� ��������

    void LateUpdate()
    {
        // ������� ������� ������
        Vector3 targetPosition = player.position + offset;

        // ������� �������� ������ � ������� �������
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        // ��������� ������� ������
        transform.position = smoothedPosition;

        // ��������� ���� ������, ����� ��� �� ���������
        transform.rotation = Quaternion.Euler(35, -55, 0); // ������� ������ ����
    }
}
