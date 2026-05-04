using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    public interface IRouteTransition
    {
        public UniTask Enter(VisualElement target, CancellationToken ct);
        public UniTask Exit(VisualElement target, CancellationToken ct);
        public UniTask Cover(VisualElement target, CancellationToken ct);
        public UniTask Reveal(VisualElement target, CancellationToken ct);
    }
}
