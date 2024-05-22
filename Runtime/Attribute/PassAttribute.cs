using System;

namespace RenderPipelineGraph.Attribute {

    [AttributeUsage(AttributeTargets.Field)]
    public abstract class ListIndexAttribute : System.Attribute {
        public int[] listIndex;
        protected ListIndexAttribute(int[] listIndex) {
            this.listIndex = listIndex ?? Array.Empty<int>();
        }
    }
    public class ReadAttribute : ListIndexAttribute {
        public ReadAttribute(int[] listIndex = null) : base(listIndex) {

        }
    }
    public class WriteAttribute : ListIndexAttribute {
        public WriteAttribute(int[] listIndex = null) : base(listIndex) {

        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class FragmentAttribute : System.Attribute {
        public int index;
        public int listIndex;
        public FragmentAttribute(int index = 0, int listIndex = 0) {
            this.index = index;
            this.listIndex = listIndex;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class InputAttribute : FragmentAttribute {
        public InputAttribute(int index = 0, int listIndex = 0) : base(index, listIndex) {
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class DepthAttribute : FragmentAttribute {
        public DepthAttribute(int index = 0, int listIndex = 0) : base(index, listIndex) {
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

    [AttributeUsage(AttributeTargets.Field)]
    public class ListSizeAttribute : System.Attribute {
        public int size;
        public ListSizeAttribute(int size) {
            this.size = size;
        }
    }
// [AttributeUsage(AttributeTargets.Field)]
// public class SetGlobalPropertyIdAttribute : System.Attribute {
//     public string propertyIdStr;
//     public SetGlobalPropertyIdAttribute(string propertyIdStr) {
//         this.propertyIdStr = propertyIdStr;
//     }
// }
}
