using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UI.Xml
{
    [Serializable]
    public class TextComponentWrapper
    {
        [SerializeField]
        private Text textComponent;

#if TEXTMESHPRO_PRESENT
        [SerializeField]
        private TextMeshProUGUI textMeshProComponent;
#endif

        public XmlElement xmlElement;

        public TextComponentWrapper(Text textComponent)
        {
            this.textComponent = textComponent;
            this.xmlElement = textComponent.GetComponent<XmlElement>();
        }

#if TEXTMESHPRO_PRESENT
        public TextComponentWrapper(TextMeshProUGUI textMeshProComponent)
        {
            this.textMeshProComponent = textMeshProComponent;
            this.xmlElement = textMeshProComponent.GetComponent<XmlElement>();
        }
#endif

        public string text
        {
            get
            {
#if TEXTMESHPRO_PRESENT
                if (textMeshProComponent != null) return textMeshProComponent.text;
#endif

                if (textComponent != null) return textComponent.text;

                return null;
            }
            set
            {
#if TEXTMESHPRO_PRESENT
                if (textMeshProComponent != null) textMeshProComponent.text = value;
#endif

                if (textComponent != null) textComponent.text = value;
            }
        }

        public Color color
        {
            get
            {
#if TEXTMESHPRO_PRESENT
                if (textMeshProComponent != null) return textMeshProComponent.color;
#endif

                if (textComponent != null) return textComponent.color;

                return default(Color);
            }
            set
            {
#if TEXTMESHPRO_PRESENT
                if (textMeshProComponent != null)
                {
                    textMeshProComponent.color = value;

                    if (textMeshProComponent.fontMaterial.GetColor("_FaceColor") != Color.white)
                    {
                        textMeshProComponent.fontMaterial.SetColor("_FaceColor", Color.white);
                    }
                }
#endif

                if (textComponent != null) textComponent.color = value;
            }
        }


    }
}
