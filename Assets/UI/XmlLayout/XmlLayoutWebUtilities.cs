using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Xml;
using UI.Xml.CustomAttributes;
using UI.Xml.Tags;
using UI.Xml.Configuration;
using UnityEngine.Networking;

namespace UI.Xml
{
    public static class XmlLayoutWebUtilities
    {
        public static void LoadTextFromUrl(Action<string> callback, string url)
        {
            XmlLayoutTimer.StartCoroutine(LoadText(url, callback));
        }

        private static IEnumerator LoadText(string url, Action<string> callback)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);

#if UNITY_2018_1_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            if (www.isNetworkError)
            {
                Debug.LogError(www.error);
                yield break;
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }

        public static void LoadResourcesFromUrls<T>(Action<Dictionary<string, T>> callback, params string[] urls)
            where T : UnityEngine.Object
        {
            int completeCount = 0;

            Dictionary<string, T> results = new Dictionary<string, T>();

            var distinctUrls = urls.Distinct().ToArray();
            int count = distinctUrls.Length;

            foreach (var url in distinctUrls)
            {
                XmlLayoutTimer.StartCoroutine(LoadResource<T>(url,
                    (x) =>
                    {
                        results.Add(url, x);
                        completeCount++;

                        if (completeCount == count)
                        {
                            callback(results);
                        }
                    }));
            }
        }

        private static IEnumerator LoadResource<T>(string url, Action<T> callback)
            where T : UnityEngine.Object
        {
            Type type = typeof(T);
            UnityWebRequest www = null;

            if (type == typeof(Sprite))
            {
                www = UnityWebRequestTexture.GetTexture(url);
            }
            //else if (type == typeof(AudioClip))
            //{
            //    www = UnityWebRequest.GetAudioClip(url, AudioType.OGGVORBIS);
            //}
#if UNITY_2018_1_OR_NEWER
            else if (type == typeof(TextAsset))
            {
                www = UnityWebRequest.Get(url);
            }
#endif
            else
            {
                Debug.LogWarning("[XmlLayout][XmlLayoutWebUtilities] Warning: unsupported resource type '" + type.Name + "'.");
                yield break;
            }

#if UNITY_2018_1_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif


            if (www.isNetworkError)
            {
                Debug.LogError(www.error);
                yield break;
            }
            else
            {
                if (type == typeof(Sprite))
                {
                    Texture2D texture = (www.downloadHandler as DownloadHandlerTexture).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                    callback((T)Convert.ChangeType(sprite, type));
                }
                //else if (type == typeof(AudioClip))
                //{
                //    callback((T)Convert.ChangeType((www.downloadHandler as DownloadHandlerAudioClip).audioClip, type));
                //}
#if UNITY_2018_1_OR_NEWER
                else if (type == typeof(TextAsset))
                {
                    callback((T)Convert.ChangeType(new TextAsset(www.downloadHandler.text), type));
                }
#endif
            }
        }
    }
}
