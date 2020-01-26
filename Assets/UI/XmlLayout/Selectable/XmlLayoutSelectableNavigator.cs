/*
 * This component emulates standard navigation behaviour for XmlLayout interfaces, e.g. it handles
 *  'Tab'                                - Pressing this will move to the next available 'selectable' item
 *  'Submit' (usually Enter or Space)    - Pressing this will 'submit' the form
 */
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

namespace UI.Xml
{
    public class XmlLayoutSelectableNavigator : MonoBehaviour
    {
        public static XmlLayoutSelectableNavigator instance;

        EventSystem system;

        void Start()
        {
            system = EventSystem.current;
        }

        void OnEnable()
        {
            if (instance != null && instance != this) Destroy(this);

            instance = this;

            if (!XmlLayoutUtilities.XmlLayoutConfiguration.UseXmlLayoutSelectableNavigation) this.enabled = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Selectable next = null;

                if (system.currentSelectedGameObject != null)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                    }
                    else
                    {
                        next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                    }
                }

                if (next == null)
                {
#if UNITY_2019_2_OR_NEWER
                    next = Selectable.allSelectablesArray[0];
#else
                    next = Selectable.allSelectables[0];
#endif
                }

                if (next != null) next.Select();
            }

            if (Input.GetButtonDown("Submit"))
            {
                if (system.currentSelectedGameObject != null && system.currentSelectedGameObject.activeInHierarchy)
                {
                    var xmlElement = system.currentSelectedGameObject.GetComponent<XmlElement>();
                    if (xmlElement != null)
                    {
                        if (xmlElement.m_onSubmitEvents != null && xmlElement.m_onSubmitEvents.Count > 0)
                        {
                            if (xmlElement.tagType.Contains("InputField") && Input.GetKeyDown(KeyCode.Space))
                            {
                                // do not consider 'Space' to be a submit event for an input field
                            }
                            else
                            {
                                xmlElement.OnSubmit(new BaseEventData(system));
                            }
                        }
                        else
                        {
                            xmlElement.OnPointerClick(new PointerEventData(system));
                        }
                    }
                }
            }
        }
    }
}
