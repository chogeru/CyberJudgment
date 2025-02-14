using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneSwitcher : MonoBehaviour
{
    public string initialSceneName = "Title";

    public KeyCode keyToPress = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress) ||
           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            ResetApplication();
        }
    }

    void ResetApplication()
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.scene.buildIndex == -1)
            {
                Destroy(obj);
            }
        }

        SceneManager.LoadScene(initialSceneName, LoadSceneMode.Single);
    }
}
