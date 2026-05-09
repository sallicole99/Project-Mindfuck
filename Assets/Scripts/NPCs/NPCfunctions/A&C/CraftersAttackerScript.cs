using UnityEngine;
using System.Collections;

public class CraftersAttackerScript : MonoBehaviour
{
    public void Attack() => StartCoroutine(AttackPlayer());
    
	public IEnumerator AttackPlayer()
	{
		float speed = 350f;
		float acceleration = 25f;
		float spinDistance = 10f;
		Vector3 currentAngle = playerTransform.forward;
		float time = 0f;
        float echoDistance = 0f;
        Transform[] array = echos;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(true);
		}
		while (time < 15f)
		{
			currentAngle = Quaternion.AngleAxis(speed * Time.deltaTime, Vector3.up) * currentAngle;
			transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z) + currentAngle * spinDistance;
            for (int j = 0; j < echos.Length; j++)
			{
				Vector3 echoAngle = Quaternion.AngleAxis(echoDistance * j * -1f * Time.deltaTime, Vector3.up) * currentAngle;
				echos[j].transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z) + echoAngle * (spinDistance + j + 1f);
			}
			speed += acceleration * Time.deltaTime;
            echoDistance += 300f * Time.deltaTime;
			time += Time.deltaTime;
			yield return null;
		}
		crafters.SetActive(true);
        craftersScript.GiveConsequence();
		Destroy(gameObject);
		yield break;
	}

    [Header("References & Components")]
    public Transform playerTransform;
    public GameObject crafters;
    public CraftersScript craftersScript;

    [Header("Echoes & Positioning")]
	[SerializeField] private Transform[] echos;
}