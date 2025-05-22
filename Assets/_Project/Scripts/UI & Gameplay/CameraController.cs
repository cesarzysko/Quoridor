using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float SPEED = 1f;

    [SerializeField] private Transform m_cameraObject;
    
    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector3 camPos = m_cameraObject.position;
        camPos += Time.deltaTime * SPEED * input;
        camPos.x = Mathf.Clamp(camPos.x, -Board.BOARD_BOUNDARY, Board.BOARD_BOUNDARY);
        camPos.y = Mathf.Clamp(camPos.y, -Board.BOARD_BOUNDARY, Board.BOARD_BOUNDARY);
        m_cameraObject.position = camPos;
    }
}
