using UnityEngine;
using UnityEngine.UIElements;
using static Poi.Tools.ModularShadersGeneratorWindow;

namespace Poi.Tools
{
    public class ShaderDestinationListElement : VisualElement
    {
        TextField folderPath, matchString, versionOverrideField;
        Button browseButton;
        Toggle enabledToggle;
#if UNITY_2022_1_OR_NEWER
        EnumField matchType;
#endif

        ShaderDestinationManager.ShaderDestination destinationItem;

        public ShaderDestinationListElement()
        {
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("Poi/ShaderDestinationListElement");
            Add(tree.CloneTree());

            enabledToggle = this.Q<Toggle>("enabledToggle");
            folderPath = this.Q<TextField>("folderPath");
            browseButton = this.Q<Button>("browseButton");
            matchString = this.Q<TextField>("nameMatch");
            versionOverrideField = this.Q<TextField>("versionOverrideField");

            browseButton.clicked += () => ShowFolderSelector(folderPath);
#if UNITY_2022_1_OR_NEWER
            matchType = this.Q<EnumField>("matchType");
            matchType.RegisterValueChangedCallback(evt =>
            {
                matchString.SetEnabled(MatchTypeEnablesTextField((ShaderDestinationManager.ShaderDestination.MatchType)evt.newValue));
            });
#endif
        }

        bool MatchTypeEnablesTextField(ShaderDestinationManager.ShaderDestination.MatchType matchType)
        {
            return matchType != ShaderDestinationManager.ShaderDestination.MatchType.Always;
        }

        public void BindListItem(ShaderDestinationManager.ShaderDestination item)
        {
            destinationItem = item;
            enabledToggle.SetValueWithoutNotify(item.enabled);
            enabledToggle.RegisterValueChangedCallback(HandleEnableToggleValue);

            folderPath.SetValueWithoutNotify(item.folderPath);
            folderPath.RegisterValueChangedCallback(HandleFolderPathTextFieldValue);

            matchString.SetValueWithoutNotify(item.matchString);
            matchString.RegisterValueChangedCallback(HandleMatchStringTextFieldValue);
            matchString.SetEnabled(MatchTypeEnablesTextField(item.matchType));

            versionOverrideField.SetValueWithoutNotify(item.versionOverride);
            versionOverrideField.RegisterValueChangedCallback(HandleVersionOverrideFieldValue);

#if UNITY_2022_1_OR_NEWER
            matchType.SetValueWithoutNotify(item.matchType);
            matchType.RegisterValueChangedCallback(HandleMatchTypeEnumFieldValue);
#endif
        }

        public void UnbindListItem()
        {
            destinationItem = null;
            enabledToggle.UnregisterValueChangedCallback(HandleEnableToggleValue);
            folderPath.UnregisterValueChangedCallback(HandleFolderPathTextFieldValue);
            matchString.UnregisterValueChangedCallback(HandleMatchStringTextFieldValue);
            versionOverrideField.UnregisterValueChangedCallback(HandleVersionOverrideFieldValue);
#if UNITY_2022_1_OR_NEWER
            matchType.UnregisterValueChangedCallback(HandleMatchTypeEnumFieldValue);
#endif
        }

        void HandleEnableToggleValue(ChangeEvent<bool> evt) => destinationItem.enabled = evt.newValue;
        void HandleFolderPathTextFieldValue(ChangeEvent<string> evt) => destinationItem.folderPath = evt.newValue;
        void HandleMatchStringTextFieldValue(ChangeEvent<string> evt) => destinationItem.matchString = evt.newValue;
        void HandleVersionOverrideFieldValue(ChangeEvent<string> evt) => destinationItem.versionOverride = evt.newValue;
        void HandleMatchTypeEnumFieldValue(ChangeEvent<System.Enum> evt) => destinationItem.matchType = (ShaderDestinationManager.ShaderDestination.MatchType)evt.newValue;
    }
}