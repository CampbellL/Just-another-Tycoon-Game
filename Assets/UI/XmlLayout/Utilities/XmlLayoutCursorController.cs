using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UI.Xml
{    
    public class XmlLayoutCursorController : XmlLayoutSingleton<XmlLayoutCursorController>        
    {
        [SerializeField]
        protected eCursorState m_CursorState = eCursorState.Default;
        public eCursorState CursorState
        {
            get { return m_CursorState; }
            set 
            { 
                m_CursorState = value;
                StateChanged();
            }
        }

        [SerializeField]
        protected Vector2 m_CursorHotSpot = Vector2.zero;
        public Vector2 CursorHotSpot
        {
            get { return m_CursorHotSpot; }
            set
            {
                m_CursorHotSpot = value;
                StateChanged();
            }
        }

        public SerializableDictionary<eCursorState, CursorInfo> cursors = new SerializableDictionary<eCursorState, CursorInfo>();
        private SerializableDictionary<eCursorState, CursorInfo> m_defaultCursors = new SerializableDictionary<eCursorState, CursorInfo>();

        public override void Awake()
        {
            base.Awake();            

            if (!cursors.ContainsKey(eCursorState.Default)) SetCursorForState(eCursorState.Default, XmlLayoutUtilities.LoadResource<Texture2D>("Cursors/DefaultCursor"), Vector2.zero);

            CursorState = eCursorState.Default;
        }

        protected void StateChanged()
        {
            CursorInfo cursorInfo = cursors.ContainsKey(CursorState) ? cursors[CursorState] : cursors[eCursorState.Default];

            if(cursorInfo != null && cursorInfo.cursor != null) Cursor.SetCursor(cursorInfo.cursor, cursorInfo.hotspot, CursorMode.Auto);            
        }

        public void SetCursorForState(eCursorState state, CursorInfo cursorInfo, bool isDefault = false)
        {            
            if (!cursors.ContainsKey(state)) cursors.Add(state, cursorInfo);
            else cursors[state] = cursorInfo;

            if (isDefault)
            {
                if (!m_defaultCursors.ContainsKey(state)) m_defaultCursors.Add(state, cursorInfo);
                else m_defaultCursors[state] = cursorInfo;
            }

            StateChanged();
        }

        public void SetCursorForState(eCursorState state, Texture2D cursor, Vector2 hotspot, bool isDefault = false)
        {
            SetCursorForState(state, new CursorInfo { cursor = cursor, hotspot = hotspot }, isDefault);            
        }

        public void ResetCursorToDefaultForState(eCursorState state)
        {
            if (m_defaultCursors.ContainsKey(state))
            {
                SetCursorForState(state, m_defaultCursors[state]);
            }
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                CursorState = eCursorState.Click;
            }
            else
            {
                // TODO:: preserve other cursor states
                CursorState = eCursorState.Default;
            }
        }

        public enum eCursorState
        {
            Default,
            Click,
            
            // TODO::
/*            ResizeLeft,
            ResizeRight,
            ResizeTop,
            ResizeBottom,
            ResizeLeftCorner,
            ResizeRightCorner*/
        }

        [Serializable]
        public class CursorInfo
        {
            [SerializeField]
            public Texture2D cursor;
            [SerializeField]
            public Vector2 hotspot;
        }
    }
}
