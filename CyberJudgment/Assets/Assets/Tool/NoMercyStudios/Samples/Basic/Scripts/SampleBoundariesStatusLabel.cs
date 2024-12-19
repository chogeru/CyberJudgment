using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NoMercyStudios.BoundariesPro
{
    public class SampleBoundariesStatusLabel : MonoBehaviour
    {
        // UI - Text
        private Text text = null;
        public Text Text
        {
            get
            {
                if (this.text == null)
                    this.text = GetComponent<Text>();
                return this.text;
            }
        }

        // Target
        [SerializeField] private LevelBoundaryTrackedObject trackedObject = null;
        public LevelBoundaryTrackedObject TrackedObject
        {
            get
            {
                if (this.trackedObject == null)
                {
                    LevelBoundaryTrackedObject attachedTrackedObject = this.gameObject.GetComponent<LevelBoundaryTrackedObject>();
                    if (attachedTrackedObject == null)
                        attachedTrackedObject = this.gameObject.AddComponent<LevelBoundaryTrackedObject>();
                    return attachedTrackedObject;
                }
                return this.trackedObject;
            }
        }

        // Clear on start
        void Start()
        {
            if (this.Text != null)
                this.Text.text = string.Empty;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.Text == null)
                return;

            if (LevelBoundariesManager.Instance != null)
            {
                // Retrieve status
                bool isInside = LevelBoundariesManager.Instance.IsInsideAnyBoundary(this.TrackedObject);

                // Update text
                this.Text.text = "Status: " + (isInside == true ? "<color=green>Inside</color>" : "<color=red>Outside</color>");
            }
        }
    }

}