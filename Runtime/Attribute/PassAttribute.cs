using System;

namespace RenderPipelineGraph.Attribute {
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadAttribute : System.Attribute {
        public ReadAttribute() {

        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class WriteAttribute : System.Attribute {
        public WriteAttribute() {

        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class FragmentAttribute : System.Attribute {
        public int index;
        public FragmentAttribute(int index = 0) {
            this.index = index;
        }
    }
}
