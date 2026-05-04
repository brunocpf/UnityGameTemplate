using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    public static class RouteTransitions
    {
        public static IRouteTransition None { get; } = new NoneTransition();

        public static IRouteTransition Fade(float duration, float coveredOpacity = 0.5f)
        {
            return new FadeTransition(duration, coveredOpacity);
        }

        private sealed class NoneTransition : IRouteTransition
        {
            public UniTask Enter(VisualElement target, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask Exit(VisualElement target, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask Cover(VisualElement target, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask Reveal(VisualElement target, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }
        }

        private sealed class FadeTransition : IRouteTransition
        {
            private readonly float _duration;
            private readonly float _coveredOpacity;

            public FadeTransition(float duration, float coveredOpacity)
            {
                _duration = duration;
                _coveredOpacity = coveredOpacity;
            }

            public UniTask Enter(VisualElement target, CancellationToken ct)
            {
                return AnimateOpacity(target, 0f, 1f, _duration, ct);
            }

            public UniTask Exit(VisualElement target, CancellationToken ct)
            {
                float from = target.resolvedStyle.opacity;
                return AnimateOpacity(target, from, 0f, _duration, ct);
            }

            public UniTask Cover(VisualElement target, CancellationToken ct)
            {
                float from = target.resolvedStyle.opacity;
                return AnimateOpacity(target, from, _coveredOpacity, _duration, ct);
            }

            public UniTask Reveal(VisualElement target, CancellationToken ct)
            {
                float from = target.resolvedStyle.opacity;
                return AnimateOpacity(target, from, 1f, _duration, ct);
            }
        }

        private static async UniTask AnimateOpacity(VisualElement target, float from, float to, float duration, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                target.style.opacity = to;
                return;
            }

            UniTaskCompletionSource tcs = new();
            MotionHandle handle = LMotion.Create(from, to, duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => tcs.TrySetResult())
                .Bind(value => target.style.opacity = value);

            using CancellationTokenRegistration registration = ct.Register(() =>
            {
                if (handle.IsActive())
                {
                    handle.Cancel();
                }

                tcs.TrySetCanceled();
            });

            try
            {
                await tcs.Task;
            }
            catch (System.OperationCanceledException)
            {
                // Cancellation is expected when the route is interrupted; let caller handle.
                throw;
            }
        }
    }
}
