using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This quiz scenario is executed directly before asking the player:
/// NL: "Welke van deze epileptische aanvallen heb je zojuist ervaren?"
/// EN: "Which of these epileptic seizures did you just experience?"
/// 
/// Features a standard environment and a simple seizure example before fading back to the quiz.
/// </summary>
public class QuizScenario_Absence : MonoBehaviour
{
    [SerializeField]
    QuizScenario scenario;
    [SerializeField]
    QuizManager manager;
    [SerializeField]
    InterupsyVRController player;
    [SerializeField]
    GameObject quizEnvironment;
    [SerializeField]
    GameObject environment;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Sequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Main sequence coroutine. Includes fading in and out of this scenario.
    /// </summary>
    /// <returns></returns>
    IEnumerator Sequence()
    {
        player.FadeOut();
        yield return new WaitForSeconds(2.1f);
        quizEnvironment.SetActive(false);
        environment.SetActive(true);
        player.transform.position = new Vector3(0, 0, 0);
        player.FadeIn();
        yield return new WaitForSeconds(2.1f);

        player.FadeOut();
        yield return new WaitForSeconds(2.1f);
        quizEnvironment.SetActive(true);
        environment.SetActive(false);
        player.transform.position = new Vector3(0, 0, 0);
        player.FadeIn();
        yield return new WaitForSeconds(2.1f);
        scenario.Unload();
        yield return null;
    }
}
