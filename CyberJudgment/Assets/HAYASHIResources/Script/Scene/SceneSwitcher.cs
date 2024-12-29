using UnityEngine;
using UnityEngine.SceneManagement;
using AbubuResouse.Singleton;
using UnityEngine.InputSystem;

public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName;

    public KeyCode keyToPress = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress) || Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Application.Quit();
        }
    }
}
