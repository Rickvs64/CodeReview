using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizButton : BaseInteractable
{
    [SerializeField]
    QuizManager manager;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip submitSound;

    public override void OnHover()
    {
        SubmitAnswer();
    }

    public override void OnPress()
    {
        SubmitAnswer();
    }

    /// <summary>
    /// Contact the QuizManager instance to submit an answer.
    /// Manager may handle/ignore input depending on its current state.
    /// </summary>
    void SubmitAnswer()
    {
        manager.SubmitAnswer(this);
    }

    /// <summary>
    /// Called by QuizManager. Not part of SubmitAnswer() as that'd play the audio even when buttons are disabled.
    /// </summary>
    public void PlaySubmitSound()
    {
        audioSource.PlayOneShot(submitSound);
    }
}
