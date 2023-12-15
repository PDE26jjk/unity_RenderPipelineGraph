using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph.Editor.Views.blackborad {
    public class RPGBlackboardCategory: GraphElement {
        VisualElement m_DragIndicator;
        VisualElement m_MainContainer;
        VisualElement m_Header;
        Label m_TitleLabel;
        Foldout m_Foldout;
        TextField m_TextField;
        public RPGBlackboardCategory() {
            var tpl = Resources.Load("UXML/RPGBlackboardCategory") as VisualTreeAsset;
            m_MainContainer = tpl.CloneTree();
            m_MainContainer.AddToClassList("mainContainer");
            m_Header = m_MainContainer.Q<VisualElement>("sectionHeader");
            m_TitleLabel = m_MainContainer.Q<Label>("sectionTitleLabel");
        }
    }
}
