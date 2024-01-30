using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;


/// <summary>
/// <author>Jefferson Reis</author>
/// <explanation>Works only on Android. To test, change the platform to Android.</explanation>
/// </summary>

public class TextToSpeech : MonoBehaviour
{
    public string words = "Hello";


    IEnumerator Start()
    {
        // Remove the "spaces" in excess
        Regex rgx = new Regex("\\s+");
        // Replace the "spaces" with "% 20" for the link Can be interpreted
        string result = rgx.Replace(words, "+");//rgx.Replace(words, "%20");
        string url = "http://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q=" + result;
        WWW www = new WWW(url);
        yield return www;

        AudioSource player = GetComponent<AudioSource>();

        
        player.clip = www.GetAudioClip(false, true, AudioType.UNKNOWN);

        player.Play();




        
        
    }

    void OnGUI()
    {
        words = GUI.TextField(new Rect(Screen.width / 2 - 200 / 2, 10, 200, 30), words);
        if (GUI.Button(new Rect(Screen.width / 2 - 150 / 2, 40, 150, 50), "Speak"))
        {
            StartCoroutine(Start());
        }
    }

}//closes the class