using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RenderPipelineGraph {
    public class RPGWindow : EditorWindow {
        // [MenuItem("Window/Render Pipeline/Render Pipeline Graph", false, 3012)]
        // public static void ShowWindow() {
        //     var win = windows.SingleOrDefault();
        //     if (!win) {
        //         win = CreateInstance<RPGWindow>();
        //         windows.Add(win);
        //     }
        //     // win.titleContent = EditorGUIUtility.TrTextContent("hahah");
        //     win.titleContent.text = "haha";
        //     win.Show();
        //     win.Focus();
        // } 
        public static void ShowWindow(RPGAsset rpgAsset) {
            foreach (RPGWindow window in windows) {
                if (window.graphView?.Asset == rpgAsset) {
                    window.Show();
                    window.Focus();
                    return;
                }
            }
            var win = CreateInstance<RPGWindow>();
            windows.Add(win);
            win.graphView = new(rpgAsset);
            win.titleContent.text = rpgAsset.name; 
            win.Show();
            win.Focus();
            // GetWindow((VisualEffectResource)null, true);
        }
        internal RPGView graphView;
        static List<RPGWindow> windows = new();
        ShortcutHandler m_ShortcutHandler;
        protected void SetupFramingShortcutHandler(RPGView view) {
            m_ShortcutHandler ??= new ShortcutHandler(
                new Dictionary<Event, ShortcutDelegate> {
                    {
                        Event.KeyboardEvent("a"), view.FrameAll
                    }, {
                        Event.KeyboardEvent("f"), view.FrameSelection
                    }, {
                        Event.KeyboardEvent("o"), view.FrameOrigin
                    }, {
                        Event.KeyboardEvent("^#>"), view.FramePrev
                    }, {
                        Event.KeyboardEvent("^>"), view.FrameNext
                    },

                });
        }
        protected void CreateGUI() {
            if (graphView == null) return;
            rootVisualElement.Add(graphView);
            SetupFramingShortcutHandler(graphView);
            if (rootVisualElement.panel != null) {
                rootVisualElement.AddManipulator(m_ShortcutHandler);
            }
        }
        protected void OnDestroy() {
            windows.Remove(this);
        }

        void OnFocus() {
            if (graphView != null) {
                graphView.StretchToParentSize();
                graphView.OnFocus();
            }
        }
    }
}
