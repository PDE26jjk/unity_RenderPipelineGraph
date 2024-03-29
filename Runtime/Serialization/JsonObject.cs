﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RenderPipelineGraph.Serialization {
    // Almost Copy From ShaderGraph.
    [Serializable]
    public class JsonObject : ISerializationCallbackReceiver {

        [SerializeField]
        string m_Type;
        
        [SerializeField]
        string m_ObjectId = Guid.NewGuid().ToString("N");
        
        public string objectId => m_ObjectId;
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Type = $"{GetType().FullName}";
            OnBeforeSerialize();
        }
        
        public virtual string Serialize() {
            // return EditorJsonUtility.ToJson(this, true); // This can handle Unity Object but cannot be build and it is useless in this project.
            return JsonUtility.ToJson(this, true);
        }
        public virtual void Deserailize(string typeInfo, string jsonData) {
            // EditorJsonUtility.FromJsonOverwrite(jsonData, this);// see comment above.
            JsonUtility.FromJsonOverwrite(jsonData, this);
        }
        public virtual T CastTo<T>() where T : JsonObject { return (T)this; }
        
        public virtual void OnBeforeSerialize() { }

        public virtual void OnBeforeDeserialize() { }

        public virtual void OnAfterDeserialize() { }

        public virtual void OnAfterDeserialize(string json) { }

        public virtual void OnAfterMultiDeserialize(string json) { }
        
        internal static Guid GenerateNamespaceUUID(string Namespace, string Name) {
            Guid namespaceGuid;
            if (!Guid.TryParse(Namespace, out namespaceGuid)) {
                // Fallback namespace in case the one provided is invalid.
                // If an object ID was used as the namespace, this shouldn't normally be reachable.
                namespaceGuid = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c0");
            }
            return GenerateNamespaceUUID(namespaceGuid, Name);
        }
        
        internal static Guid GenerateNamespaceUUID(Guid Namespace, string Name) {
            // Generate a deterministic guid using namespace guids: RFC 4122 §4.3 version 5.
            void FlipByNetworkOrder(byte[] bytes) {
                bytes = new byte[] {
                    bytes[3],
                    bytes[2],
                    bytes[1],
                    bytes[0],
                    bytes[5],
                    bytes[4],
                    bytes[7],
                    bytes[6]
                };
            }

            var namespaceBytes = Namespace.ToByteArray();
            FlipByNetworkOrder(namespaceBytes);
            var nameBytes = Encoding.UTF8.GetBytes(Name);
            var hash = SHA1.Create().ComputeHash(namespaceBytes.Concat(nameBytes).ToArray());
            byte[] newguid = new byte[16];
            Array.Copy(hash, newguid, 16);
            newguid[6] = (byte)((newguid[6] & 0x0F) | 0x80);
            newguid[8] = (byte)((newguid[8] & 0x3F) | 0x80);
            FlipByNetworkOrder(newguid);
            return new Guid(newguid);
        }
    }
}
