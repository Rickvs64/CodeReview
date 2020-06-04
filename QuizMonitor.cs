using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuizMonitor : MonoBehaviour
{
    [SerializeField]
    TextMesh questionComponent;
    [SerializeField]
    List<TextMesh> answerComponents;
    [SerializeField]
    TextMesh scoreComponent;
    [SerializeField]
    AudioSource voiceSource;
    [SerializeField]
    AudioSource ambientSource;
    [SerializeField]
    AudioSource notificationSource;

    [SerializeField]
    Color standardColor = Color.white;
    [SerializeField]
    Color correctColor;
    [SerializeField]
    Color incorrectColor;

    [SerializeField]
    AudioClip correctSound;
    [SerializeField]
    AudioClip incorrectSound;
    [SerializeField]
    AudioClip victorySound;
    [SerializeField]
    AudioClip gameOverSound;

    [SerializeField]
    float ceilingLightsIntensity = 1.5f;            // All lights with this intensity will be selected and gradually turned off during the game-over transition.

    int selectedAnswer;
    List<Light> lights;

    // Configuration variables.
    float lightFadeDuration = 4.0f;                 // How long it takes for the lighting to fade out after a game over.

    // Start is called before the first frame update
    void Start()
    {
        Clear();
        lights = FindCeilingLightsInScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Simple method for determining an answer's letter prefix by its index.
    /// E.g. the 2nd answer in a list would have the character 'B'.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    char GetAnswerLetterByIndex(int i)
    {
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return alphabet[i];
    }

    /// <summary>
    /// Play the audio file from Resources.
    /// </summary>
    /// <param name="path"></param>
    void PlayAudioFromResources(string path)
    {
        AudioClip clip = (AudioClip)Resources.Load(path);
        if (!clip)
        {
            Debug.Log(String.Format($"Could not find audio file: {path}"));
            return;
        }
        voiceSource.clip = clip;
        voiceSource.Play();
        Debug.Log(String.Format($"Playing audio file: {path}"));
    }

    /// <summary>
    /// Stop the audio file from Resources.
    /// </summary>
    void StopAudioFromResources()
    {
        voiceSource.Stop();
    }

    /// <summary>
    /// Find all ceiling lights with the requested intensity.
    /// The intensity is used to find all correct lights.
    /// </summary>
    /// <returns></returns>
    List<Light> FindCeilingLightsInScene()
    {
        return FindObjectsOfType<Light>().Where(i => i.intensity == ceilingLightsIntensity).ToList();
    }

    /// <summary>
    /// Show the question, answers and duration on the screen.
    /// Only shows the question and plays audio - needs to be called AFTER having shown the relevant media clip if it exists.
    /// Called by QuizManager.
    /// </summary>
    /// <param name="q"></param>
    public void ShowQuestion(QuizQuestion q)
    {
        Clear();
        questionComponent.text = q.Question;
        for (int i = 0; i < q.Answers.Count; i++)
        {
            answerComponents[i].text = String.Format("{0}: {1}", GetAnswerLetterByIndex(i), q.Answers[i]);
        }

        PlayAudioFromResources(String.Format($"Quiz/Audio/{q.DetermineAudioName()}"));
        StartCoroutine(FadeAudioSource.StartFade(ambientSource, 1.0f, 1.0f));
    }

    /// <summary>
    /// Show the current question as confirming, indicating an answer has been submitted and is about to be validated.
    /// </summary>
    /// <param name="answerIndex"></param>
    public void ShowConfirming(int answerIndex)
    {
        selectedAnswer = answerIndex;

        questionComponent.text = "Het antwoord is...";

        string answer = answerComponents[selectedAnswer].text;
        answerComponents[selectedAnswer].text = String.Format($"[ {answer} ]");

        StopAudioFromResources();
        StartCoroutine(FadeAudioSource.StartFade(ambientSource, 1.0f, 0.5f));
    }

    /// <summary>
    /// Show the question as having been answered correctly.
    /// </summary>
    public void ShowAnswerAsCorrect()
    {
        questionComponent.text = "JUIST!";
        answerComponents[selectedAnswer].color = correctColor;

        notificationSource.PlayOneShot(correctSound);
    }

    /// <summary>
    /// Show the question as having been answered incorrectly.
    /// Also highlights the correct answer.
    /// </summary>
    /// <param name="correctIndex"></param>
    public void ShowAnswerAsIncorrect(int correctIndex)
    {
        questionComponent.text = "FOUT!";

        answerComponents[selectedAnswer].color = incorrectColor;

        string answer = answerComponents[correctIndex].text;
        answerComponents[correctIndex].text = String.Format($">>> {answer} <<<");
        answerComponents[correctIndex].color = correctColor;

        notificationSource.PlayOneShot(incorrectSound);
    }

    /// <summary>
    /// Update the displayed current score.
    /// </summary>
    /// <param name="score"></param>
    public void UpdateScore(int score)
    {
        scoreComponent.text = String.Format($"Score: {score}");
    }

    /// <summary>
    /// Clear the screen.
    /// </summary>
    public void Clear()
    {
        questionComponent.text = "";
        foreach (TextMesh a in answerComponents)
        {
            a.text = "";
            a.color = standardColor;
        }
    }

    /// <summary>
    /// Show that the quiz has ended and reveal the final score.
    /// </summary>
    /// <param name="victory">Whether the player won (no questions left) or lost (too many incorrect answers).</param>
    public void ShowGameEnded(bool victory, int score)
    {
        Clear();
        scoreComponent.text = "";

        StartCoroutine(FadeAudioSource.StartFade(ambientSource, 3.0f, 0.0f));

        questionComponent.text = String.Format($"Score: {score}");
        questionComponent.fontSize = 128;
        if (victory)
        {
            questionComponent.color = correctColor;
            notificationSource.PlayOneShot(victorySound);
        }
        else
        {
            questionComponent.color = incorrectColor;
            notificationSource.PlayOneShot(gameOverSound);
            StartCoroutine(GameOverSequence());
        }
    }

    /// <summary>
    /// Gradually dim the ceiling lights in the scene.
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOverSequence()
    {
        float currentTime = 0;
        float start = ceilingLightsIntensity;

        while (currentTime < lightFadeDuration)
        {
            currentTime += Time.deltaTime;
            foreach (Light l in lights)
            {
                l.intensity = Mathf.Lerp(start, 0.1f, currentTime / lightFadeDuration);
            }
            yield return null;
        }

        yield return null;
    }
}
