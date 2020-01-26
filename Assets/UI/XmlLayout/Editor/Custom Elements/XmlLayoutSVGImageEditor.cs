#if UNITY_2018_1_OR_NEWER
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Xml
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>
    [CustomEditor(typeof(XmlLayoutSVGImage), isFallback = true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Image Component.
    ///   Extend this class to write a custom editor for an Image-derived component.
    /// </summary>
    public class SVGImageEditor : GraphicEditor
    {
        SerializedProperty m_Sprite;
        GUIContent m_SpriteContent;

        SerializedProperty m_PreserveAspect;
        GUIContent m_PreserveAspectContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SpriteContent = new GUIContent("Source SVG Image");
            m_Sprite = serializedObject.FindProperty("m_Sprite");

            m_PreserveAspectContent = new GUIContent("Preserve Aspect");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_PreserveAspect, m_PreserveAspectContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif