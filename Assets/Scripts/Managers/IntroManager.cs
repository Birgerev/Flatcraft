using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Task = System.Threading.Tasks.Task;

public class IntroManager : MonoBehaviour
{
    public AudioSource vhsInsertSound;
    public AudioSource staticNoise;
    public Animator logoAnimator;
    public SpriteRenderer fadeBackground;
    

    // Start is called before the first frame update
    void Start()
    {
        IntroSequence();
    }

    async void IntroSequence()
    {
        Scene introScene = SceneManager.GetSceneByName("Intro");
        
        AudioSequence();
        
        await Task.Delay(6000);

        await FadeBackground();
        
        SceneManager.UnloadSceneAsync(introScene);
    }

    async void AudioSequence()
    {
        vhsInsertSound.Play();
        await Task.Delay(3000);
        logoAnimator.SetTrigger("ShowLogo");
        await Task.Delay(500);
        staticNoise.Play();
    }

    async Task FadeBackground()
    {
        const float fadeSpeed = .01f;
        
        while (fadeBackground.color.a > 0)
        {
            Color color = fadeBackground.color;
            color.a -= fadeSpeed;

            fadeBackground.color = color;
            await Task.Delay(50);
        }
    }
}
