using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml.CustomAttributes
{
    public class RectAlignmentAttribute : SizeAttribute
    {
        public override void Apply(XmlElement xmlElement, string value, AttributeDictionary elementAttributes)
        {            
            bool applyX = true, applyY = true;

            // if an explicit width is defined, then this will be handled by the width attribute
            if (elementAttributes.ContainsKey("width") || xmlElement.HasAttribute("width"))
            {                                
                applyX = false;
            }
            // if an explicit height is defined, then this will be handled by the height attribute
            if (elementAttributes.ContainsKey("height") || xmlElement.HasAttribute("height"))
            {                                
                applyY = false;
            }

            // If a content size fitter is present, then it may override defined width/height values
            if (!applyX || !applyY)
            {
                if (elementAttributes.ContainsKey("contentSizeFitter") || xmlElement.HasAttribute("contentSizeFitter"))
                {
                    string contentSizeFitter = xmlElement.HasAttribute("contentSizeFitter") ? xmlElement.GetAttribute("contentSizeFitter") : elementAttributes.GetValue("contentSizeFitter");

                    if (contentSizeFitter == "horizontal" || contentSizeFitter == "both")
                    {                        
                        applyX = true;
                    }

                    if (contentSizeFitter == "vertical" || contentSizeFitter == "both")
                    {                        
                        applyY = true;
                    }                    
                }
            }
            
            var alignment = GetRectAlignment(value);
            var alignmentStruct = GetRectAlignmentStruct(alignment);            
            
            var elementTransform = xmlElement.rectTransform;            

            if (applyX ^ applyY)
            {
                elementTransform.anchorMin = new Vector2(applyX ? alignmentStruct.AnchorMin.x : elementTransform.anchorMin.x,
                                                         applyY ? alignmentStruct.AnchorMin.y : elementTransform.anchorMin.y);

                elementTransform.anchorMax = new Vector2(applyX ? alignmentStruct.AnchorMax.x : elementTransform.anchorMax.x,
                                                         applyY ? alignmentStruct.AnchorMax.y : elementTransform.anchorMax.y);               
            }

            elementTransform.pivot = new Vector2(applyX ? alignmentStruct.Pivot.x : elementTransform.pivot.x,
                                                 applyY ? alignmentStruct.Pivot.y : elementTransform.pivot.y);            
        }

        protected RectAlignmentStruct GetRectAlignmentStruct(RectAlignment alignment)
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Vector2 anchorMin = new Vector2(0, 0);
            Vector2 anchorMax = new Vector2(1, 1);            

            switch (alignment)
            {
                case RectAlignment.LowerCenter:
                    pivot = new Vector2(0.5f, 0);                                     
                    break;
                case RectAlignment.LowerLeft:
                    pivot = new Vector2(0, 0);                    
                    break;
                case RectAlignment.LowerRight:
                    pivot = new Vector2(1, 0);                                    
                    break;

                case RectAlignment.MiddleCenter:
                    break;

                case RectAlignment.MiddleLeft:
                    pivot = new Vector2(0, 0.5f);                             
                    break;
                case RectAlignment.MiddleRight:
                    pivot = new Vector2(1, 0.5f);                                  
                    break;

                case RectAlignment.UpperCenter:
                    pivot = new Vector2(0.5f, 1);                                    
                    break;

                case RectAlignment.UpperLeft:
                    pivot = new Vector2(0, 1);
                    break;
                case RectAlignment.UpperRight:
                    pivot = new Vector2(1, 1);                    
                    break;
            }

            return new RectAlignmentStruct
            {
                Pivot = pivot,
                AnchorMin = anchorMin,
                AnchorMax = anchorMax
            };
        }
    }
}
