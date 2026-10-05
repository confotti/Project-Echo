using System.Collections.Generic;
using UnityEngine;

public class CloneObject : MonoBehaviour
{
    public List<CloneFrame> recording = new List<CloneFrame>();

    private bool isRecording;
    private bool isReplaying;

    private float recordingTime;
    private int replayIndex;

    public void StartRecording()
    {
        recording.Clear();
        recordingTime = 0f;

        isRecording = true;
        isReplaying = false;
    }

    public void StopRecording()
    {
        isRecording = false;
    }

    public void StartReplay()
    {
        isReplaying = true;
        replayIndex = 0;
        recordingTime = 0f;
    }

    private void Update()
    {
        if (isRecording)
        {
            recordingTime += Time.unscaledDeltaTime;

            recording.Add(new CloneFrame(
                transform.position,
                transform.rotation,
                recordingTime
            ));
        }

        if (isReplaying)
        {
            ReplayRecording();
        }
    }

    private void ReplayRecording()
    {
        if (recording.Count < 2)
        {
            isReplaying = false;
            return;
        }

        recordingTime += Time.deltaTime;

        // Find the two recorded frames surrounding replay time.
        while (replayIndex < recording.Count - 2 &&
               recording[replayIndex + 1].time < recordingTime)
        {
            replayIndex++;
        }

        CloneFrame a = recording[replayIndex];
        CloneFrame b = recording[replayIndex + 1];

        float duration = b.time - a.time;

        float t = duration > 0f ? Mathf.Clamp01((recordingTime - a.time) / duration) : 1f;

        transform.position = Vector3.Lerp(a.position, b.position, t);
        transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

        if (recordingTime >= recording[recording.Count - 1].time)
        {
            transform.position = recording[recording.Count - 1].position;
            transform.rotation = recording[recording.Count - 1].rotation;
            isReplaying = false;

            gameObject.SetActive(false);
        }
    }
}



[System.Serializable]
public struct CloneFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public CloneFrame(Vector3 position, Quaternion rotation, float time)
    {
        this.position = position;
        this.rotation = rotation;
        this.time = time;
    }
}