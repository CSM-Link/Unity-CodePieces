using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XLuaCore.Editor
{
    [CustomPropertyDrawer(typeof(LuaInjection))]
    public class LuaInjectionDrawer : PropertyDrawer
    {
        private PopupField<Component> componentField;
        private PropertyField nameField;
        private HelpBox emptyName, mismatchComponent;

        private SerializedProperty nameProp, componentProp;
        
        private string ComponentItemFormater(Component component)
        {
            if (!component)
            {
                return "GameObject";
            }

            return component.GetType().Name;
        }
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var outerContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            
            emptyName = new HelpBox("The injection name can not be empty.", HelpBoxMessageType.Error);
            mismatchComponent = new HelpBox("The injection component is not matched with the game object.", HelpBoxMessageType.Error);
            
            nameProp = property.FindPropertyRelative("Name");
            componentProp = property.FindPropertyRelative("Component");
            
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                }
            };

            var objField = new PropertyField(property.FindPropertyRelative("Object"), "")
            {
                style =
                {
                    width = new StyleLength(Length.Percent(30f))
                }
            };
            container.Add(objField);
            
            var components = new List<Component>();
            var obj = (GameObject)property.FindPropertyRelative("Object").objectReferenceValue;
            if (obj)
            {
                obj.GetComponents(components);
            }
            components.Insert(0, null);
                
            nameField = new PropertyField(nameProp, "")            
            {
                style =
                {
                    width = new StyleLength(Length.Percent(30f))
                }
            };
            
            componentField = 
                new PopupField<Component>(components, 0, 
                    ComponentItemFormater, ComponentItemFormater)
            {
                style =
                {
                    width = new StyleLength(Length.Percent(40f))
                },
                value = (Component)componentProp.objectReferenceValue
            };

            container.Add(nameField);
            container.Add(componentField);

            if (!obj)
            {
                componentField.visible = false;
            }
            
            objField.RegisterValueChangeCallback(OnObjectChanged);
            nameField.RegisterValueChangeCallback((e) =>
            {
                emptyName.style.display = string.IsNullOrEmpty(e.changedProperty.stringValue)
                                                    ? DisplayStyle.Flex 
                                                    : DisplayStyle.None;
            });
            componentField.RegisterValueChangedCallback(e =>
            {
                mismatchComponent.style.display = !componentField.choices.Contains(e.newValue) 
                                                    ? DisplayStyle.Flex 
                                                    : DisplayStyle.None;
                componentProp.objectReferenceValue = e.newValue;
                componentProp.serializedObject.ApplyModifiedProperties();
            });
            
            outerContainer.Add(container);
            
            outerContainer.Add(emptyName);
            outerContainer.Add(mismatchComponent);
            
            return outerContainer;
        }

        private void Check()
        {
            emptyName.style.display = string.IsNullOrEmpty(nameProp.stringValue)
                                                ? DisplayStyle.Flex 
                                                : DisplayStyle.None;
            mismatchComponent.style.display = !componentField.choices.Contains((Component)componentProp.objectReferenceValue) 
                                                ? DisplayStyle.Flex 
                                                : DisplayStyle.None;
        }

        private void OnObjectChanged(SerializedPropertyChangeEvent e)
        {
            if (e.changedProperty.objectReferenceValue)
            {
                var components = new List<Component>();
                ((GameObject)e.changedProperty.objectReferenceValue).GetComponents(components);
                components.Insert(0, null);
                componentField.choices = components;
                
                componentField.visible = true;
            }
            else
            {
                componentField.visible = false;
            }
            Check();
        }
    }
}
