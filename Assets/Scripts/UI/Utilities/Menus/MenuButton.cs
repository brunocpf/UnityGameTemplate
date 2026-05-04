using System;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// A focusable button with two independent disable axes:
    ///   * <see cref="SoftDisabled"/> — content-driven (e.g. not enough MP, dead target)
    ///   * <see cref="SetInteractive"/> — stack-driven (e.g. router-managed activation)
    /// Either axis being "off" suppresses pointer hit-testing (and therefore <c>:hover</c> styling
    /// and the <see cref="HoverFocusManipulator"/>) and intercepts click/submit events.
    /// </summary>
    [UxmlElement]
    public partial class MenuButton : Button
    {
        private const string _rootClass = "menu-button";
        private const string _softDisabledUssClassName = _rootClass + "--soft-disabled";
        private const string _softActiveUssClassName = _rootClass + "--soft-active";

        private bool _softDisabled;
        private bool _softActive;

        public MenuButton() : this(null) { }

        public MenuButton(Action? clickEvent) : base(clickEvent)
        {
            AddToClassList(_rootClass);
            this.AddManipulator(new HoverFocusManipulator());
            RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
        }

        [UxmlAttribute]
        public bool SoftDisabled
        {
            get => _softDisabled;
            set
            {
                _softDisabled = value;
                EnableInClassList(_softDisabledUssClassName, value);
            }
        }

        [UxmlAttribute]
        public bool SoftActive
        {
            get => _softActive;
            set
            {
                _softActive = value;
                EnableInClassList(_softActiveUssClassName, value);
            }
        }

        private void OnClick(ClickEvent evt)
        {
            if (_softDisabled)
            {
                evt.StopImmediatePropagation();
            }
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            if (_softDisabled)
            {
                evt.StopImmediatePropagation();
            }
        }
    }
}
