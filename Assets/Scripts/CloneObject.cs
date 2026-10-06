using System.Collections.Generic;
using UnityEngine;

public class CloneObject : MonoBehaviour
{
    public List<CloneFrame> recording = new List<CloneFrame>();

    private bool isRecording;
    private bool isReplaying;

    private float recordingTime;
    private int replayIndex;

    private PlayerMovement cloneMovement;

    private void Awake()
    {
        cloneMovement = GetComponent<PlayerMovement>();
    }

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
        if (isReplaying)
        {
            ReplayRecording();
        }
    }

    private void LateUpdate()
    {
        if (isRecording)
        {
            recordingTime += Time.deltaTime;

            recording.Add(new CloneFrame(
                transform.position,
                transform.rotation,
                recordingTime,
                cloneMovement.attackFrame,
                cloneMovement.dashFrame
            ));
        }

        cloneMovement.attackFrame = false;
        cloneMovement.dashFrame = false;
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

        //Lite hemskt, men det får va så nu, om det funkar
        if (a.attacked || b.attacked)
        {
            cloneMovement.Attack();
            recording[replayIndex] = new CloneFrame(a.position, a.rotation, a.time, false, a.dashed);
            recording[replayIndex+1] = new CloneFrame(b.position, b.rotation, b.time, false, b.dashed);
        }

        if (a.dashed || b.dashed)
        {
            cloneMovement.dashEffect.Play();
            recording[replayIndex] = new CloneFrame(a.position, a.rotation, a.time, a.attacked, false);
            recording[replayIndex + 1] = new CloneFrame(b.position, b.rotation, b.time, b.attacked, false);
        }


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
    public bool attacked;
    public bool dashed;

    public CloneFrame(Vector3 position, Quaternion rotation, float time, bool attacked, bool dashed)
    {
        this.position = position;
        this.rotation = rotation;
        this.time = time;
        this.attacked = attacked;
        this.dashed = dashed;
    }
}