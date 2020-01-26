using UnityEngine;

namespace UI.Xml
{
    [RequireComponent(typeof(AudioSource))]
    class XmlLayoutOneShotAudio : MonoBehaviour
    {
        /// <summary>
        /// The audio source
        /// </summary>
        private AudioSource _audioSource;

        private void Awake()
        {
            this._audioSource = this.GetComponent<AudioSource>();

            // Prevent audio from cutting out while switching scenes.
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            if (!this._audioSource.isPlaying)
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
