using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace UI.Xml.Tags
{
    public class XmlLayoutTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                // The XmlLayout element doesn't use a primary component
                return null;
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributesToApply)
        {
            // reset XmlElement values to their default (except for new values provided)
            // the reason we do this is because unlike most XmlElements, the XmlLayout's XmlElement is not destroyed before a rebuild
            // if we don't reset the values here, then if you were to clear an existing attribute value (e.g. showAnimation), then it would still be there after the rebuild
            attributesToApply = XmlLayoutUtilities.MergeAttributes(defaultAttributeValues, attributesToApply);

            base.ApplyAttributes(attributesToApply);

            if (!Application.isPlaying) return;

            if (attributesToApply.ContainsKey("cursor") && !string.IsNullOrEmpty(attributesToApply["cursor"]))
            {                
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    XmlLayoutCursorController.Instance.SetCursorForState(XmlLayoutCursorController.eCursorState.Default, attributesToApply["cursor"].ToCursorInfo(), true);
                }, currentXmlElement);
            }

            if (attributesToApply.ContainsKey("cursorClick") && !string.IsNullOrEmpty(attributesToApply["cursorClick"]))
            {
                XmlLayoutTimer.AtEndOfFrame(() =>
                {
                    XmlLayoutCursorController.Instance.SetCursorForState(XmlLayoutCursorController.eCursorState.Click, attributesToApply["cursorClick"].ToCursorInfo(), true);
                }, currentXmlElement);
            }
        }

        protected override AttributeDictionary defaultAttributeValues
        {
            get
            {
                return new AttributeDictionary(new Dictionary<string, string>()
                {
                    {"vm-DataSource", string.Empty},
                    {"onClickSound", string.Empty},
                    {"onShowSound", string.Empty},
                    {"onHideSound", string.Empty},
                    {"onMouseEnterSound", string.Empty},
                    {"onMouseExitSound", string.Empty},
                 
                    {"showAnimation", "None"},
                    {"hideAnimation", "None"},
                    {"showAnimationDelay", "0"},
                    {"hideAnimationDelay", "0"},
                    {"animationDuration", "0.25"},
                    {"defaultOpacity", "1"},
                    
                    {"audioVolume", "1"},
                    {"audioMixerGroup", string.Empty},
                    
                    {"allowDragging", "false"},
                    {"restrictDraggingToParentBounds", "true"},
                    {"returnToOriginalPositionWhenReleased", "true"},
                    {"isDropReceiver", "true"},

                    {"tooltip", string.Empty},
                    {"tooltipBackgroundColor", string.Empty},
                    {"tooltipBackgroundImage", string.Empty},
                    {"tooltipBorderColor", string.Empty},
                    {"tooltipBorderImage", string.Empty},
                    {"tooltipFollowMouse", "false"},
                    {"tooltipFont", string.Empty},
                    {"tooltipFontSize", "0"},
                    {"tooltipOffset", "0"},
                    {"tooltipPadding", "0"},
                    {"tooltipPosition", "Right"},
                    {"tooltipTextColor", string.Empty},
                    {"tooltipTextOutlineColor", string.Empty},

                    {"cursor", string.Empty},
                    {"cursorClick", string.Empty},
                    {"currentOffset", "0"}                      
                });
            }
        }
    }
}
