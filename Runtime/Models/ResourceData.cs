using UnityEngine.Rendering.RenderGraphModule;

namespace RenderPipelineGraph {

    public enum ResourceType {
        Texture,
        Buffer,
        AccelerationStructure,
    }
    public enum UseType {
        Imported,
        Transient,
        Shared,
    }
    public class ResourceData : RPGModel {
        public ResourceType type;
        public UseType useType;
        public string name;
    }
    public class BufferData : ResourceData {
        public BufferData() {
            type = ResourceType.Buffer;
        }
        public BufferDesc desc;
    }
    public class TextureData : ResourceData {
        public TextureData() {
            type = ResourceType.Texture;
        }
        public TextureDesc desc;
    }

}
