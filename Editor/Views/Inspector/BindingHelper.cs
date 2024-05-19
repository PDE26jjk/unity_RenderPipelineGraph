using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RenderPipelineGraph.Editor {
    public class BindingHelper<T> : ScriptableObject where T : RPGModel {
        [NonSerialized]
        public T Model;
    }
    internal class PlaceholderObject : ScriptableObject {
    }
}
