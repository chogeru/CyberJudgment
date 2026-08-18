using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoMercyStudios.BoundariesPro.Samples.FPS
{
    public abstract class PlayerBaseInput : MonoBehaviour
    {
        [Header("Input - Movement")]
        [SerializeField] protected float horizontalAxis = 0f;
        [SerializeField] protected float verticalAxis = 0f;

        // Getters
        public virtual Vector2 MovementInput
        {
            get
            {
                return new Vector2(this.horizontalAxis, this.verticalAxis);
            }
            protected set
            {
                this.horizontalAxis = value.x;
                this.verticalAxis = value.y;
            }
        }


        [Header("Input - Look")]
        [SerializeField] protected float mouseX = 0f;
        [SerializeField] protected float mouseY = 0f;

        // Getters
        public virtual Vector2 RotationInput
        {
            get
            {
                return new Vector2(this.mouseX, this.mouseY);
            }
            protected set
            {
                this.mouseX = value.x;
                this.mouseY = value.y;
            }
        }


        [Header("Input - Actions")]
        // Jump
        [SerializeField] protected bool jump = false;
        public virtual bool Jump
        {
            get
            {
                return jump;
            }
        }

        // Shoot
        [SerializeField] protected bool shoot = false;
        public virtual bool Shoot
        {
            get
            {
                return shoot;
            }
        }

        // Reload
        [SerializeField] protected bool reload = false;
        public virtual bool Reload
        {
            get
            {
                return reload;
            }
        }

        // Run
        [SerializeField] protected bool run = false;
        public virtual bool Run
        {
            get
            {
                return run;
            }
        }
    }
}