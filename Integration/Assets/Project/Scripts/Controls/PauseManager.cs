using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Project.Scripts.Controls
{
    public class PauseManager : MonoBehaviour
    {
        private float _timeScale;

        public bool Paused { get; private set; }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            if (Paused)
            {
                // unpause
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = _timeScale;
            }
            else
            {
                // pause
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _timeScale = Time.timeScale;
                Time.timeScale = 0;
            }

            Paused = !Paused;
        }

        void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}