using UnityEngine;

namespace Assets.Scripts.Controls.Inputs
{
    public abstract class AbstractInputManager : MonoBehaviour
    {
        /// <summary>
        /// Returns the input vector for looking around.
        /// </summary>
        public abstract Vector2 GetLookVector();

        /// <summary>
        /// Returns the input vector for moving around.
        /// </summary>
        public abstract Vector2 GetMoveVector();
        
        /// <summary>
        /// Returns true during the frame where the Jump button is pressed.
        /// </summary>
        public abstract bool JumpButtonDown();
    }
}