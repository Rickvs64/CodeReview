using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;


enum QuizState
{
    Asleep,
    Initializing,
    ShowingScenario,
    AwaitingAnswer,
    ShowingResult,
    EndingGame
}
public class QuizManager : MonoBehaviour
{
    [SerializeField]
    List<QuizButton> buttons;
    [SerializeField]
    QuizMediaPlayer mediaPlayer;
    [SerializeField]
    QuizMonitor monitor;
    [SerializeField]
    QuizLivesIndicator livesIndicator;
    [SerializeField]
    List<QuizScenario> scenarios;

    [SerializeField]
    SceneChanges sceneChanges;

    List<QuizQuestion> remainingQuestions;
    List<QuizQuestion> answeredCorrectly;
    List<QuizQuestion> answeredIncorrectly;

    QuizQuestion currentQuestion;
    QuizScenario currentScenario;
    float score = 500;
    QuizState state = QuizState.Asleep;

    // Configuration variables.
    readonly float delayBeforeRevealAnswer = 1.0f;          // Delay after submitting your answer and before revealing the correct one.
    readonly float delayBeforeAward = 0.2f;                 // Delay after revealing the correct answer and before awarding points.
    readonly float delayBeforeNextQuestion = 0.7f;          // Delay after awarding points (or not) and before loading the next question.
    readonly float delayBeforeLoadMenu = 5.0f;              // Delay after revealing final score and loading the main menu.
    readonly float scoreDrainPerSecond = 5.0f;              // Amount of points drained per second, encouraging the player to answer as fast as possible.
    readonly int maxIncorrect = 2;                          // Maximum allowed incorrect answers. Another incorrect one after this results in a game over. Default is 2.


    // Start is called before the first frame update.
    void Start()
    {
        // TODO: Add a start button to the scene so the player can prepare and then start the quiz manually.
        // For now this method is called automatically after 5s.
        StartCoroutine(DelayStartQuiz(5.0f));
    }

    // Update is called once per frame.
    void Update()
    {
        DrainScore();
    }

    /// <summary>
    /// TEMPORARY method for starting the quiz after a delay.
    /// </summary>
    /// <param name="delay">Delay in seconds.</param>
    /// <returns></returns>
    IEnumerator DelayStartQuiz(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartQuiz();
        yield return null;
    }

    /// <summary>
    /// Boot up the quiz, loading questions and showing the first one.
    /// </summary>
    void StartQuiz()
    {
        state = QuizState.Initializing;
        answeredCorrectly = new List<QuizQuestion>();
        answeredIncorrectly = new List<QuizQuestion>();

        LoadQuestions();

        StartNextQuestion();
    }

    /// <summary>
    /// Load questions into memory.
    /// </summary>
    void LoadQuestions()
    {
        remainingQuestions = LoadQuestionsFromFile("Quiz/Questions");
        remainingQuestions.Shuffle();
    }

    /// <summary>
    /// Load the next (random, non-duplicate) question.
    /// </summary>
    void StartNextQuestion()
    {
        // Load a random remaining question as the current question.
        int i = UnityEngine.Random.Range(0, remainingQuestions.Count - 1);
        currentQuestion = remainingQuestions[i];
        remainingQuestions.RemoveAt(i);

        // TODO: Show the question's media clip if it contains one, wait for it to complete before asking the question and allowing input.
        // TODO: Set a timer for when time runs out.
        if (currentQuestion.Scenario == "")
        {
            // Immediately show the question and answers, awaiting user input.
            AwaitAnswer();
        }
        else
        {
            // Load and handle the supplied scenario first, THEN show the question and answers.
            monitor.Clear();
            LoadScenario(currentQuestion.Scenario);
        }
    }

    /// <summary>
    /// Start waiting for user input.
    /// </summary>
    void AwaitAnswer()
    {
        ShowQuestionAndAnswers(currentQuestion);
        state = QuizState.AwaitingAnswer;
    }

    /// <summary>
    /// Load the scenario that belongs to this quiz question.
    /// Won't continue the quiz until this scenario calls EndScenario() on its own.
    /// </summary>
    /// <param name="scenario">Scenario name to look for. Has to match with an entry from the serializable scenarios property.</param>
    void LoadScenario(String scenario)
    {
        state = QuizState.ShowingScenario;

        currentScenario = FindScenarioByName(scenario);
        currentScenario.Load();
    }

