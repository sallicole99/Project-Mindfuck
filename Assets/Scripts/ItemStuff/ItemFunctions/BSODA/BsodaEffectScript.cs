using UnityEngine;
using UnityEngine.AI;

public class BsodaEffectScript : MonoBehaviour
{
    #region Initialization
    private void Start() => agent = GetComponent<NavMeshAgent>();
    #endregion

    #region Per-Frame Logic
    private void Update()
    {
        if (inBsoda)
        {
            agent.velocity = otherVelocity;
        }

        if (failSave.CountdownWithDeltaTime() == 0)
        {
            inBsoda = false;
        }
    }
    #endregion

    #region Collision Detection
    private void OnTriggerEnter(Collider other) => HandleBsodaCollision(other);

    private void OnTriggerExit()
    {
        failSave = 0;
        inBsoda = false;
    }

    private void OnTriggerStay(Collider other) => HandleBsodaCollision(other);
    #endregion

    #region BSODA Effect Handling
    private void HandleBsodaCollision(Collider other)
    {
        if (other.CompareTag("BSODA"))
        {
            inBsoda = true;
            otherVelocity = other.GetComponent<Rigidbody>().velocity;
            failSave = 1;
        }
        else if (other.transform.name == "Gotta Sweep")
        {
            inBsoda = true;
            otherVelocity = 0.1f * agent.speed * transform.forward + other.GetComponent<NavMeshAgent>().velocity;
            failSave = 1;
        }
    }
    #endregion

    #region Internal State
    private NavMeshAgent agent;
    private Vector3 otherVelocity;
    private bool inBsoda;
    private float failSave;
    #endregion
}