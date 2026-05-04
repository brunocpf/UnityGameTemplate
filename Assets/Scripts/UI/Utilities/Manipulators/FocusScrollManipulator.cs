using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Smoothly scrolls a <see cref="ScrollView"/> so that whichever child receives focus
    /// is always fully visible. Apply via <c>scrollView.AddManipulator(new FocusScrollManipulator())</c>.
    /// </summary>
    public sealed class FocusScrollManipulator : Manipulator
    {
        private readonly float _scrollDuration;

        private MotionHandle _scrollMotion;

        public FocusScrollManipulator(float scrollDuration = 0.2f) : base()
        {
            _scrollDuration = scrollDuration;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target is not ScrollView)
            {
                throw new InvalidOperationException(
                    $"{nameof(FocusScrollManipulator)} requires a {nameof(ScrollView)} target.");
            }

            target.contentContainer.RegisterCallback<FocusInEvent>(OnFocusIn);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.contentContainer.UnregisterCallback<FocusInEvent>(OnFocusIn);
            target.UnregisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            CancelScrollMotion();
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (evt.target is not VisualElement focused)
            {
                return;
            }

            _ = focused.schedule.Execute(() => AnimateScrollTo(focused));
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            CancelScrollMotion();
        }

        private void AnimateScrollTo(VisualElement focused)
        {
            if (focused.panel == null || target is not ScrollView scrollView)
            {
                return;
            }

            Rect element = focused.worldBound;
            Rect viewport = scrollView.contentViewport.worldBound;
            Vector2 current = scrollView.scrollOffset;
            Vector2 destination = current;

            if (element.yMin < viewport.yMin)
            {
                destination.y = current.y - (viewport.yMin - element.yMin);
            }
            else if (element.yMax > viewport.yMax)
            {
                destination.y = current.y + (element.yMax - viewport.yMax);
            }

            if (element.xMin < viewport.xMin)
            {
                destination.x = current.x - (viewport.xMin - element.xMin);
            }
            else if (element.xMax > viewport.xMax)
            {
                destination.x = current.x + (element.xMax - viewport.xMax);
            }

            if (destination == current)
            {
                return;
            }

            CancelScrollMotion();
            _scrollMotion = LMotion.Create(current, destination, _scrollDuration)
                .WithEase(Ease.OutCubic)
                .Bind(offset => scrollView.scrollOffset = offset);
        }

        private void CancelScrollMotion()
        {
            if (_scrollMotion.IsActive())
            {
                _scrollMotion.Cancel();
            }
        }
    }
}
