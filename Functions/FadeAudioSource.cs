using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FadeAudioSource
{
    /// <summary>
    /// Transition an AudioSource's volume from A to B gradually.
    /// </summary>
    /// <param name="audioSource">AudioSource object to affect.</param>
    /// <param name="duration">Duration, time it takes to fully lerp from A to B.</param>
    /// <param name="targetVolume">Target volume at the end of this transition.</param>
    /// <returns></returns>
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}