using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour
{
    #region Initialization
    private void Start() => myAudio = GetComponent<AudioSource>();
    #endregion

    #region UpdateLoopHandlers
    private void Update()
    {
        HandleLockTimer();
        HandleOpenTimer();
        HandleDoorInteraction();
    }

    private void HandleLockTimer()
    {
        if (lockTime.CountdownWithDeltaTime(1.25f) == 0 & bDoorLocked)
        {
            UnlockDoor();
        }
    }

    private void HandleOpenTimer()
    {
        if (openTime.CountdownWithDeltaTime() == 0 & bDoorOpen)
        {
            CloseDoor();
        }
    }

    private void HandleDoorInteraction()
    {
        if ((Input.GetMouseButtonDown(0) | Singleton<InputManager>.Instance.GetActionKey(InputAction.Interact)) && trigger.ScreenRaycastMatchesCollider(out _, 15) && Time.timeScale != 0f)
        {
            if (bDoorLocked)
            {
                myAudio.PlayOneShot(GameC.aud_Rattling);
            }
            else
            {
                if (!bDoorOpen)
                {
                    baldi?.Hear(transform.position, 1);
                }
                OpenDoor(3);
            }
        }
    }
    #endregion

    #region DoorStateManagement
    public void OpenDoor(float time)
    {
        if (!bDoorOpen)
        {
            myAudio.PlayOneShot(doorOpen);
        }
        SetDoorState(true, time);
    }

    private void CloseDoor()
    {
        SetDoorState(false);
        myAudio.PlayOneShot(doorClose);
    }

    private void SetDoorState(bool open, float time = 3)
    {
        barrier.enabled = !open;
        invisibleBarrier.enabled = !open;
        secondBarrier.enabled = !open;
        bDoorOpen = open;

        int shift = open ? 1 : 0;
        inside.material.SetInt("_Swap", shift);
        outside.material.SetInt("_Swap", shift);

        if (time != 0)
        {
            openTime = time;
        }
    }
    #endregion

    #region LockingMechanics
    public void LockDoor(float time)
    {
        myAudio.PlayOneShot(Click);
        bDoorLocked = true;
        lockTime = time;
    }

    public void UnlockDoor()
    {
        bDoorLocked = false;
        myAudio.PlayOneShot(GameC.aud_Unlocked);
    }

    public bool DoorLocked => bDoorLocked;
    #endregion

    #region CollisionHandlers
    private void OnTriggerStay(Collider OPEN)
    {
        if (!bDoorLocked & OPEN.CompareTag("NPC") & !Check)
        {
            OpenDoor(3);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!(prin.angry || other.transform.name != "Principal of the Thing"))
        {
            HandlePrincipalInteraction();
        }

        if (bDoorLocked && other.transform.name == "Baldi")
        {
            OpenDoor(3);
        }
    }

    private void HandlePrincipalInteraction()
    {
        if (Faculty)
        {
            if (FacultyTimesTwo)
            {
                if (!bDoorOpen)
                {
                    StartCoroutine(FacultyDoor());
                }
            }
            else
            {
                if (prin.onFaculty)
                {
                    prin.onFaculty = false;
                }
                else if (!bDoorOpen)
                {
                    StartCoroutine(FacultyDoor());
                }
            }
        }

        if (bDoorLocked)
        {
            OpenDoor(0.18f);
        }
    }

    public IEnumerator FacultyDoor()
    {
        myAudio.PlayOneShot(KnockKnock, 1f);
        Check = true;
        StartCoroutine(prin.CheckTheDoor());
        yield return new WaitForSeconds(2);
        Check = false;
    }
    #endregion

    #region SerializedConfig
    [Header("Audio Settings")]
    [SerializeField] private AudioClip doorOpen;
    [SerializeField] private AudioClip doorClose, KnockKnock, Click;

    [Header("Barrier Settings")]
    [SerializeField] private MeshCollider barrier;
    [SerializeField] private MeshCollider secondBarrier, trigger, invisibleBarrier;

    [Header("Renderer Settings")]
    [SerializeField] private MeshRenderer inside;
    [SerializeField] private MeshRenderer outside;

    [Header("Door Behavior Settings")]
    [SerializeField] private GameControllerScript GameC;
    [SerializeField] private BaldiScript baldi;
    [SerializeField] private PrincipalScript prin;
    [SerializeField] private bool Faculty, FacultyTimesTwo;
    #endregion

    #region RuntimeState
    private AudioSource myAudio;
    private bool bDoorOpen, bDoorLocked, Check;
    [HideInInspector] public float lockTime, openTime;
    #endregion
}