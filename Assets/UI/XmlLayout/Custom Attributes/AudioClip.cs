/*
using UnityEngine;

namespace UI.Xml.CustomAttributes
{
    public class OnClickSoundAttribute : AudioClipAttribute
    {
        protected override string attributeName { get { return "onClickSound"; } }
    }

    public class OnShowSoundAttribute : AudioClipAttribute
    {
        protected override string attributeName { get { return "onShowSound"; } }
    }

    public class OnHideSoundAttribute : AudioClipAttribute
    {
        protected override string attributeName { get { return "onHideSound"; } }
    }

    public class OnMouseExitSoundAttribute : AudioClipAttribute
    {
        protected override string attributeName { get { return "onMouseExitSound"; } }
    }

    public abstract class AudioClipAttribute: CustomXmlAttribute
    {
        protected virtual string attributeName { get { return null; } }

        private void ApplyLocalValue(XmlElement xmlElement, string value)
        {
            // I could do this with reflection, but with so few values it's easy enough to do one-by-one and will perform better
            switch(attributeName)
            {
                case "onClickSound":
                    xmlElement.OnClickSound = value.ToAudioClip();
                    break;
                case "onShowSound":
                    xmlElement.OnShowSound = value.ToAudioClip();
                    break;
                case "onHideSound":
                    xmlElement.OnHideSound = value.ToAudioClip();
                    break;
                case "onMouseExitSound":
                    xmlElement.OnMouseExitSound = value.ToAudioClip();
                    break;
            }

        }

        public override bool UsesApplyMethod { get { return true; } }

        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary attributes)
        {
            if (value.IsUrl())
            {
                if (XmlLayoutResourceDatabase.instance.GetResource<AudioClip>(value) == null)
                {
                    XmlLayoutResourceDatabase.instance.LoadResourcesFromUrls<AudioClip>(
                        () =>
                        {
                            // clip is now downloaded and local
                            ApplyLocalValue(xmlElement, value);
                        },
                        value);
                }
                else
                {
                    // We've already downloaded this file, no need to do so again
                    ApplyLocalValue(xmlElement, value);
                }
            }
            else
            {
                // Local path,
                ApplyLocalValue(xmlElement, value);
            }
        }
    }
}
*/