using UnityEngine;

[System.Serializable]
public class CustomFrameDuration
{
    public int frameIndex;      // Index of the frame
    public float duration;      // Custom duration for this frame
}

public class TextureChanger : MonoBehaviour
{
    public Texture[] textures; // Array of textures to cycle through
    public float frameRate = 1.0f; // Frame rate for texture changes
    public bool loopForever = true; // Whether to loop forever
    public CustomFrameDuration[] customFrameDurations; // Array of custom frame durations
    public int materialIndex = 0; // Index of the material to change textures on
    private Renderer[] childRenderers;
    private int currentTextureIndex = 0;
    private float timeSinceLastFrameChange = 0.0f;

    void Start()
    {
        // Get all child renderers
        childRenderers = GetComponentsInChildren<Renderer>();

        // Set the initial texture
        UpdateTextures();
    }

    void Update()
    {
        // If there are no textures or the frame rate is 0, exit
        if (textures == null || textures.Length == 0 || frameRate <= 0)
            return;

        // Increment time since last frame change
        timeSinceLastFrameChange += Time.deltaTime;

        // Calculate frame duration
        float frameDuration = GetFrameDuration();

        // Check if it's time to change to the next frame
        if (timeSinceLastFrameChange >= frameDuration)
        {
            // Change to the next texture
            currentTextureIndex++;

            // Check for looping
            if (currentTextureIndex >= textures.Length)
            {
                if (loopForever)
                {
                    currentTextureIndex = 0; // Loop back to the first texture
                }
                else
                {
                    currentTextureIndex = textures.Length - 1; // Stay at the last texture
                }
            }

            // Update the textures on all child renderers
            UpdateTextures();

            // Reset the timer
            timeSinceLastFrameChange = 0.0f;
        }
    }

    float GetFrameDuration()
    {
        // Check for custom duration
        foreach (CustomFrameDuration customFrameDuration in customFrameDurations)
        {
            if (customFrameDuration.frameIndex == currentTextureIndex)
            {
                return customFrameDuration.duration;
            }
        }

        // If no custom duration is set for the current frame, use the frame rate
        return 1.0f / frameRate;
    }

    void UpdateTextures()
    {
        // Apply the current texture to the specified material index on all child renderers
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null && childRenderers[i].materials.Length > materialIndex)
            {
                // Apply the texture to the specified material index
                childRenderers[i].materials[materialIndex].mainTexture = textures[currentTextureIndex];
            }
        }
    }
}
