// In order to use this, you need to a) have PagedRect installed within the project, and b) make sure PAGEDRECT_PRESENT is defined
// You can do this by
// a) defining it here: #define PAGEDRECT_PRESENT (not reccommended as it will be overwritten if you update XmlLayout)
// b) defining it in your "Player Settings" -> "Scripting Define Symbols"
// c) Adding a file called "smcs.rsp" to your PROJECT_DIR/Assets/ folder, and putting the following text in it "-define: PAGEDRECT_PRESENT" (you will need to restart Visual Studio after doing so)
#if PAGEDRECT_PRESENT
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UI.Pagination;
using System;
using System.Linq;
using System.Reflection;

namespace UI.Xml.Tags
{
    public class PagedRect_HorizontalTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/HorizontalPagination"; } } }
    public class PagedRect_Horizontal_ScrollRectTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/HorizontalPagination - ScrollRect"; } } }

    public class PagedRect_VerticalTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/VerticalPagination"; } } }
    public class PagedRect_Vertical_ScrollRectTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/VerticalPagination - ScrollRect"; } } }

    public class PagedRect_SliderTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/Slider"; } } }
    public class PagedRect_Slider_ScrollRectTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/Slider - ScrollRect"; } } }

    public class PagedRect_PagePreviews_HorizontalTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/Page Previews - Horizontal"; } } }
    public class PagedRect_PagePreviews_VerticalTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/Page Previews - Vertical"; } } }

    public class PagedRect_Tabs_Horizontal_ScrollRectTagHandler : PagedRectTagHandler { public override string prefabPath { get { return "Prefabs/Tabs - Horizontal - ScrollRect"; } } }

    public class PagedRectTagHandler : ElementTagHandler
    {
        public override MonoBehaviour primaryComponent
        {
            get
            {
                return currentInstanceTransform.GetComponent<PagedRect>();
            }
        }

        PagedRect pagedRect
        {
            get
            {
                return (PagedRect)primaryComponent;
            }
        }

        private List<Page> pagesToRemove = new List<Page>();

        public override string prefabPath
        {
            get
            {
                return "Prefabs/HorizontalPagination - ScrollRect";
            }
        }

        public override void ApplyAttributes(AttributeDictionary attributes)
        {
            base.ApplyAttributes(attributes);

            if (attributes.ContainsKey("showPagination"))
            {
                if (!attributes.GetValue<bool>("showPagination"))
                {                    
                    var viewportRectTransform = (RectTransform)currentInstanceTransform.GetComponentInChildren<Viewport>().transform;
                    viewportRectTransform.offsetMax = Vector2.zero;
                    viewportRectTransform.offsetMin = Vector2.zero;

                    pagedRect.Pagination.gameObject.SetActive(false);
                }                
            }            
        }

        public override RectTransform transformToAddChildrenTo
        {
            get
            {
                return ((PagedRect)primaryComponent).Viewport.transform as RectTransform;
            }
        }

        public override bool isCustomElement
        {
            get
            {
                return true;
            }
        }

        public override string elementChildType
        {
            get
            {
                return "pagedRect";
            }
        }        
        
        public override void Open(AttributeDictionary attributes)
        {
            base.Open(attributes);

            _previousPagedRectTagHandler = CurrentPagedRectTagHandler;
            CurrentPagedRectTagHandler = this;

            // We're using PagedRect's built-in prefabs (so as to always use the up-to-date versions)
            // But, the built-in prefabs have a few pages already set, so we need to clear them            
            pagesToRemove = pagedRect.Pages.ToList();
        }

        public override void Close()
        {
            base.Close();
            
            // now remove the template pages
            foreach (var page in pagesToRemove)
            {                
                pagedRect.RemovePage(page, true);
            }
            
            CurrentPagedRectTagHandler = _previousPagedRectTagHandler;
        }        

