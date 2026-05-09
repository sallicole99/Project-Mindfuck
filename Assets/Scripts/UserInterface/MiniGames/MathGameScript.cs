using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MathGameScript : MonoBehaviour
{
    #region Callbacks
    private void Start() => InitializeGame();

    private void Update()
    {
        HandleAudioFeedback();
        HandleInput();

        if (problem > 3)
        {
            HandleGameEnd();
        }
    }
    #endregion

    #region Initialization
    private void InitializeGame()
    {
        gc = FindObjectOfType<GameControllerScript>();
        lg = FindObjectOfType<LearningGameManager>();
        baldiScript = gc.baldiScrpt;

        specialCodes = new Dictionary<string, Action>
        {
            { "31718", () => { StartCoroutine(CheatText("THIS IS WHERE IT ALL BEGAN")); SceneManager.LoadSceneAsync("TestRoom"); } },
            { "53045009", () => { StartCoroutine(CheatText("USE THESE TO STICK TO THE CEILING!")); gc.Fliparoo(); } },
            { "11211994", () => { StartCoroutine(CheatText("EPILEPSY WARNING! HIGHLY UNSTABILITY!")); GameControllerScript.ChaosMode = !GameControllerScript.ChaosMode; OhGodNo = true; problem = 4; lg.learnMusic.pitch = 4.3f; baldiAudio.audioDevice.pitch = 1.5f; baldiAudio.audioDevice.gameObject.AddComponent<AudioDistortionFilter>(); baldiAudio.audioDevice.GetComponent<AudioDistortionFilter>().distortionLevel = 0.75f; baldiAudio.audioDevice.PlayOneShot(bal_screech); baldiAudio.audioDevice.PlayOneShot(bal_screech); baldiAudio.audioDevice.PlayOneShot(bal_screech); baldiAudio.audioDevice.PlayOneShot(bal_screech);} }
        };

        baldiAudio.audioDevice.ignoreListenerPause = true;
        lg.learnMusic.ignoreListenerPause = true;
        if (!gc.spoopMode)
        {
            gc.schoolMusic.ignoreListenerPause = true;
        }

        endDelay = gc.spoopMode ? 1f : 5f;
        lg.ActivateLearningGame();

        if (gc.notebooks == 1)
        {
            baldiAudio.QueueAudio(bal_intro);
            baldiAudio.QueueAudio(bal_howto);
        }

        if (gc.spoopMode)
        {
            BlackCoverUp.SetActive(true);
            baldiFeedTransform.gameObject.SetActive(false);
            baldiFeed.enabled = false;
        }

        NewProblem();
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        if (Input.anyKeyDown && Input.inputString.Length > 0 && char.IsNumber(Input.inputString, 0))
        {
            ButtonPress(int.Parse(Input.inputString[0].ToString()));
            UpdateInputText();
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ButtonPress(-1);
            UpdateInputText();
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (context.Length > 0)
            {
                context = context.Substring(0, context.Length - 1);
            }
            else if (negative)
            {
                negative = false;
            }
            UpdateInputText();
        }
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && questionInProgress)
        {
            questionInProgress = false;
            CheckAnswer();
        }
    }
    #endregion

    #region Audio Feedback
    private void HandleAudioFeedback()
    {
        if (!baldiAudio.audioDevice.isPlaying) return;
        
        UpdateAudioFeedback();
    }

    private void UpdateAudioFeedback()
    {
        baldiAudio.audioDevice.GetOutputData(clipSampleData, 0);
        float clipLoudness = 0f;

        foreach (float sample in clipSampleData)
        {
            clipLoudness += Mathf.Abs(sample) * baldiAudio.audioDevice.volume;
        }

        int spriteIndex = Mathf.RoundToInt(Mathf.Clamp(clipLoudness * 2f, 0f, 6f));
        baldiFeed.sprite = talkSprites[spriteIndex];
    }
    #endregion

    #region Problem Generation
    private void NewProblem()
    {
        ResetProblemUI();

        if (problem <= 3)
        {
            GenerateMathProblem();
        }
        else
        {
            HandleProblemCompletion();
        }
    }

    private void ResetProblemUI()
    {
        playerAnswer.text = string.Empty;
        problem++;
        playerAnswer.ActivateInputField();
        questionInProgress = true;
    }

    private void GenerateMathProblem()
    {
        if (!gc.spoopMode)
        {
            StartCoroutine(PlayClassicMusic());
        }

        baldiAudio.QueueAudio(bal_problems[problem - 1]);

        if ((gc.mode == "endless" && gc.notebooks == 2 && problem == 3 && !impossibleQuestionShown) || (gc.mode == "story" && gc.notebooks > 1 && problem == 3))
        {
            GenerateImpossibleProblem();
            impossibleQuestionShown = true;
        }
        else
        {
            GenerateSimpleMathProblem();
        }
    }

    private void GenerateSimpleMathProblem()
    {
        num1 = UnityEngine.Random.Range(0, 10);
        num2 = UnityEngine.Random.Range(0, 10);
        sign = UnityEngine.Random.Range(0, 2);

        baldiAudio.QueueAudio(bal_numbers[Mathf.RoundToInt(num1)]);
        solution = sign == 0 ? num1 + num2 : num1 - num2;

        string signText = sign == 0 ? "+" : "-";
        questionText.text = $"Solve Math Q{problem}: \n \n{num1}{signText}{num2}=?";
        baldiAudio.QueueAudio(sign == 0 ? bal_plus : bal_minus);
        baldiAudio.QueueAudio(bal_numbers[Mathf.RoundToInt(num2)]);
        baldiAudio.QueueAudio(bal_equals);
    }

    private void GenerateImpossibleProblem()
    {
        impossibleMode = true;
        questionText.text = $"Solve Math Q{problem}: \n";

        num1 = UnityEngine.Random.Range(1f, 9999f);
        num2 = UnityEngine.Random.Range(1f, 9999f);
        num3 = UnityEngine.Random.Range(1f, 9999f);
        sign = Mathf.RoundToInt(UnityEngine.Random.Range(0, 1));

        string baseQuestion = sign == 0 ? $"{num1} + ({num2} × {num3} = ?" : $"({num1} ÷ {num2}) + {num3} =?";
        questionText2.text = "\n" + ApplyGlitchEffect(baseQuestion);
        questionText3.text = "\n" + ApplyGlitchEffect(baseQuestion);

        baldiAudio.QueueAudio(bal_screech);
        baldiAudio.QueueAudio(bal_times);
        baldiAudio.QueueAudio(bal_screech);
        baldiAudio.QueueAudio(bal_divided);
        baldiAudio.QueueAudio(bal_screech);
        baldiAudio.QueueAudio(bal_equals);
    }

    private string ApplyGlitchEffect(string text)
    {
        string[] glitchChars = { "!", "@", "#", "$", "%", "^", "&", "*", "?", "~", "_", ">", "/", "<", "|", "`" };
        System.Text.StringBuilder glitchyText = new System.Text.StringBuilder();

        foreach (char c in text)
        {
            if (UnityEngine.Random.value > 0.8f)
            {
                glitchyText.Append(glitchChars[UnityEngine.Random.Range(0, glitchChars.Length)]);
            }
            else
            {
                glitchyText.Append(c);
            }
        }

        return glitchyText.ToString();
    }
    #endregion

    #region Answer Evaluation
    public void OKButton() => CheckAnswer();

    public void CheckAnswer()
    {
        if (CheckSpecialCodes(playerAnswer.text)) return;

        if (problem <= 3)
        {
            if (IsCorrectAnswer())
            {
                HandleCorrectAnswer();
            }
            else
            {
                HandleIncorrectAnswer();
            }
        }

        ResetInputState();
    }

    private bool CheckSpecialCodes(string answer)
    {
        if (specialCodes.TryGetValue(answer, out Action action))
        {
            action.Invoke();
            return false;
        }

        return false;
    }

    private bool IsCorrectAnswer() => playerAnswer.text == solution.ToString() && !impossibleMode;

    private void HandleCorrectAnswer()
    {
        results[problem - 1].sprite = correct;

        if (!OhGodNo)
        {
            baldiAudio.audioDevice.Stop();
            baldiAudio.ClearQueue();
        }

        int praiseIndex = UnityEngine.Random.Range(0, bal_praises.Length);
        baldiAudio.QueueAudio(bal_praises[praiseIndex]);

        NewProblem();
    }

    private void HandleIncorrectAnswer()
    {
        problemsWrong++;
        results[problem - 1].sprite = incorrect;

        if (!gc.spoopMode && !OhGodNo)
        {
            StartCoroutine(PlayAnimation(baldiFeed, angrySprites, 0.15f, 0));
            gc.ActivateSpoopMode();
        }

        HandleBaldiAnger();
        if (!OhGodNo)
        {
            baldiAudio.ClearQueue();
            baldiAudio.audioDevice.Stop();
        }
        NewProblem();
    }

    private void HandleBaldiAnger()
    {
        if (gc.mode == "story")
        {
            if (problem == 3)
            {
                baldiScript.GetAngry(1.16f);
            }
            else
            {
                baldiScript.GetTempAngry(0.25f);
            }
        }
        else
        {
            baldiScript.GetAngry(1f);
        }
    }

    private void ResetInputState()
    {
        minusButton.interactable = true;
        context = string.Empty;
        negative = false;
    }
    #endregion

    #region Game End Handling
    private void HandleProblemCompletion()
    {
        if (!gc.spoopMode)
        {
            questionText.text = resultText;
        }
        else
        {
            ProvideHintOrFeedback();
        }
    }

    private void ProvideHintOrFeedback()
    {
        if (gc.mode == "endless" && problemsWrong <= 0)
        {
            questionText.text = endlessHintText[UnityEngine.Random.Range(0, endlessHintText.Length)];
        }
        else if (gc.mode == "story" && problemsWrong >= 3)
        {
            questionText.text = failedText;
            questionText2.text = questionText3.text = string.Empty;

            if (baldiScript.isActiveAndEnabled)
            {
                baldiScript.Hear(playerPosition, 7f);
            }

            gc.failedNotebooks++;
        }
        else
        {
            questionText.text = hintText[UnityEngine.Random.Range(0, hintText.Length)];
            questionText2.text = questionText3.text = string.Empty;
        }
    }

    private void HandleGameEnd()
    {
        endDelay -= Time.unscaledDeltaTime;

        if (endDelay <= 0f)
        {
            GC.Collect();
            if (OhGodNo)
			{
				AudioListener.pause = false;
				SceneManager.LoadScene("MainMenu");
				return;
			}
            ExitGame();
        }
    }

    private void ExitGame()
    {
        if (problemsWrong <= 0 && gc.mode == "endless")
        {
            baldiScript.GetAngry(-1f);
        }

        lg.DeactivateLearningGame(gameObject);
    }
    #endregion

    #region Utility Methods
    private IEnumerator CheatText(string text)
    {
        while (true)
        {
            questionText.text = text;
            questionText2.text = string.Empty;
            questionText3.text = string.Empty;
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator PlayClassicMusic()
    {
        int musicIndex = problem - 1;
        if (musicIndex < 0 || musicIndex >= learnMusics.Length)
        {
            yield break;
        }

        if (musicIndex >= 1)
        {
            lg.learnMusic.loop = false;
            yield return new WaitWhile(() => lg.learnMusic.isPlaying);
        }

        if (!gc.spoopMode)
        {
            lg.learnMusic.loop = true;
            lg.learnMusic.clip = learnMusics[musicIndex];
            lg.learnMusic.Play();
        }
        else
        {
            lg.learnMusic.Stop();
        }
    }

    public void ButtonPress(int value)
    {
        if (value >= 0)
        {
            if (context.Length < 9)
            {
                context += value.ToString();
                UpdateInputText();
                return;
            }
        }
        else
        {
            negative = !negative;
            UpdateInputText();
        }
    }

    private IEnumerator PlayAnimation(Image img, Sprite[] sprites, float speed, int ID)
    {
        isPlayingAnimation = true;
        animsPlayed[ID] = false;

        for (int i = 0; i < sprites.Length - 1; i++)
        {
            img.sprite = sprites[i];
            yield return new WaitForSecondsRealtime(speed);
        }

        img.sprite = sprites[sprites.Length - 1];
        animsPlayed[ID] = true;
    }

    private void UpdateInputText()
    {
        if (negative)
        {
            playerAnswer.text = "-" + context;
            return;
        }
        playerAnswer.text = context;
    }

    public void ClearSubmission()
    {
        negative = false;
        context = string.Empty;
        playerAnswer.text = string.Empty;
    }
    #endregion

    #region Serialized Fields
    [Header("Game References")]
    public GameControllerScript gc;
    public LearningGameManager lg;
    public BaldiScript baldiScript;

    [Header("UI Elements")]
    [SerializeField] private Image[] results = new Image[3];
    [SerializeField] private TMP_InputField playerAnswer;
    [SerializeField] private TMP_Text questionText, questionText2, questionText3;
    [SerializeField] private Image baldiFeed;
    [SerializeField] private Transform baldiFeedTransform;
    [SerializeField] private GameObject BlackCoverUp;
    [SerializeField] private Button minusButton;
    [SerializeField] private string[] hintText = { "I GET ANGRIER FOR EVERY PROBLEM YOU GET WRONG", "I HEAR EVERY DOOR YOU OPEN" }, endlessHintText = { "That's more like it...", "Keep up the good work or see me after class..." };
    [SerializeField] private string resultText = "WOW! YOU EXIST!", failedText = "I HEAR MATH THAT BAD";

    [Header("Audio Clips")]
    [SerializeField] private AudioQueueScript baldiAudio;
    [SerializeField] private AudioClip bal_intro, bal_howto, bal_plus, bal_minus, bal_times, bal_divided, bal_equals, bal_screech;
    [SerializeField] private AudioClip[] bal_numbers, bal_praises, bal_problems, learnMusics;

    [Header("Sprites")]
    [SerializeField] private Sprite[] talkSprites;
    [SerializeField] private Sprite[] angrySprites;
    [SerializeField] private Sprite correct, incorrect;
    #endregion

    #region Internal State
    private string context = string.Empty;
    private float num1, num2, num3, solution;
    private bool questionInProgress, impossibleMode, negative, OhGodNo;
    private bool impossibleQuestionShown;
    private const int SampleDataLength = 64;
    private float endDelay;
    private int problem, problemsWrong, sign;
    private float[] clipSampleData = new float[SampleDataLength];
    private Dictionary<string, Action> specialCodes;
    [HideInInspector] public Vector3 playerPosition;
    private bool[] animsPlayed = new bool[3];
    [HideInInspector] public bool isPlayingAnimation = false;
    #endregion
}