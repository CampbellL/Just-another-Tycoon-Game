using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace UI.Xml
{
    [CustomPropertyDrawer(typeof(AttributeDictionary))]
    public class AttributeDictionaryPropertyDrawer : PropertyDrawer
    {
        float lineHeight = 16;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            var xmlElement = property.serializedObject.targetObject as XmlElement;

            if (xmlElement != null)
            {                
                EditorGUI.LabelField(position, property.displayName);
                position.y += lineHeight;
                position.x += 10;

                if (xmlElement.attributes != null)
                {
                    var positionA = position;
                    positionA.width = EditorGUIUtility.labelWidth;

                    var positionB = position;
                    positionB.width = position.width - positionA.width - 10;
                    positionB.x += positionA.width - 10;

                    foreach (var attribute in xmlElement.attributes)
                    {
                        EditorGUI.LabelField(positionA, attribute.Key);
                        EditorGUI.LabelField(positionB, attribute.Value);

                        positionA.y += lineHeight;
                        positionB.y += lineHeight;                        
                    }
                }
            }
            else
            {
                base.OnGUI(position, property, label);
            }            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var xmlElement = property.serializedObject.targetObject as XmlElement;

            if (xmlElement != null)
            {
                var height = lineHeight;
                
                if (xmlElement.attributes != null)
                {
                    height += xmlElement.attributes.Count * lineHeight;
                }

                return height;
            } 
            else 
            {
                return base.GetPropertyHeight(property, label);
            }
        }
    }
}
