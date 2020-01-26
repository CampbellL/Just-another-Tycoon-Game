using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UI.Xml
{
    [DisallowMultipleComponent]
    public class XmlLayoutAnimator : MonoBehaviour
    {
        #region Properties
        private XmlElement _xmlElement = null;
        private XmlElement xmlElement
        {
            get
            {
                if (_xmlElement == null) _xmlElement = GetComponent<XmlElement>();
                return _xmlElement;
            }
        }

        //private List<XmlLayoutAnimation> m_currentAnimations = new List<XmlLayoutAnimation>();
        private XmlLayoutAnimationBase m_currentAnimation = null;
        //private float m_currentIndex = 0f;
        //private float m_currentRate = 1f;

        private Queue<XmlLayoutAnimationBase> m_animationQueue = new Queue<XmlLayoutAnimationBase>();

        private static Dictionary<Type, MethodInfo> m_SetValueForCustomAnimationAtIndex_MethodInfoCache = new Dictionary<Type, MethodInfo>();
        private static Dictionary<string, AnimationCurve> curves = new Dictionary<string, AnimationCurve>()
        {
            {"Linear", AnimationCurve.Linear(0, 0, 1, 1)},
            {"EaseInOut", AnimationCurve.EaseInOut(0, 0, 1, 1)},
            {"ExpoEaseIn", AnimationExtensions.GenerateCurve(AnimationExtensions.ExpoEaseIn)},
            {"ExpoEaseOut", AnimationExtensions.GenerateCurve(AnimationExtensions.ExpoEaseOut)},
            {"CubicEaseIn", AnimationExtensions.GenerateCurve(AnimationExtensions.CubicEaseIn)},
            {"CubicEaseOut", AnimationExtensions.GenerateCurve(AnimationExtensions.CubicEaseOut)},
            {"ElasticEaseIn", AnimationExtensions.GenerateCurve(AnimationExtensions.ElasticEaseIn)},
            {"ElasticEaseOut", AnimationExtensions.GenerateCurve(AnimationExtensions.ElasticEaseOut)},
            {"BounceEaseIn", AnimationExtensions.GenerateCurve(AnimationExtensions.BounceEaseIn)},
            {"BounceEaseOut", AnimationExtensions.GenerateCurve(AnimationExtensions.BounceEaseOut)},
            {"BounceEaseInOut", AnimationExtensions.GenerateCurve(AnimationExtensions.BounceEaseInOut)},
            {"BounceEaseOutIn", AnimationExtensions.GenerateCurve(AnimationExtensions.BounceEaseOutIn)},
            {"BackEaseIn", AnimationExtensions.GenerateCurve(AnimationExtensions.BackEaseIn)},
            {"BackEaseOut", AnimationExtensions.GenerateCurve(AnimationExtensions.BackEaseOut)},
        };

        #endregion

        #region Standard Animations
        public void FadeIn(float duration, Action onCompleteCallback = null)
        {
            Animate(0, 1, duration, (v) => xmlElement.CanvasGroup.alpha = v, onCompleteCallback);
        }

        public void FadeOut(float duration, Action onCompleteCallback = null)
        {
            Animate(1, 0, duration, (v) => xmlElement.CanvasGroup.alpha = v, onCompleteCallback);
        }

        public void Grow(float duration, Action onCompleteCallback = null)
        {
            Action<float> setter = (v) => xmlElement.rectTransform.localScale = new Vector3(v, v, v);

            Animate(new List<XmlLayoutAnimationBase>()
            {
                new XmlLayoutAnimation<float>(0, 1.25f, duration * 0.7f, setter),
                new XmlLayoutAnimation<float>(1.25f, 1, duration * 0.3f, setter, "EaseInOut", true, onCompleteCallback),
            });
        }

        public void Grow(float duration, bool horizontal, bool vertical, Action onCompleteCallback = null)
        {
            Action<float> setter = null;

            if (horizontal)
            {
                setter = (v) => xmlElement.rectTransform.localScale = new Vector3(v, 1, 1);
            }
            else
            {
                setter = (v) => xmlElement.rectTransform.localScale = new Vector3(1, v, 1);
            }

            Animate(0, 1, duration, setter, onCompleteCallback);
        }

        public void Shrink(float duration, Action onCompleteCallback = null)
        {
            Action<float> setter = (v) => xmlElement.rectTransform.localScale = new Vector3(v, v, v);

            Animate(new List<XmlLayoutAnimationBase>()
            {
                new XmlLayoutAnimation<float>(1, 1.25f, duration * 0.3f, setter),
                new XmlLayoutAnimation<float>(1.25f, 0, duration * 0.7f, setter, "EaseInOut", true, onCompleteCallback)
            });
        }

        public void Shrink(float duration, bool horizontal, bool vertical, Action onCompleteCallback = null)
        {
            Action<float> setter = null;

            if (horizontal)
            {
                setter = (v) => xmlElement.rectTransform.localScale = new Vector3(v, 1, 1);
            }
            else
            {
                setter = (v) => xmlElement.rectTransform.localScale = new Vector3(1, v, 1);
            }

            Animate(1, 0, duration, setter, onCompleteCallback);
        }
        #endregion

        #region Custom Animations
        public void PlayCustomAnimation(string animationName, Action onCompleteCallback)
        {
            if (!xmlElement.xmlLayoutInstance.animations.ContainsKey(animationName))
            {
                Debug.LogWarningFormat("[XmlLayout] Unrecognised animation name : '{0}'", animationName);
                return;
            }

            List<XmlLayoutAnimationBase> _animation = GetCustomAnimation(animationName, onCompleteCallback);

            if (_animation != null && _animation.Count != 0)
            {
                var lastAnimation = _animation.Last();
                if (lastAnimation.onCompleteCallback == null)
                {
                    lastAnimation.onCompleteCallback = onCompleteCallback;
                }

                Animate(_animation);
            }
        }

        private List<XmlLayoutAnimationBase> GetCustomAnimation(string animationName, Action onCompleteCallback)
        {
            if (!xmlElement.xmlLayoutInstance.animations.ContainsKey(animationName))
            {
                Debug.LogWarningFormat("[XmlLayout] Unrecognised animation name : '{0}'", animationName);
                return null;
            }

            var animation = xmlElement.xmlLayoutInstance.animations[animationName];

            if (animation.type == Xml.XmlLayoutAnimation.eAnimationType.Chained)
            {
                List<XmlLayoutAnimationBase> animationList = new List<XmlLayoutAnimationBase>();

                // chained animation
                foreach (var childAnimation in animation.animations)
                {
                    animationList.AddRange(GetCustomAnimation(childAnimation, null));
                }

                return animationList;
            }
            else if (animation.type == Xml.XmlLayoutAnimation.eAnimationType.Simultaneous)
            {
                XmlLayoutAnimationGroup animationGroup = new XmlLayoutAnimationGroup();

                foreach(var childAnimation in animation.animations)
                {
                    animationGroup.animations.AddRange(GetCustomAnimation(childAnimation, null).Cast<XmlLayoutAnimation>());
                }

                return new List<XmlLayoutAnimationBase>() { animationGroup };
            }

            // Normal animation
            if (!animation.attribute.Contains("."))
            {
                Debug.LogWarningFormat("[XmlLayout] Unrecognised animation target attribute : '{0}'. Animation targets must use the following pattern: ComponentType.PropertyName, e.g. RectTransform.localScale, Image.color, RectTransform.eulerAngles, etc.", animation.attribute);
                return null;
            }

            var componentName = animation.attribute.Substring(0, animation.attribute.IndexOf("."));
            var propertyName = animation.attribute.Replace(componentName + ".", "");

            var component = xmlElement.GetComponent(componentName);
            if (component == null)
            {
                Debug.LogWarningFormat("[XmlLayout] Unable to locate component '{0}'", componentName);
                return null;
            }

            var propertyInfo = component.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                Debug.LogWarningFormat("[XmlLayout] Unable to locate property '{0}' on component '{1}'.", propertyName, componentName);
                return null;
            }

            if (!curves.ContainsKey(animation.curve))
            {
                Debug.LogWarningFormat("[XmlLayout] Invalid curve type '{0}' for animation. Valid values are : {1}", animation.curve, string.Join(", ", curves.Keys.ToArray()));
                return null;
            }


            // Using reflection to avoid having to write the same code for each value type
            MethodInfo method = GetType().GetMethod("GetCustomAnimationOfType", BindingFlags.Instance | BindingFlags.NonPublic)
                                .MakeGenericMethod(new Type[] { animation.valueType });

            XmlLayoutAnimation _animation = (XmlLayoutAnimation)method.Invoke(this, new object[] { animation, propertyInfo, component, onCompleteCallback });

            return new List<XmlLayoutAnimationBase>() { _animation };
        }

        internal XmlLayoutAnimation GetCustomAnimationOfType<T>(Xml.XmlLayoutAnimation animation, PropertyInfo propertyInfo, Component component, Action onCompleteCallback = null)
            where T : struct
        {
            T from;

            if (animation.hasValueFrom)
            {
                from = (T)animation.valueFrom;
            }
            else
            {
                from = (T)propertyInfo.GetMemberValue(component);
            }

            return new XmlLayoutAnimation<T>(from, (T)animation.valueTo, animation.duration, (v) => { propertyInfo.SetMemberValue(component, v); }, animation.curve, animation.hasValueFrom, onCompleteCallback);
        }

        private void SetInitialValueForCustomAnimation<T>(XmlLayoutAnimation animation)
            where T : struct
        {
            var tempAnim = animation as XmlLayoutAnimation<T>;
            tempAnim.setter(tempAnim.from);
        }

        private void SetValueForCustomAnimationAtIndex<T>(XmlLayoutAnimation animation)
            where T : struct
        {
            var tempAnim = animation as XmlLayoutAnimation<T>;
            var value = GetCurveValueAtIndex(animation.curve, tempAnim.from, tempAnim.to, animation.index);

            tempAnim.setter(value);
        }

        private T GetCurveValueAtIndex<T>(string curve, T from, T to, float index)
            where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                return (T)(object)GetCurveValueAtIndex(curve, (float)(object)from, (float)(object)to, index);
            }
            else if (typeof(T) == typeof(Vector2))
            {
                return (T)(object)GetCurveValueAtIndex(curve, (Vector2)(object)from, (Vector2)(object)to, index);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return (T)(object)GetCurveValueAtIndex(curve, (Vector3)(object)from, (Vector3)(object)to, index);
            }
            else if (typeof(T) == typeof(Color))
            {
                return (T)(object)GetCurveValueAtIndex(curve, (Color)(object)from, (Color)(object)to, index);
            }

            return default(T);
        }

        #endregion

        #region Animation code
        /// <summary>
        /// Animate each of the provided animations one after another
        /// </summary>
        /// <param name="animationList"></param>
        /// <returns></returns>
        private void Animate(List<XmlLayoutAnimationBase> animationList)
        {
            this.enabled = true;

            foreach (var animation in animationList)
            {
                m_animationQueue.Enqueue(animation);
            }

            NextAnimation();
        }

        /// <summary>
        /// Smoothly animate a property value from one value to another over the specified duration,
        /// using the provided value setter
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        private void Animate(XmlLayoutAnimationBase animation)
        {
            this.enabled = true;

            if (animation is XmlLayoutAnimation)
            {
                InitializeAnimation(animation as XmlLayoutAnimation);
            }
            else
            {
                var group = animation as XmlLayoutAnimationGroup;

                foreach(var childAnimation in group.animations)
                {
                    InitializeAnimation(childAnimation, false);
                }

                m_currentAnimation = group;
            }
        }

        private void InitializeAnimation(XmlLayoutAnimation animation, bool isSoloAnimation = true)
        {
            if (animation.hasValueFrom)
            {
                // set the initial value
                MethodInfo method = GetType().GetMethod("SetInitialValueForCustomAnimation", BindingFlags.Instance | BindingFlags.NonPublic)
                                             .MakeGenericMethod(new Type[] { animation.valueType });
                method.Invoke(this, new object[] { animation });
            }

            // Cache the MethodInfo if it hasn't been cached already
            // (caching it saves a small amount of time in the Update() method)
            if (!m_SetValueForCustomAnimationAtIndex_MethodInfoCache.ContainsKey(animation.valueType))
            {
                m_SetValueForCustomAnimationAtIndex_MethodInfoCache.Add(
                    animation.valueType,
                    GetType().GetMethod("SetValueForCustomAnimationAtIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                             .MakeGenericMethod(new Type[] { animation.valueType }));
            }

            if (isSoloAnimation) m_currentAnimation = animation;
        }

        private void Animate(float from, float to, float duration, Action<float> setter, Action onCompleteCallback = null)
        {
            Animate(new XmlLayoutAnimation<float>(from, to, duration, setter, "EaseInOut", true, onCompleteCallback));
        }

        /// <summary>
        /// Play the next animation in the queue (if there is one)
        /// </summary>
        private void NextAnimation()
        {
            if (m_animationQueue.Count > 0)
            {
                Animate(m_animationQueue.Dequeue());
            }
            else
            {
                m_currentAnimation = null;

                // Pause update calls
                this.enabled = false;
            }
        }

        private float GetIndexIncrement(float rate)
        {
            return rate * (xmlElement.xmlLayoutInstance.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        #endregion

        #region Curve methods
        private float GetCurveValueAtIndex(string curve, float from, float to, float index)
        {
            return Mathf.Lerp(from, to, curves[curve].Evaluate(index));
        }

        private Vector2 GetCurveValueAtIndex(string curve, Vector2 from, Vector2 to, float index)
        {
            return Vector2.LerpUnclamped(from, to, curves[curve].Evaluate(index));
        }

        private Vector3 GetCurveValueAtIndex(string curve, Vector3 from, Vector3 to, float index)
        {
            return Vector3.LerpUnclamped(from, to, curves[curve].Evaluate(index));
        }

        private Color GetCurveValueAtIndex(string curve, Color from, Color to, float index)
        {
            return Color.LerpUnclamped(from, to, curves[curve].Evaluate(index));
        }
        #endregion

        #region Unity Methods
        List<ushort> m_completedAnimations = new List<ushort>(4);

        private void Update()
        {
            // if there's no current animation, then don't do anything
            if (m_currentAnimation == null) return;

            // Simultaneous animations
            if (m_currentAnimation is XmlLayoutAnimationGroup)
            {
                Update_XmlLayoutAnimationGroup(m_currentAnimation as XmlLayoutAnimationGroup);
            }
            else
            {
                // single animation
                Update_XmlLayoutAnimation(m_currentAnimation as XmlLayoutAnimation);
            }

        }

        private void Update_XmlLayoutAnimation(XmlLayoutAnimation animation)
        {
            animation.index = Mathf.Min(1, animation.index + GetIndexIncrement(animation.rate));

            m_SetValueForCustomAnimationAtIndex_MethodInfoCache[animation.valueType].Invoke(this, new object[] { animation });

            if (animation.index == 1f)
            {
                if (animation.onCompleteCallback != null) animation.onCompleteCallback();

                NextAnimation();
            }
        }

        private void Update_XmlLayoutAnimationGroup(XmlLayoutAnimationGroup animationGroup)
        {
            m_completedAnimations.Clear();

            for (ushort i = 0; i < animationGroup.animations.Count; i++)
            {
                var animation = animationGroup.animations[i];

                animation.index = Mathf.Min(1, animation.index + GetIndexIncrement(animation.rate));

                // Get the MethodInfo from the cache
                m_SetValueForCustomAnimationAtIndex_MethodInfoCache[animation.valueType].Invoke(this, new object[] { animation });

                if (animation.index == 1f)
                {
                    if (animation.onCompleteCallback != null) animation.onCompleteCallback();

                    m_completedAnimations.Add(i);
                }
            }

            if (m_completedAnimations.Count > 0)
            {
                m_completedAnimations.Reverse();

                for (ushort i = 0; i < m_completedAnimations.Count; i++)
                {
                    animationGroup.animations.RemoveAt(m_completedAnimations[i]);
                }

                // Finished the current block, play the next animation (if there is one)
                if (animationGroup.animations.Count == 0)
                {
                    if (animationGroup.onCompleteCallback != null) animationGroup.onCompleteCallback();

                    NextAnimation();
                }
            }
        }
        #endregion

        #region Nested XmlLayoutAnimation classes

        internal abstract class XmlLayoutAnimationBase
        {
            public bool isSimultaneous = false;

            // action called when animation is completed
            public Action onCompleteCallback = null;
        }

        /// <summary>
        /// Abstract base class for animations used by the animator
        /// </summary>
        internal abstract class XmlLayoutAnimation : XmlLayoutAnimationBase
        {
            // duration of the animation (in seconds)
            public float duration;
            // has a 'from' value been set
            public bool hasValueFrom = false;
            // what value type does this animation use
            public Type valueType;
            // what type of curve does this animation use
            public string curve;

            // Current index of this animation
            public float index = 0;

            public float rate = 0;
        }

        internal class XmlLayoutAnimationGroup : XmlLayoutAnimationBase
        {
            public List<XmlLayoutAnimation> animations = new List<XmlLayoutAnimation>();
        }

        /// <summary>
        /// Generic class for animations used by the animator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class XmlLayoutAnimation<T> : XmlLayoutAnimation where T : struct
        {
            // initial value
            public T from;
            // final value
            public T to;
            // action used to set the value
            public Action<T> setter;

            public XmlLayoutAnimation(T from, T to, float duration, Action<T> setter, string curve = "Linear", bool hasValueFrom = true, Action onCompleteCallback = null)
            {
                this.from = from;
                this.to = to;
                this.duration = duration;
                this.setter = setter;
                this.curve = curve;
                this.onCompleteCallback = onCompleteCallback;
                this.hasValueFrom = hasValueFrom;
                this.valueType = typeof(T);

                this.rate = 1.0f / duration;
            }
        }

        /// <summary>
        /// As some methods are _only_ called via reflection;
        /// we need to call them somewhere in code that is not actually ever executed.
        /// This prevents the methods from being stripped out by IL2CPP optimization
        /// </summary>
        private static class XmlLayoutAnimatorCompilerHints
        {
            public static void Hint()
            {
                var animator = new XmlLayoutAnimator();
                animator.SetInitialValueForCustomAnimation<float>(null);
                animator.SetInitialValueForCustomAnimation<int>(null);
                animator.SetInitialValueForCustomAnimation<Vector2>(null);
                animator.SetInitialValueForCustomAnimation<Vector3>(null);
                animator.SetInitialValueForCustomAnimation<Color>(null);

                animator.SetValueForCustomAnimationAtIndex<float>(null);
                animator.SetValueForCustomAnimationAtIndex<int>(null);
                animator.SetValueForCustomAnimationAtIndex<Vector2>(null);
                animator.SetValueForCustomAnimationAtIndex<Vector3>(null);
                animator.SetValueForCustomAnimationAtIndex<Color>(null);
            }
        }
        #endregion
    }
}