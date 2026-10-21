using UnityEngine;

public class ColorRestorationEdgeVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ColorRestorationController colorController;
    [SerializeField] private ParticleSystem edgeParticles;

    [Header("Edge")]
    [SerializeField] private float heightOffset = 0.15f;
    [SerializeField] private float radiusMultiplier = 1.05f;

    [Header("Particle Movement")]
    [SerializeField] private float outwardSpeed = 40f;
    [SerializeField] private float inwardSpeed = -40f;

    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.MainModule main;
    private bool initialized;
    private bool wasTransitioning;
    private float previousRadius;

    private void Awake()
    {
        if (edgeParticles == null)
            edgeParticles = GetComponent<ParticleSystem>();

        if (edgeParticles == null)
            return;

        shape = edgeParticles.shape;
        main = edgeParticles.main;

        transform.localScale = Vector3.one;

        var emission = edgeParticles.emission;
        emission.enabled = false;

        edgeParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

        if (colorController != null)
            previousRadius = colorController.ColorRadius;

        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized || colorController == null || colorController.Player == null)
            return;

        Transform player = colorController.Player;

        transform.position = player.position + Vector3.up * heightOffset;
        transform.rotation = Quaternion.identity;

        float currentRadius = colorController.ColorRadius;

        shape.radius = currentRadius * radiusMultiplier;
        shape.rotation = new Vector3(90f, 0f, 0f);

        bool isTransitioning = colorController.IsTransitioning;

        if (isTransitioning)
        {
            float radiusChange = currentRadius - previousRadius;

            if (Mathf.Abs(radiusChange) > 0.0001f)
            {
                main.startSpeed = radiusChange > 0f? outwardSpeed: inwardSpeed;
            }
        }

        var emission = edgeParticles.emission;
        emission.enabled = isTransitioning;

        if (isTransitioning && !wasTransitioning)
        {
            edgeParticles.Play();
        }
        else if (!isTransitioning && wasTransitioning)
        {
            edgeParticles.Stop(true,ParticleSystemStopBehavior.StopEmitting);
        }

        previousRadius = currentRadius;
        wasTransitioning = isTransitioning;
    }
}