    /// <summary>
    /// Find a scenario that matches with the supplied name.
    /// </summary>
    /// <param name="name">Name to look for.</param>
    /// <returns>The scenario with a matching name or null if no matching scenario was found.</returns>
    QuizScenario FindScenarioByName(string name)
    {
        foreach (QuizScenario qs in scenarios)
        {
            if (qs.Name == name)
                return qs;
        }
        Debug.Log("Error in QuizManager.FindScenarioByName(): No matching scenario found!");
        return null;
    }

    /// <summary>
    /// Show the question and its (shuffled) answers in the world for the player to see.
    /// </summary>
    /// <param name="q">Question object, also contains the answers.</param>
    void ShowQuestionAndAnswers(QuizQuestion q)
    {
        monitor.ShowQuestion(q);
        // TODO: Disable some buttons visually if the question has fewer answers than four.

        Debug.Log("QUESTION:");
        Debug.Log("Q: " + q.Question);
        foreach (string a in q.Answers)
        {
            Debug.Log("A: " + a);
        }
    }

    /// <summary>
    /// Load the desired list of questions from a locally stored file.
    /// </summary>
    /// <param name="path">Path to file containing questions.</param>
    /// <returns></returns>
    List<QuizQuestion> LoadQuestionsFromFile(string path)
    {
        TextAsset file = (TextAsset)Resources.Load(path);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(file.text);

        List<QuizQuestion> questions = new List<QuizQuestion>();

        XmlNodeList QuestionNodeList;
        XmlNode root = doc.DocumentElement;

        QuestionNodeList = root.SelectNodes("descendant::QuizQuestion");

        foreach (XmlNode q in QuestionNodeList)
        {
            int score = int.Parse(q.SelectSingleNode("descendant::Score").InnerText);
            string question = q.SelectSingleNode("descendant::Question").InnerText;
            float duration = float.Parse(q.SelectSingleNode("descendant::Duration").InnerText);
            string scenario = q.SelectSingleNode("descendant::Scenario").InnerText;

            List<string> answers = new List<string>();
            XmlNode ans = q.SelectSingleNode("descendant::Answers");
            XmlNodeList answerNodeList = ans.SelectNodes("descendant::Answer");
            foreach (XmlNode a in answerNodeList)
            {
                answers.Add(a.InnerText);
            }

            questions.Add(new QuizQuestion(score, question, answers, duration, scenario));
        }

        return questions;
    }

    /// <summary>
    /// Check whether the supplied answer index matches the question's correct answer.
    /// </summary>
    /// <param name="a">Index of your answer, corresponds to QuizQuestion's answers list.</param>
    /// <param name="q">Original question containing all the possible answers and the correct one.</param>
    /// <returns></returns>
    bool IsCorrectAnswer(int a, QuizQuestion q)
    {
        return (q.Answers[a] == q.CorrectAnswer);
    }

    /// <summary>
    /// After submitting your answer, wait for a short duration before revealing the result.
    /// </summary>
    /// <param name="yourAnswer">The index of your submitted answer.</param>
    /// <returns></returns>
    IEnumerator WaitRevealAnswer(int yourAnswer)
    {
        monitor.ShowConfirming(yourAnswer);
        Debug.Log("Answer submitted. Now waiting before showing result.");
        Debug.Log("Drum roll, please...");

        yield return new WaitForSeconds(delayBeforeRevealAnswer);

        if (IsCorrectAnswer(yourAnswer, currentQuestion))
        {
            answeredCorrectly.Add(currentQuestion);
            ShowResultAsCorrect(yourAnswer);
            StartCoroutine(WaitUpdateScoreAndNextQuestion(true));
        }
        else
        {
            answeredIncorrectly.Add(currentQuestion);
            ShowResultAsIncorrect(yourAnswer, currentQuestion.Answers.IndexOf(currentQuestion.CorrectAnswer));
            StartCoroutine(WaitUpdateScoreAndNextQuestion(false));
        }
        yield return null;
    }

    /// <summary>
    /// Show the result as correctly answered in the scene.
    /// </summary>
    /// <param name="yourAnswer">The index of your submitted answer.</param>
    void ShowResultAsCorrect(int yourAnswer)
    {
        monitor.ShowAnswerAsCorrect();
        livesIndicator.ShowLives((maxIncorrect + 1) - answeredIncorrectly.Count);
        Debug.Log("Answered correctly!");
    }

