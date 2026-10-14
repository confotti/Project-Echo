using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EnableVolume : MonoBehaviour
{
    public Volume volume;

    public GameObject object1;
    public GameObject object2;

    [Header("Slow Motion")]
    public float slowTimeScale = 0.25f;
    public float slowDuration = 0.4f;
    public float returnDuration = 0.5f;

    private SplitToning splitToning;
    private bool sequenceStarted;

    private void Start()
    {
        if (volume != null && volume.profile.TryGet(out splitToning))
        {
            splitToning.active = false;
        }
    }

    private void Update()
    {
        bool object1Dead = object1 == null || !object1.activeInHierarchy;
        bool object2Dead = object2 == null || !object2.activeInHierarchy;

        if (object1Dead && object2Dead && !sequenceStarted)
        {
            sequenceStarted = true;
            StartCoroutine(LastKillEffect());
        }
    }

    private IEnumerator LastKillEffect()
    {
        // Enable the visual effect
        if (splitToning != null)
        {
            splitToning.active = true;
        }

        // Slow down time
        Time.timeScale = slowTimeScale;

        // Wait in slowed time
        yield return new WaitForSecondsRealtime(slowDuration);

        // Smoothly return to normal speed
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            Time.timeScale = Mathf.Lerp(
                slowTimeScale,
                1f,
                Mathf.Clamp01(elapsed / returnDuration)
            );

            yield return null;
        }

        Time.timeScale = 1f;
    }
}