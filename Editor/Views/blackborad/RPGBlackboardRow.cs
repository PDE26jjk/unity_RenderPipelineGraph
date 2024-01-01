using System;
using RenderPipelineGraph.Interface;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public partial class RPGBlackboardRow : BlackboardRow, IRPGDeletable {
        internal RPGBlackboardField m_Field;
        internal ResourceData Model;
        internal string ResourceName {
            get => m_Field.text;
            set => m_Field.text = value;
        }
        RPGBlackboardRow(RPGBlackboardField item, VisualElement propertyView) : base(item, propertyView) {
            m_Field = item;
        }
        public RPGBlackboardRow(ResourceData model) : this(new RPGBlackboardField() {
            name = "rpg-field"
        }, new RPGBlackboardPropertyView() {
            name = "rpg-properties"
        }) {
            ResourceName = model.name;
            m_Field.typeText = Enum.GetName(typeof(ResourceType), model.type);
            this.Model = model;
            Button button = this.Q<Button>("expandButton");
            if (button != null) {
                // button.clickable.clicked += OnExpand;
            }
        }

        // public void Delete() {
        //     try {
        //         GetFirstAncestorOfType<RPGView>()?.DeleteElements(new GraphElement[] {
        //             this
        //         });
        //     }catch{}
        // }
        public void OnDelete() {
            Debug.Log("delete");
            this.NotifyResourceDeleteVM();
            RemoveFromHierarchy();
        }
    }
}
