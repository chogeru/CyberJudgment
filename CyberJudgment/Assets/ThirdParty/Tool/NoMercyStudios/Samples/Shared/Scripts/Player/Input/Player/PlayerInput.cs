using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro.Samples.FPS
{
    public class PlayerInput : PlayerBaseInput
    {
        // Start is called before the first frame update
        void Start()
        {
            // Lock and hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            // Standard input: Update keyboard & mouse
            this.UpdateKeyboardInput();
            this.UpdateMouseInput();

            // Toggle cursor lock
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            // Toggle cursor lock
            if (focus == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            //else
            //{
            //    Cursor.lockState = CursorLockMode.None;
            //    Cursor.visible = true;
            //}
        }

        // Keyboard - Input
        void UpdateKeyboardInput()
        {
            // Keyboard axis - Movement
            this.horizontalAxis = Input.GetAxis("Horizontal");
            this.verticalAxis = Input.GetAxis("Vertical");

            // Extra action: Jump
            this.jump = Input.GetKeyDown(KeyCode.Space);

            // Extra action: Reload
            this.reload = Input.GetKeyDown(KeyCode.R);

            // Extra action: Run
            this.run = Input.GetKey(KeyCode.LeftShift);
        }

        // Mouse - Input
        void UpdateMouseInput()
        {
            // Mouse axis - Rotation
            this.mouseX = Input.GetAxis("Mouse X");
            this.mouseY = Input.GetAxis("Mouse Y");

            // Extra action: Shoot
            this.shoot = Input.GetMouseButton(0);
        }

        //// VR - Input
        //void UpdateOpenXRInput()
        //{
        //    this.UpdateKeyboardInput();
        //}
    }
}