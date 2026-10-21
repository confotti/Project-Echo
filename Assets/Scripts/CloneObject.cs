using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloneObject : MonoBehaviour
{
    public List<CloneFrame> recording = new List<CloneFrame>();

    public GameObject SwordCollider;
    public CameraFollow cameraFollow;

    private Transform ogCameraTarget;

    [Header("Color Transition")]
    [SerializeField] private ColorRestorationController colorController;

    public bool IsRecording => isRecording;
    public bool IsReplaying => isReplaying;
    public bool IsCloneActive => isRecording || isReplaying;

    private bool isRecording;
    private bool isReplaying;

    private float recordingTime;
    private int replayIndex;
    private PlayerMovement cloneMovement;

    private void Awake()
    {
        cloneMovement = GetComponent<PlayerMovement>();

        if (cameraFollow != null)
            ogCameraTarget = cameraFollow.target;
    }

    public void StartRecording()
    {
        recording.Clear();
        recordingTime = 0f;
        isRecording = true;
        isReplaying = false;
        SwordCollider.layer = 6;
        SwordCollider.tag = "Untagged";

        if (colorController != null)
        {
            colorController.enabled = true;
            colorController.EnterCloneMode();
        }

        if (cameraFollow != null)
            cameraFollow.target = transform;
    }

    public void StopRecording()
    {
        isRecording = false;

        if (colorController != null)
            colorController.ExitCloneMode();

        if (cameraFollow != null)
            cameraFollow.target = ogCameraTarget;
    }

    public void StartReplay()
    {
        isRecording = false;
        isReplaying = true;
        replayIndex = 0;
        recordingTime = 0f;
        SwordCollider.layer = 0;
        SwordCollider.tag = "Sword";

        cameraFollow.target = ogCameraTarget;
    }

    private void Update()
    {
        if (isReplaying)
            ReplayRecording();
    }

    private void LateUpdate()
    {
        if (isRecording && cloneMovement != null)
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
            FinishReplay();
            return;
        }

        recordingTime += Time.deltaTime;

        while (replayIndex < recording.Count - 2 && recording[replayIndex + 1].time < recordingTime)
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
            if (cloneMovement.dashEffect != null)
                cloneMovement.dashEffect.Play();

            recording[replayIndex] = new CloneFrame(a.position, a.rotation, a.time, a.attacked, false);
            recording[replayIndex + 1] = new CloneFrame(b.position, b.rotation, b.time, b.attacked, false);
        }

        if (recordingTime >= recording[recording.Count - 1].time)
        {
            transform.position = recording[recording.Count - 1].position;
            transform.rotation = recording[recording.Count - 1].rotation;

            FinishReplay();
        }
    }

    private void FinishReplay()
    {
        if (!isReplaying)
            return;

        isReplaying = false;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        isRecording = false;
        isReplaying = false;

        if (cameraFollow != null && ogCameraTarget != null)
            cameraFollow.target = ogCameraTarget;
    }

    public void KillFromWeapon()
    {
        isRecording = false;
        isReplaying = false;

        if (colorController != null)
            colorController.ExitCloneMode();

        if (cameraFollow != null && ogCameraTarget != null)
            cameraFollow.target = ogCameraTarget;

        gameObject.SetActive(false);
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