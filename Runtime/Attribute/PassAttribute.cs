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
    [AttributeUsage(AttributeTargets.Field)]
    public class DepthAttribute : System.Attribute {
        public int index;
        public DepthAttribute(int index = 0) {
            this.index = index;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultAttribute : System.Attribute {
        public DefaultAttribute() {
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class CullingWhenEmptyAttribute : System.Attribute {
        public CullingWhenEmptyAttribute() {
        }
    }
}
