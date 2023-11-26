using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class RPGWindow : EditorWindow {
    [MenuItem("Window/Render Pipeline/Render Pipeline Graph", false, 3012)]
    public static void ShowWindow() {
        var win = windows.SingleOrDefault();
        if (!win) {
            win = CreateInstance<RPGWindow>();
            windows.Add(win);
        }
        // win.titleContent = EditorGUIUtility.TrTextContent("hahah");
        win.titleContent.text = "haha";
        win.Show();
        win.Focus();
        // GetWindow((VisualEffectResource)null, true);
    }
    RPGView graphView;
    static List<RPGWindow> windows = new();
    ShortcutHandler m_ShortcutHandler;
    protected void SetupFramingShortcutHandler(RPGView view)
    {
        m_ShortcutHandler = new ShortcutHandler(
            new Dictionary<Event, ShortcutDelegate>
            {
                { Event.KeyboardEvent("a"), view.FrameAll },
                { Event.KeyboardEvent("f"), view.FrameSelection },
                { Event.KeyboardEvent("o"), view.FrameOrigin },
                { Event.KeyboardEvent("^#>"), view.FramePrev },
                { Event.KeyboardEvent("^>"), view.FrameNext },

            });
    }
    protected void CreateGUI() {
        graphView = new();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
        if (rootVisualElement.panel != null)
        {
            rootVisualElement.AddManipulator(m_ShortcutHandler);
        }
    }
    protected void OnDestroy() {
        windows.Remove(this);
    }
}
