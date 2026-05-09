using UnityEngine;

public class SwingingDoorScript : MonoBehaviour
{
    #region Initialization
    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
        bDoorLocked = true;
        normalInside = inside.material;
        normalOutside = outside.material;
    }
    #endregion

    #region UpdateLoopHandlers
    private void Update()
    {
        HandleUnlocking();
        HandleTimers();
        HandleDoorClosing();
    }

    private void HandleUnlocking()
    {
        if (bUnlockDoor && bDoorLocked)
        {
            SetLock(false);
            bUnlockDoor = false;
        }

        if (!requirementMet && gc.notebooks >= gc.UnlockAmount)
        {
            requirementMet = true;
            SetLock(false);
        }
    }

    private void HandleTimers()
    {
        if (lockTime.CountdownWithDeltaTime() == 0 & bDoorLocked & requirementMet)
        {
            SetLock(false);
        }
    }

    private void HandleDoorClosing()
    {
        if (openTime.CountdownWithDeltaTime() == 0 & bDoorOpen & !bDoorLocked)
        {
            SetDoorState(false);
        }
    }
    #endregion

    #region TriggerDetection
    private void OnTriggerStay(Collider opened)
    {
        if ((opened.CompareTag("Player") | (opened.CompareTag("NPC") & opened.isTrigger)) && !bDoorLocked)
        {
            SetDoorState(true);
            openTime = 2f;
        }
    }

    private void OnTriggerEnter(Collider open)
    {
        if (open.CompareTag("Player"))
        {
            HandlePlayerInteraction();
        }
        else if (open.CompareTag("NPC"))
        {
            HandleNPCInteraction();
        }
    }
    #endregion

    #region DoorInteraction
    public void LockDoor(float time)
    {
        SetLock(true);
        lockTime = time;
    }

    private void HandlePlayerInteraction()
    {
        if ((gc.notebooks <= gc.maxNotebooks || gc.mode == "endless") && !heardDoor && !bDoorLocked)
        {
            PlayDoorSound();
            baldi.Hear(transform.position, 1);
        }
    }

    private void HandleNPCInteraction()
    {
        if ((!myAudio.isPlaying || !bDoorOpen) && !bDoorLocked)
        {
            PlayDoorSound();
        }
    }

    private void PlayDoorSound() => myAudio.PlayOneShot(doorOpen);
    #endregion

    #region DoorState
    private void SetDoorState(bool set)
    {
        heardDoor = set;
        bDoorOpen = set;
        int shift = set ? 1 : 0;
        inside.material.SetInt("_Swap", shift);
        outside.material.SetInt("_Swap", shift);
    }

    private void SetLock(bool lockState)
    {
        barrier.enabled = lockState;
        obstacle.SetActive(lockState);
        bDoorLocked = lockState;

        inside.material = lockState ? lockedIn : normalInside;
        outside.material = lockState ? lockedOut : normalOutside;
    }
    #endregion

    #region SerializedFields
    [Header("References")]
    [SerializeField] private GameControllerScript gc;
    [SerializeField] private BaldiScript baldi;
    [SerializeField] private AudioClip doorOpen;

    [Header("Door Mechanics and Materials")]
    [SerializeField] private MeshCollider barrier;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private MeshRenderer inside, outside;
    [SerializeField] private Material lockedIn, lockedOut;
    [SerializeField] private bool bUnlockDoor;
    #endregion

    #region RuntimeState
    private bool requirementMet, bDoorOpen, bDoorLocked, heardDoor;
    private Material normalInside, normalOutside;
    private float lockTime, openTime;
    private AudioSource myAudio;
    #endregion
}