using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using TelePresent.AudioSyncPro;

namespace TelePresent.AudioSyncPro
{
    public class AnimationMarkerSync : EditorWindow
    {
        public ASP_MarkerProfile markerProfile;
        private AnimationClip newAnimationClip;
        public AnimationClip selectedAnimationClip;

        [MenuItem("Tools/TelePresent/ASP Animation Marker Sync")]
        public static void ShowWindow()
        {
            GetWindow<AnimationMarkerSync>("Animation Marker Sync");
        }

        private void OnGUI()
        {
            GUILayout.Label("Welcome to the Animation Marker Sync Tool!", EditorStyles.wordWrappedLabel);
            GUILayout.Label("This tool allows you to add audio markers from your marker profile to animation clips. You can create a new animation or add markers to an existing animation.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);  // Add some space before the rest of the UI

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);  //horizontal padding
            markerProfile = (ASP_MarkerProfile)EditorGUILayout.ObjectField("Marker Profile", markerProfile, typeof(ASP_MarkerProfile), false);
            GUILayout.Space(10);  // horizontal padding
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Button to create new animation
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);  // Add horizontal padding
            if (GUILayout.Button("Create New Animation", GUILayout.Width(200)))
            {
                if (markerProfile != null)
                {
                    CreateNewAnimation();
                }
                else
                {
                    Debug.LogError("Marker Profile is not assigned!");
                }
            }
            GUILayout.Space(10);  // horizontal padding
            GUILayout.EndHorizontal();

            GUILayout.Space(10); // space between buttons

            // Object picker for an existing animation clip
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);  //horizontal padding
            selectedAnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Selected Animation Clip", selectedAnimationClip, typeof(AnimationClip), false);
            GUILayout.Space(10);  //horizontal padding
            GUILayout.EndHorizontal();

            if (selectedAnimationClip != null)
            {
                GUILayout.Space(10); // space between UI elements
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);  //horizontal padding
                if (GUILayout.Button("Add Markers to selected Animation", GUILayout.Width(250)))
                {
                    AddMarkersToSelectedAnimation();
                }
                GUILayout.Space(10);  // horizontal padding
                GUILayout.EndHorizontal();
            }
        }

        private void CreateNewAnimation()
        {
            // Create a new animation clip
            newAnimationClip = new AnimationClip();
            newAnimationClip.name = markerProfile.audioClip != null ? markerProfile.audioClip.name + "_MarkersAnimation" : "MarkersAnimation";

            // Loop through each marker in the marker profile and create a keyframe for each
            foreach (var marker in markerProfile.markerList)
            {
                // Create a keyframe at the marker's time
                CreateMarkerKeyframe(newAnimationClip, marker);
            }

            // Save the new animation clip as an asset in the project
            string path = EditorUtility.SaveFilePanelInProject("Save Animation", newAnimationClip.name, "anim", "Specify where to save the new animation.");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newAnimationClip, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"New animation created and saved at {path}");
            }
        }

        private void AddMarkersToSelectedAnimation()
        {
            if (selectedAnimationClip == null)
            {
                Debug.LogError("No animation clip selected.");
                return;
            }

            // Add keyframes to the selected animation clip
            foreach (var marker in markerProfile.markerList)
            {
                // Add a keyframe at the marker's time
                CreateMarkerKeyframe(selectedAnimationClip, marker);
            }

            // Save the changes to the selected animation clip
            EditorUtility.SetDirty(selectedAnimationClip);
            AssetDatabase.SaveAssets();
            Debug.Log($"Markers added to the selected animation: {selectedAnimationClip.name}");
        }

        private void CreateMarkerKeyframe(AnimationClip animationClip, ASP_Marker marker)
        {
            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                type = typeof(Transform), // Customize this based on what you are animating
                path = "",                 // Path to the animated object (can be adjusted)
                propertyName = "m_LocalPosition.x" // Example property (you can customize this)
            };

            // Get the existing curve or create a new one
            AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, curveBinding);
            if (curve == null)
            {
                curve = new AnimationCurve();
            }

            // Add a keyframe at the time defined by the marker
            Keyframe keyframe = new Keyframe(marker.Time, 1.0f); // The value of the keyframe can be adjusted
            curve.AddKey(keyframe);

            // Set the updated curve in the animation clip at the appropriate path and property
            AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);

            // Ensure the animation clip is updated
            EditorUtility.SetDirty(animationClip);
        }
    }
}