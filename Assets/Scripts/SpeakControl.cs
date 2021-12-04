using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoogleCloudPlatform;

public class SpeakControl : MonoBehaviour
{
    [SerializeField]
    TMP_InputField inputField;

    [SerializeField]
    private GoogleText2Speech speechApiRef;

    public void OnClickButton()
    {
        var text = inputField.text;

        speechApiRef.SpeakText(text);
    }
}
