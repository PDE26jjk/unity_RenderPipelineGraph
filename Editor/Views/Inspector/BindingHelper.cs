using System;
using UnityEngine;

namespace RenderPipelineGraph.Editor {
    public class BindingHelper<T> : ScriptableObject where T: RPGModel {
        [NonSerialized]
        public T Model;
    }
}
