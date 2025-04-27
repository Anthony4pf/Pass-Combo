using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject
{
    public event UnityAction<Vector2> OnTapEvent = delegate { };

    private PlayerInputActions m_playerInputActions;

    private void OnEnable()
    {
        if (m_playerInputActions == null)
        {
            m_playerInputActions = new PlayerInputActions();
            // Subscribe to the performed event of the Pass action (button press)
            m_playerInputActions.Player.Pass.performed += OnTapPressed;
        }

        m_playerInputActions.Enable();
    }

    private void OnDisable()
    {
        m_playerInputActions.Disable();
        m_playerInputActions.Player.Pass.performed -= OnTapPressed;
    }

    private void OnTapPressed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition =  m_playerInputActions.Player.Position.ReadValue<Vector2>();//context.ReadValue<Vector2>();
        OnTapEvent.Invoke(screenPosition);
    }
}