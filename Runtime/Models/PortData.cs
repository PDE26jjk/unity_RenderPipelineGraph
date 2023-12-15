using System.Collections.Generic;

namespace RenderPipelineGraph {
    public enum PortType {
        Resource,
        Dependence
    }
    public class PortData {
        public NodeData owner;
        public PortType portType;
        public HashSet<PortData> linkTo = new();
        public string name;
        public static void Connect(PortData p1,PortData p2) {
            p1.linkTo.Add(p2);
            p2.linkTo.Add(p1);
        }
        public static bool Disconnect(PortData p1,PortData p2) {
            if (p1.linkTo.Contains(p2) && p2.linkTo.Contains(p1)) {
                p1.linkTo.Remove(p2);
                p2.linkTo.Remove(p1);
                return true;
            }
            return false;
        }
    }
    public class ResourcePortData:PortData {
        public ResourceType resourceType;
        public ResourcePortData(NodeData owner) {
            this.portType = PortType.Resource;
            this.owner = owner;
        }
    }
    public class DependencePortData:PortData {
        public DependencePortData(NodeData owner) {
            this.portType = PortType.Dependence;
            this.owner = owner;
        }
    }

}
