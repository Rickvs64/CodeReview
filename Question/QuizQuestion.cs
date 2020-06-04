using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[Serializable]
public class QuizQuestion
{
    float score = 100.0f;
    string question = "(Undefined question)";
    List<string> answers;       // The first element is considered the correct answer.
    float duration = 10.0f;
    string scenario = "";

    string correctAnswer = "";  // Set to the 1st list element of answers[] by constructor.

    public QuizQuestion()
    {
        answers = new List<string>();
    }

    public QuizQuestion(string question, List<string> answers)
    {
        this.question = question;
        this.answers = answers;
        correctAnswer = answers[0];     // Answers are shuffled in the quiz so the right answer needs to be stored.
        answers.Shuffle();
    }

    public QuizQuestion(int score, string question, List<string> answers, float duration, string scenario)
    {
        this.score = score;
        this.question = question;
        this.answers = answers;
        this.duration = duration;
        this.scenario = scenario;
        correctAnswer = answers[0];     // Answers are shuffled in the quiz so the right answer needs to be stored.
        answers.Shuffle();
    }



    /// <summary>
    /// Convert the question into an audio clip file name (removing special characters and spacing).
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public string DetermineAudioName()
    {
        return Regex.Replace(question, "[^0-9a-zA-Z]+", "").ToLower();
    }

    /// <summary>
    /// Convert the question into a media clip file name (removing special characters and spacing).
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public string DetermineMediaName()
    {
        return Regex.Replace(question, "[^0-9a-zA-Z]+", "").ToLower();
    }


    public float Score { get => score; set => score = value; }
    public string Question { get => question; set => question = value; }
    public List<string> Answers { get => answers; set => answers = value; }
    public float Duration { get => duration; set => duration = value; }
    public string CorrectAnswer { get => correctAnswer; set => correctAnswer = value; }
    public string Scenario { get => scenario; set => scenario = value; }

    public override string ToString()
    {
        return String.Format("Question: {0}; Answer: {1}", question, answers[0]);
    }
}