        public override Dictionary<string, string> attributes
        {
            get
            {
                var keycodeString = System.String.Join(",", System.Enum.GetNames(typeof(KeyCode)));

                return new Dictionary<string, string>()
                {
                    {"defaultPage", "xs:int"},
                    {"autoDiscoverPages", "xs:boolean"},
                    {"showPagination", "xs:boolean"},
                    {"showFirstAndLastButtons", "xs:boolean"},
                    {"showPreviousAndNextButtons", "xs:boolean"},
                    {"maximumNumberOfButtonsToShow", "xs:int"},
                    {"showButtonTemplatesInEditor", "xs:boolean"},
                    {"showPageButtons", "xs:boolean"},
                    {"showNumbersOnButtons", "xs:boolean"},
                    {"showPageTitlesOnButtons", "xs:boolean"},
                    {"animationType", "SlideHorizontal,SlideVertical,Fade"},
                    {"animationSpeed", "xs:float"},
                    {"automaticallyMoveToNextPage", "xs:boolean"},
                    {"delayBetweenPages", "xs:float"},
                    {"loopEndlessly", "xs:boolean"},
                    {"useKeyboardInput", "xs:boolean"},
                    {"previousPageKey", keycodeString},                       
                    {"nextPageKey", keycodeString},                           
                    {"firstPageKey", keycodeString},
                    { "lastPageKey", keycodeString},                           
                    {"useSwipeInput", "xs:boolean"},                        // Note: not relevant to ScrollRect-based PagedRects
                    {"useSwipeInputForScrollRect", "xs:boolean"},
                    {"swipeDeltaThreshold", "xs:float"},
                    {"useScrollWheelInput", "xs:boolean"},
                    {"onlyUseScrollWheelInputWhenMouseIsOver", "xs:boolean"},
                    {"highlightWhenMouseIsOver", "xs:boolean"},
                    {"normalColor", "xmlLayout:color"},
                    {"highlightColor", "xmlLayout:color"},
                    {"editorSelectedPage", "xs:int"},
                    {"color", "xmlLayout:color"},
                    {"showPagePreviews", "xs:boolean"},
                    {"pagePreviewScale", "xs:float"},
                    {"enablePagePreviewOverlays", "xs:boolean"},
                    {"pagePreviewOverlayImage", "xs:string"},
                    {"pagePreviewOverlayNormalColor", "xmlLayout:color"},
                    {"pagePreviewOverlayHoverColor", "xmlLayout:color"},                    
                    {"lockOneToOneScaleRatio", "xs:boolean"},
                    {"spaceBetweenPages", "xs:float"},
                    {"pagePreviewOverlayScaleOverride", "xs:float"},
                    {"loopSeamlessly", "xs:boolean"},
                    {"showScrollbar", "xs:boolean"},
                    {"pageChangedEvent", "xmlLayout:function"}
                };
            }
        }

        protected override List<string> eventAttributeNames
        {
            get
            {
                var eventNames = base.eventAttributeNames;
                eventNames.Add("pagechangedevent");
                
                return eventNames;
            }
        }

        protected override void HandleEventAttribute(string eventName, string eventValue)
        {            
            if (eventName.Equals("pageChangedEvent", StringComparison.OrdinalIgnoreCase))
            {                
                var eventData = eventValue.Trim(new Char[] { ')', ';' })
                                      .Split(',', '(');


                var methodName = eventData[0];

                var layout = currentXmlLayoutInstance;
                var controller = layout.XmlLayoutController;
                var controllerType = controller.GetType();

                var method = controllerType.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (method == null)
                {
                    Debug.LogError("[XmlLayout][XmlLayoutController] No method named '" + methodName + "' has been defined in this XmlLayoutController!");
                    return;
                }

                var methodParameters = method.GetParameters();
                
                pagedRect.PageChangedEvent.RemoveAllListeners();
                pagedRect.PageChangedEvent.AddListener((a, b) =>
                {                                        
                    if (methodParameters.Count() == 0)
                    {
                        method.Invoke(controller, null);
                    }
                    else if (methodParameters.Count() == 1)
                    {
                        method.Invoke(controller, new object[] { a });
                    }
                    else
                    {
                        method.Invoke(controller, new object[] { a, b });
                    }
                });

            }
            else
            {
                base.HandleEventAttribute(eventName, eventValue);
            }
        }

        private static PagedRectTagHandler _previousPagedRectTagHandler { get; set; }
        public static PagedRectTagHandler CurrentPagedRectTagHandler { get; private set; }
    }
}
#endif
