using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public class XmlLayoutSingleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        protected static T _Instance;
        protected bool dontInstantiate = false;
        private static bool sceneUnloading = false;

        public static T Instance
        {
            get
            {
                if (!_Instance)
                {
                    _Instance = (T)MonoBehaviour.FindObjectOfType<T>();

                    if (!_Instance && !sceneUnloading)
                    {                        
                        GameObject Container = new GameObject();
                        Container.name = "__" + typeof(T).ToString();

                        _Instance = Container.AddComponent<T>();

                        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += (scene) => { OnSceneUnloaded(scene); };
                        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadMode) => { OnSceneLoaded(scene); };
                    }
                }

                return _Instance;
            }
        }

        /// <summary>
        /// If you use the Awake function, make sure to call this
        /// base.Awake()
        /// </summary>
        public virtual void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this as T;
            }
            else
            {
                // We may not have more than one of this object - if one already exists, then destroy this
                this.dontInstantiate = true;

                Destroy(this);
                sceneUnloading = false;
            }
        }

        void OnDestroy()
        {
            sceneUnloading = true;
        }

        public static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            sceneUnloading = true;
        }

        public static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene)
        {
            sceneUnloading = false;
        }
    }
}
