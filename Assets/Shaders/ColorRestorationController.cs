using System;
using System.Collections;
using UnityEngine;

public class ColorRestorationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Color Restoration")]
    [SerializeField] private float colorRadius = 100f;

    [SerializeField] private float edgeWidth = 2f;
    [SerializeField] private float edgeIrregularity = 0.75f;
    [SerializeField] private float edgeFrequency = 5f;

    [Header("Edge Outline")]
    [SerializeField] private Color outlineColor = Color.black;

    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private float transitionDuration = 1.5f;

    //låter andra scripts se ifall detta är in progress
    public bool IsTransitioning { get; private set; }

    //låter andra scripts läsa av deras value
    public Transform Player => player;
    public float ColorRadius => currentRadius;
    public float EdgeWidth => edgeWidth;
    public float FullColorRadius => colorRadius;

    private float currentRadius;
    private Coroutine transitionRoutine;

    //låter c# sätta/uppdatera values i shadern
    //ColorSourceID = c# name / _ColorSource = shader name
    private static readonly int ColorSourceID = Shader.PropertyToID("_ColorSource");
    private static readonly int ColorRadiusID = Shader.PropertyToID("_ColorRadius");
    private static readonly int EdgeWidthID = Shader.PropertyToID("_EdgeWidth");
    private static readonly int ColorAmountID = Shader.PropertyToID("_ColorAmount");
    private static readonly int EdgeIrregularityID = Shader.PropertyToID("_EdgeIrregularity");
    private static readonly int EdgeFrequencyID = Shader.PropertyToID("_EdgeFrequency");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");

    //körs innan Start()
    private void Awake()
    {
        currentRadius = colorRadius;
        //sätter alla values från start
        ApplyShaderValues();
    }

    //körs efter Update(), låter dig reagera till ändringar som sker i Update()
    private void LateUpdate()
    {
        //Håller effekten alltid runt spelren
        ApplyShaderValues();
    }

    private void ApplyShaderValues()
    {
        //kollar om "Player" has been assigned
        if (player != null)
        {
            //läser player pos i world space
            Vector3 position = player.position;
            //skickar player pos till shadern som en vector4
            Shader.SetGlobalVector(ColorSourceID,new Vector4(position.x, position.y, position.z, 1f));
        }

        //skickar the values till the shader
        Shader.SetGlobalFloat(ColorRadiusID, currentRadius);
        Shader.SetGlobalFloat(EdgeWidthID, edgeWidth);

        //räknar ut hur mkt av restoration radius som är aktiv
        //clamp01 håller värdet mellan 0 - 1 (greyscale - full color)
        //? = if statement   : = else
        float colorAmount = colorRadius > 0f? Mathf.Clamp01(currentRadius / colorRadius): 1f;

        //skickar values till shadern
        Shader.SetGlobalFloat(ColorAmountID, colorAmount);
        Shader.SetGlobalFloat(EdgeIrregularityID,edgeIrregularity);
        Shader.SetGlobalFloat(EdgeFrequencyID,edgeFrequency);
        Shader.SetGlobalColor(OutlineColorID,outlineColor);
        Shader.SetGlobalFloat(OutlineWidthID,outlineWidth);
    }

    public void EnterCloneMode()
    {
        //gör radius = 0 när är i klon mode, genom en animation
        TransitionToRadius(0f);
    }

    //Action onComplete = null = låter andra scripts detecta när denna är klar
    public void ExitCloneMode(Action onComplete = null)
    {
        //återgår till maxvärde när i player mode
        TransitionToRadius(colorRadius, onComplete);
    }

    private void TransitionToRadius(float targetRadius,Action onComplete = null)
    {
        //om en transition redan spelas, stanna den innan en ny spelas
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        //berättar för andra scripts att detta just nu sker
        IsTransitioning = true;
        //startar en Coroutine som ändrar radius
        transitionRoutine = StartCoroutine(AnimateRadius(targetRadius, onComplete));
    }

    //IEnumerator låter dig pausa the execution mitt i, ist för att spela allting direkt
    private IEnumerator AnimateRadius(float targetRadius,Action onComplete)
    {
        float startRadius = currentRadius;
        float elapsed = 0f;

        //fortsätt animera tills tiden har nått sitt mål
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime; //berättar tid sen senaste frame, oberoende från frame rate
            float t = Mathf.Clamp01(elapsed / transitionDuration); //Sätter the value mellan 0 - 1. 0 = början, 1 = slutet
            t = Mathf.SmoothStep(0f, 1f, t); //ease the animation ist för att använda en konstant hastighet
            currentRadius = Mathf.Lerp(startRadius,targetRadius,t); //hoppar mellan start radius o target radius, as t increases = kommer närmare target radius

            //pausar Coroutine till nästa frame, låter animationen spela
            yield return null;
        }

        //sätter the target value direkt
        currentRadius = targetRadius;
        transitionRoutine = null; //klarar upp the Coroutine för anim är klar
        IsTransitioning = false; //låter andra script veta att transition är klar

        onComplete?.Invoke(); //låter andra scripts peform actions när denna är klar
    }
}