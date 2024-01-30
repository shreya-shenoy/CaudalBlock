using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System.Diagnostics;

public class DictationScript : MonoBehaviour
{

    private DictationRecognizer m_DictationRecognizer;

    void Start()
    {
        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            //Final version of sentence with highest accuracy but only finishes after full phrase is spoken
            //To add functions, just create a switch case tree or if tree for each phrase using the variable "text"

            UnityEngine.Debug.LogFormat("Dictation result: {0}", text, confidence);

            if (text == "score me")
            {
                IV_Manager.ME.spaceBarPress();
            }

            if (text == "close program")
            {
                if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            //Creates a sentence in realtime but might have inaccuracies
            //Debug.LogFormat("Dictation hypothesis: {0}", text);
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                UnityEngine.Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            UnityEngine.Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.Start();
    }

    //Function to toggle on and off the voice recognition
    public void toggleVoice()
    {
        if(m_DictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            m_DictationRecognizer.Stop();
        }
        else
        {
            m_DictationRecognizer.Start();
        }
    }
}