    /// <summary>
    /// Show the result as incorrectly answered in the scene.
    /// </summary>
    /// <param name="yourAnswer">The index of your submitted answer.</param>
    /// <param name="correctAnswer">The index of the correct answer.</param>
    void ShowResultAsIncorrect(int yourAnswer, int correctAnswer)
    {
        monitor.ShowAnswerAsIncorrect(correctAnswer);
        livesIndicator.ShowLives((maxIncorrect + 1) - answeredIncorrectly.Count);
        Debug.Log("Answered incorrectly... the correct answer was: " + currentQuestion.CorrectAnswer);
    }

    /// <summary>
    /// After showing the result, wait a short duration before updating score and advancing to the next question.
    /// Will end the game if no questions remain.
    /// </summary>
    /// <param name="correct">Boolean that reflects whether the current question has been answered correctly.</param>
    /// <returns></returns>
    IEnumerator WaitUpdateScoreAndNextQuestion(bool correct)
    {
        yield return new WaitForSeconds(delayBeforeAward);
        if (correct)
            IncrementScore(currentQuestion.Score);

        yield return new WaitForSeconds(delayBeforeNextQuestion);

        if (answeredIncorrectly.Count > maxIncorrect)
        {
            // No lives remaining: game over.
            EndGame(false);
        }
        else if (remainingQuestions.Count > 0)
        {
            // More questions available: next question.
            StartNextQuestion();
        }
        else
        {
            // No more questions left: victory.
            EndGame(true);
        }

        yield return null;
    }

    /// <summary>
    /// Increment the player's score.
    /// </summary>
    /// <param name="increment">Amount to add to the original score. Can be negative.</param>
    void IncrementScore(float increment)
    {
        score += increment;
        if (score < 0.0f)
            score = 0.0f;
        ShowUpdateScore(score);
    }

    /// <summary>
    /// Show the updated player score in the scene.
    /// </summary>
    /// <param name="newScore">The updated score.</param>
    void ShowUpdateScore(float newScore)
    {
        int scoreAsInt = Convert.ToInt32(newScore);
        monitor.UpdateScore(scoreAsInt);
    }

    /// <summary>
    /// End the game. No more questions can be answered.
    /// Will also allow the player to return to the central hub.
    /// </summary>
    /// <param name="completed">Whether the quiz was completed (all questions answered) or if it's a game over (no lives remaining).</param>
    void EndGame(bool completed)
    {
        state = QuizState.EndingGame;

        int scoreAsInt = Convert.ToInt32(score);
        monitor.ShowGameEnded(completed, scoreAsInt);
        Debug.Log("The game has ended.");
        Debug.Log("Your final score is: " + score);

        StartCoroutine(WaitLoadMainMenu(delayBeforeLoadMenu));
    }

    /// <summary>
    /// Load the main menu scene after giving the player time to read their final score.
    /// </summary>
    /// <param name="delay">Delay before loading main menu.</param>
    /// <returns></returns>
    IEnumerator WaitLoadMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        sceneChanges.RequestNewScene("MenuScene");
        yield return null;
    }

    /// <summary>
    /// Drain the player's score over time (every frame) while awaiting input.
    /// </summary>
    void DrainScore()
    {
        float f = scoreDrainPerSecond * Time.deltaTime;

        if (state == QuizState.AwaitingAnswer)
            IncrementScore(-f);
    }

    /// <summary>
    /// Submit an answer.
    /// Called by QuizButton script.
    /// </summary>
    /// <param name="qb"></param>
    public void SubmitAnswer(QuizButton qb)
    {
        if (state == QuizState.AwaitingAnswer)
        {
            // Security check to make sure this button is relevant.
            // E.g. the fourth button isn't relevant for a question with only three answers.
            if ((buttons.IndexOf(qb) + 1) > currentQuestion.Answers.Count)
            {
                Debug.Log("Ignoring input from this button as no answer has been assigned.");
                return;
            }

            qb.PlaySubmitSound();
            state = QuizState.ShowingResult;
            StartCoroutine(WaitRevealAnswer(buttons.IndexOf(qb)));
        }
    }

    /// <summary>
    /// End the current scenario.
    /// Called by QuizScenario once it's completed.
    /// It's up to each individual scenario to determine when this is called.
    /// </summary>
    public void EndScenario()
    {
        AwaitAnswer();
    }
}
