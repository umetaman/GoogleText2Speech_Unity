using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleCloudPlatform
{
    [Serializable]
    public struct RequestBody
    {
        [Serializable]
        public struct AudioConfig
        {
            public string audioEncoding;
            public float pitch;
            public float speakingRate;

            //LINEAR16 = WAV
            public AudioConfig(string encoding = "LINEAR16", float pitch = 0.5f, float speakingRate = 1.0f)
            {
                this.audioEncoding = encoding;
                this.pitch = pitch;
                this.speakingRate = speakingRate;
            }
        }

        [Serializable]
        public struct Input
        {
            public string text;

            public Input(string text)
            {
                this.text = text;
            }
        }

        [Serializable]
        public struct Voice
        {
            public string languageCode;
            public string name;

            public Voice(string languageCode = "ja-JP", string name = "ja-JP-Wavenet-A")
            {
                this.languageCode = languageCode;
                this.name = name;
            }
        }

        public AudioConfig audioConfig;
        public Input input;
        public Voice voice;

        public RequestBody(AudioConfig config, Input input, Voice voice)
        {
            this.audioConfig = config;
            this.input = input;
            this.voice = voice;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public struct ResponseBody
    {
        [Serializable]
        public struct Error
        {
            public int code;
            public string message;
            public string status;

            public override string ToString()
            {
                return string.Format("Code: {0}, Message: {1}, Status: {2}",
                    code, message, status);
            }
        }

        public Error error;
        public string audioContent;
    }

    [RequireComponent(typeof(AudioSource))]
    public class GoogleText2Speech : MonoBehaviour
    {
        [Header("Google Cloud Platform")]
        [SerializeField]
        private TextAsset keyFile;
        public string Key
        {
            get => keyFile == null ? "" : keyFile.text;
        }
        [SerializeField]
        private TextAsset tokenFile;
        public string Token
        {
            get => tokenFile == null ? "" : tokenFile.text;
        }

        public string Url
        {
            get => string.Format("https://texttospeech.googleapis.com/v1beta1/text:synthesize?key={0}", Key);
            //get => "https://texttospeech.googleapis.com/v1beta1/text:synthesize";
        }

        [Header("Text to Speech")]
        public float Pitch = 0.75f;
        public float SpeakingRate = 1.25f;

        [Header("Audio")]
        [SerializeField]
        private AudioSource audioSource;

        public void SpeakText(string text)
        {
            SpeakText(text, Pitch, SpeakingRate);
        }

        public void SpeakText(string text, float pitch, float speakingRate)
        {
            var request = new RequestBody()
            {
                audioConfig = new RequestBody.AudioConfig("LINEAR16", pitch, speakingRate),
                input = new RequestBody.Input(text),
                voice = new RequestBody.Voice("ja-JP", "ja-JP-Wavenet-A")
            };

            StartCoroutine(playSpeech(Url, request));
        }

        private IEnumerator playSpeech(string url, RequestBody body)
        {
            if(audioSource == null)
            {
                Debug.LogError("AudioSource is Null.");
                yield break;
            }

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(body.ToJson());

            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", Token);

            yield return request.SendWebRequest();

            var response = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
            Debug.Log(response.error.code);


            byte[] decoded = System.Convert.FromBase64String(response.audioContent);

            var clip = WavUtility.ToAudioClip(decoded);
            audioSource.clip = clip;

            if (!audioSource.isPlaying && audioSource.clip.loadState == AudioDataLoadState.Loaded)
            {
                audioSource.Play();
            }

        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
