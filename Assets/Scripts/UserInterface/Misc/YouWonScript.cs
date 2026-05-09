using UnityEngine;
using UnityEngine.SceneManagement;

public class YouWonScript : MonoBehaviour
{
    private void Start()
    {
        greatJob = GetComponent<AudioSource>();
        greatJob.ignoreListenerPause = true;
    }

    private void Update()
    {
        if (!updateExecuted)
        {
            delay -= Time.deltaTime;

            if (delay <= 0f)
            {
                SceneManager.LoadScene(MainArea);
                updateExecuted = true;
                AudioListener.pause = false;
            }
        }
    }

    [Header("Scene Transition Settings")]
    [SerializeField] private string MainArea;
    
    private float delay = 10f;
    private AudioSource greatJob;
	private bool updateExecuted = false;
}
