using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QuizScenario is a scenario or specific seizure that can be loaded during the quiz.
/// An example of this would be experiencing an absence seizure, then asking the player what type of seizure just occurred.
/// </summary>
public class QuizScenario : MonoBehaviour
{
    [SerializeField]
    string name;
    [SerializeField]
    GameObject content;
    [SerializeField]
    QuizManager manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Load this scenario.
    /// Called by QuizManager.
    /// </summary>
    public void Load()
    {
        content.SetActive(true);
    }

    /// <summary>
    /// Unload this scenario.
    /// This is the final step that has to be called by this scenario's content when it's completed.
    /// Also contacts QuizManager to resume the quiz.
    /// </summary>
    public void Unload()
    {
        content.SetActive(false);
        manager.EndScenario();
    }

    public string Name { get => name; set => name = value; }
}
