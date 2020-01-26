using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace UI.Xml
{
    public class XmlLayoutProgressBar : MonoBehaviour
    {
        [SerializeField, Range(0, 100)]
        private float m_percentage = 0.0f;
        public float percentage
        {
            get { return m_percentage; }
            set { SetProperty(ref m_percentage, Mathf.Max(0, Mathf.Min(100, value))); }
        }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("showPercentageText")]
        private bool m_showPercentageText = true;
        public bool showPercentageText
        {
            get { return m_showPercentageText; }
            set { SetProperty(ref m_showPercentageText, value); }
        }
        public string percentageTextFormat = "0.00";

        [SerializeField]
        private bool m_horizontal = true;
        public bool horizontal
        {
            get { return m_horizontal; }
            set { SetProperty(ref m_horizontal, value); }
        }

        [SerializeField]
        private bool m_reverse = false;
        public bool reverse
        {
            get { return m_reverse; }
            set { SetProperty(ref m_reverse, value); }
        }


        [Header("References")]
        public Image ref_backgroundImage;
        public Image ref_fillImage;
        public Text ref_text;

        void SetDirty()
        {
            if (!this.isActiveAndEnabled) return;

            ref_text.gameObject.SetActive(showPercentageText);
            ref_text.text = String.Format("{0:" + percentageTextFormat + "}%", percentage);

            if (horizontal)
            {

                if (reverse)
                {
                    ref_fillImage.rectTransform.anchorMin = Vector2.zero;
                    ref_fillImage.rectTransform.anchorMax = new Vector2(1, 1);
                    ref_fillImage.rectTransform.pivot = new Vector2(1, 0);
                }
                else
                {
                    ref_fillImage.rectTransform.anchorMin = Vector2.zero;
                    ref_fillImage.rectTransform.anchorMax = new Vector2(0, 1);
                    ref_fillImage.rectTransform.pivot = Vector2.zero;
                }


                ref_fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ref_backgroundImage.rectTransform.rect.width * (percentage / 100f));
                ref_fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ref_backgroundImage.rectTransform.rect.height);
            }
            else
            {
                if (reverse)
                {
                    ref_fillImage.rectTransform.anchorMin = new Vector2(0, 1);
                    ref_fillImage.rectTransform.anchorMax = Vector2.one;
                    ref_fillImage.rectTransform.pivot = new Vector2(0.5f, 1);
                }
                else
                {
                    ref_fillImage.rectTransform.anchorMin = Vector2.zero;
                    ref_fillImage.rectTransform.anchorMax = new Vector2(1, 0);
                    ref_fillImage.rectTransform.pivot = new Vector2(0.5f, 0);
                }

                ref_fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ref_backgroundImage.rectTransform.rect.height * (percentage / 100f));
                ref_fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ref_backgroundImage.rectTransform.rect.width);
            }
        }

        void SetProperty<T>(ref T o, T value)
        {
            o = value;
            SetDirty();
        }

        void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        void OnEnable()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}
