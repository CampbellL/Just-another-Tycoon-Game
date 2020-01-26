using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{
    public partial class XmlElement
    {
        public enum SelectionState
        {
            Normal = 0,
            Highlighted = 1,
            Pressed = 2,
            Disabled = 3,
        }

        public SelectionState selectionState { get; protected set; }

        public void NotifySelectionStateChanged(SelectionState newSelectionState)
        {
            if (newSelectionState == SelectionState.Highlighted)
            {
                Highlight();
            }
            else
            {
                // We were selected, but aren't any longer
                if (this.selectionState == SelectionState.Highlighted 
                    && newSelectionState != SelectionState.Pressed) // don't clear the highlight if we changed to 'Pressed' (the mouse is most likely still over)
                {
                    RemoveHighlight();                    
                }
            }

            this.selectionState = newSelectionState;
        }

        private void Highlight()
        {
            if (!String.IsNullOrEmpty(Tooltip))
            {
                xmlLayout.ShowTooltip(this, Tooltip);
            }

            if (selectable != null && !selectable.interactable) return;
            
            if (this.selectionState != SelectionState.Pressed) PlaySound(OnMouseEnterSound);

            if (hoverClasses != null && hoverClasses.Count > 0)
            {
                hoverClasses.ForEach((c) => AddClass(c));
            }
        }

        private void RemoveHighlight()
        {
            if (!String.IsNullOrEmpty(Tooltip))
            {
                xmlLayout.HideTooltip(this);
            }

            if (selectable != null && !selectable.interactable) return;

            PlaySound(OnMouseExitSound);

            if (hoverClasses != null && hoverClasses.Count > 0)
            {
                hoverClasses.ForEach((c) => RemoveClass(c));
            }            
        }
    }
}
