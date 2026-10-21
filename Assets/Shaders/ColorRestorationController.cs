using System;
using System.Collections;
using UnityEngine;

public class ColorRestorationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Color Restoration")]
    [Min(0f)]
    [SerializeField] private float colorRadius = 100f;

    [Min(0.001f)]
    [SerializeField] private float edgeWidth = 2f;

    public bool IsTransitioning { get; private set; } 

    [Header("Transition")]
    [Min(0.01f)]
    [SerializeField] private float transitionDuration = 1.5f;

    public Transform Player => player;
    public float ColorRadius => currentRadius;
    public float EdgeWidth => edgeWidth;
    public float FullColorRadius => colorRadius;

    private float currentRadius;
    private Coroutine transitionRoutine;

    private static readonly int ColorSourceID = Shader.PropertyToID("_ColorSource");
    private static readonly int ColorRadiusID = Shader.PropertyToID("_ColorRadius");
    private static readonly int EdgeWidthID = Shader.PropertyToID("_EdgeWidth");
    private static readonly int ColorAmountID = Shader.PropertyToID("_ColorAmount");

    private void Awake()
    {
        currentRadius = colorRadius;
        ApplyShaderValues();
    }

    private void LateUpdate()
    {
        ApplyShaderValues();
    }

    private void ApplyShaderValues()
    {
        if (player != null)
        {
            Vector3 position = player.position;
            Shader.SetGlobalVector(ColorSourceID,new Vector4(position.x, position.y, position.z, 1f));
        }

        Shader.SetGlobalFloat(ColorRadiusID, currentRadius);
        Shader.SetGlobalFloat(EdgeWidthID, edgeWidth);

        float colorAmount = colorRadius > 0f? Mathf.Clamp01(currentRadius / colorRadius): 1f;

        Shader.SetGlobalFloat(ColorAmountID, colorAmount);
    }

    public void EnterCloneMode()
    {
        TransitionToRadius(0f);
    }

    public void ExitCloneMode(Action onComplete = null)
    {
        TransitionToRadius(colorRadius, onComplete);
    }

    private void TransitionToRadius(float targetRadius,Action onComplete = null)
    {
        if (transitionRoutine != null)StopCoroutine(transitionRoutine);

        IsTransitioning = true;
        transitionRoutine = StartCoroutine(AnimateRadius(targetRadius, onComplete));
    } 

    private IEnumerator AnimateRadius(float targetRadius,Action onComplete)
    {
        float startRadius = currentRadius;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            t = Mathf.SmoothStep(0f, 1f, t);
            currentRadius = Mathf.Lerp(startRadius,targetRadius,t);

            yield return null;
        }

        currentRadius = targetRadius;
        transitionRoutine = null;
        IsTransitioning = false;

        onComplete?.Invoke();
    }
}