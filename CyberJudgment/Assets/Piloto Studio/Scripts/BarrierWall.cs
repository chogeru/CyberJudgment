using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[ExecuteAlways]
public class BarrierWall : MonoBehaviour
{

    public Transform pointA; // Extremo izquierdo de la pared
    public Transform pointB; // Extremo derecho de la pared
    [SerializeField] private ParticleSystem[] particleSystem;
    [SerializeField] private ParticleSystem particlesEmitter;
    [SerializeField] private ParticleSystem SecondparticlesEmitter;

    ParticleSystem.ShapeModule shape;
    ParticleSystem.ShapeModule Secondshape;

    Vector3 center;
    Vector3 direction;

    void OnEnable()
    {
        if (particleSystem != null)
            UpdateParticle();
    }

    void UpdateParticle()
    {
        shape = particlesEmitter.shape;
        if(SecondparticlesEmitter)
         Secondshape = SecondparticlesEmitter.shape;

        foreach (ParticleSystem ps in particleSystem)
        {
            if (pointA == null || pointB == null || ps == null) return;

            // Calcular la posición media entre los puntos
            center = (pointA.localPosition + pointB.localPosition) / 2 + new Vector3(0, 0.02f, 0);

            // Calcular la distancia entre los puntos (ancho de la pared)
            float width = Vector3.Distance(pointA.position, pointB.position);
            
            // Upadte Particle System
            var main = ps.main;
            main.startSizeX = width;  // Ancho (eje X)

            // Set Particle System position
            ps.transform.localPosition = center;

            direction = (pointB.localPosition - pointA.localPosition);

            // Rotar el sistema de partículas para que siga la dirección de los puntos
            ps.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        // Particle Emitter
        //shape.position = center;
        particlesEmitter.transform.localPosition = center;
        direction = (pointB.localPosition - pointA.localPosition);
        // Calcular la rotación para que el sistema de partículas se alinee con la línea
        shape.rotation = Quaternion.LookRotation(Vector3.up, direction).eulerAngles + new Vector3(0, 90f, 0);
        shape.radius = direction.magnitude / 2.2f;



        if (SecondparticlesEmitter != null)
        {
            SecondparticlesEmitter.transform.localPosition = center;
            //Secondshape.position = center;
            Secondshape.rotation = Quaternion.LookRotation(Vector3.up, direction).eulerAngles + new Vector3(0, 90f, 0);
            Secondshape.radius = direction.magnitude / 2.2f;
        }
    }

}
