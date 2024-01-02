using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace RenderPipelineGraph.Serialization {
    static class MultiJsonInternal {
        public class UnknownJsonObject : JsonObject {
            public string typeInfo;
            public string jsonData;
            public JsonData<JsonObject> castedObject;

            public UnknownJsonObject(string typeInfo) {
                this.typeInfo = typeInfo;
            }

            public override void Deserailize(string typeInfo, string jsonData) {
                this.jsonData = jsonData;
            }

            public override string Serialize() {
                return jsonData;
            }

            public override void OnAfterDeserialize(string json) {
                if (castedObject.value != null) {
                    Enqueue(castedObject, json.Trim());
                }
            }

            public override void OnAfterMultiDeserialize(string json) {
                if (castedObject.value == null) {
                    // Never got casted so nothing ever reffed this object
                    // likely that some other unknown json object had a ref
                    // to this thing. Need to include it in the serialization
                    // step of the object still.
                    if (jsonBlobs.TryGetValue(currentRoot.objectId, out var blobs)) {
                        blobs[objectId] = jsonData.Trim();
                    }
                    else {
                        var lookup = new Dictionary<string, string>();
                        lookup[objectId] = jsonData.Trim();
                        jsonBlobs.Add(currentRoot.objectId, lookup);
                    }
                }
            }

            public override T CastTo<T>() {
                if (castedObject.value != null)
                    return castedObject.value.CastTo<T>();

                Type t = typeof(T);

                {
                    Debug.LogError($"Unable to evaluate type {typeInfo} : {jsonData}");
                }
                return null;
            }
        }

        static readonly Dictionary<string, Type> k_TypeMap = CreateTypeMap();

        internal static bool isDeserializing;

        internal static readonly Dictionary<string, JsonObject> valueMap = new();

        static List<MultiJsonEntry> s_Entries;

        internal static bool isSerializing;

        internal static readonly List<JsonObject> serializationQueue = new();

        internal static readonly HashSet<string> serializedSet = new();

        static JsonObject currentRoot;

        static Dictionary<string, Dictionary<string, string>> jsonBlobs = new();

        static Dictionary<string, Type> CreateTypeMap() {
            var map = new Dictionary<string, Type>();
#if UNITY_EDITOR
            foreach (var type in TypeCache.GetTypesDerivedFrom<JsonObject>()) 
#else
            // https://discussions.unity.com/t/how-to-find-all-classes-deriving-from-a-base-class/240655
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(JsonObject)))) 
#endif
            {
                if (type.FullName != null) {
                    map[type.FullName] = type;
                }
            }

            // foreach (var type in TypeCache.GetTypesWithAttribute(typeof(FormerNameAttribute)))
            // {
            //     if (type.IsAbstract || !typeof(JsonObject).IsAssignableFrom(type))
            //     {
            //         continue;
            //     }
            //
            //     foreach (var attribute in type.GetCustomAttributes(typeof(FormerNameAttribute), false))
            //     {
            //         var legacyAttribute = (FormerNameAttribute)attribute;
            //         map[legacyAttribute.fullName] = type;
            //     }
            // }

            return map;
        }

        public static Type ParseType(string typeString) {
            k_TypeMap.TryGetValue(typeString, out var type);
            return type;
        }

        public static List<MultiJsonEntry> Parse(string str) {
            var result = new List<MultiJsonEntry>();
            const string separatorStr = "\n\n";
            var startIndex = 0;
            var raw = new FakeJsonObject();

            while (startIndex < str.Length) {
                var jsonBegin = str.IndexOf("{", startIndex, StringComparison.Ordinal);
                if (jsonBegin == -1) {
                    break;
                }

                var jsonEnd = str.IndexOf(separatorStr, jsonBegin, StringComparison.Ordinal);
                if (jsonEnd == -1) {
                    jsonEnd = str.IndexOf("\n\r\n", jsonBegin, StringComparison.Ordinal);
                    if (jsonEnd == -1) {
                        jsonEnd = str.LastIndexOf("}", StringComparison.Ordinal) + 1;
                    }
                }

                var json = str.Substring(jsonBegin, jsonEnd - jsonBegin);

                JsonUtility.FromJsonOverwrite(json, raw);
                if (startIndex != 0 && string.IsNullOrWhiteSpace(raw.type)) {
                    throw new InvalidOperationException($"Type is null or whitespace in JSON:\n{json}");
                }

                result.Add(new MultiJsonEntry(raw.type, raw.id, json));
                raw.Reset();

                startIndex = jsonEnd + separatorStr.Length;
            }

            return result;
        }

        public static void Enqueue(JsonObject jsonObject, string json) {
            if (s_Entries == null) {
                throw new InvalidOperationException("Can only Enqueue during JsonObject.OnAfterDeserialize.");
            }

            valueMap.Add(jsonObject.objectId, jsonObject);
            s_Entries.Add(new MultiJsonEntry(jsonObject.GetType().FullName, jsonObject.objectId, json));
        }

        public static JsonObject CreateInstanceForDeserialization(string typeString) {
            if (!k_TypeMap.TryGetValue(typeString, out var type)) {
                return new UnknownJsonObject(typeString);
            }
            var output = (JsonObject)Activator.CreateInstance(type, true);
            //This CreateInstance function is supposed to essentially create a blank copy of whatever class we end up deserializing into.
            //when we typically create new JsonObjects in all other cases, we want that object to be assumed to be the latest version.
            //This doesn't work if any json object was serialized before we had the idea of version, as the blank copy would have the
            //latest version on creation and since the serialized version wouldn't have a version member, it would not get overwritten
            //and we would automatically upgrade all previously serialized json objects incorrectly and without user action. To avoid this,
            //we default jsonObject version to 0, and if the serialized value has a different saved version it gets changed and if the serialized
            //version does not have a different saved value it remains 0 (earliest version)
            // output.ChangeVersion(0);
            output.OnBeforeDeserialize();
            return output;
        }

        private static FieldInfo s_ObjectIdField =
            typeof(JsonObject).GetField("m_ObjectId", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Deserialize(JsonObject root, List<MultiJsonEntry> entries, bool rewriteIds) {
            if (isDeserializing) {
                throw new InvalidOperationException("Nested MultiJson deserialization is not supported.");
            }

            try {
                isDeserializing = true;
                currentRoot = root;
                // root.ChangeVersion(0); //Same issue as described in CreateInstance
                for (var index = 0; index < entries.Count; index++) {
                    var entry = entries[index];
                    try {
                        JsonObject value = null;
                        if (index == 0) {
                            value = root;
                        }
                        else {
                            value = CreateInstanceForDeserialization(entry.type);
                        }

                        var id = entry.id;

                        if (id != null) {
                            // Need to make sure that references looking for the old ID will find it in spite of
                            // ID rewriting.
                            valueMap[id] = value;
                        }

                        if (rewriteIds || entry.id == null) {
                            id = value.objectId;
                            entries[index] = new MultiJsonEntry(entry.type, id, entry.json);
                            valueMap[id] = value;
                        }

                        s_ObjectIdField.SetValue(value, id);
                    }
                    catch (Exception e) {
                        // External code could throw exceptions, but we don't want that to fail the whole thing.
                        // Potentially, the fallback type should also be used here.
                        Debug.LogException(e);
                    }
                }

                s_Entries = entries;

                // Not a foreach because `entries` can be populated by calls to `Enqueue` as we go.
                for (var i = 0; i < entries.Count; i++) {
                    var entry = entries[i];
                    try {
                        var value = valueMap[entry.id];
                        value.Deserailize(entry.type, entry.json);
                        // Set ID again as it could be overwritten from JSON.
                        s_ObjectIdField.SetValue(value, entry.id);
                        value.OnAfterDeserialize(entry.json);
                    }
                    catch (Exception e) {
                        if (!String.IsNullOrEmpty(entry.id)) {
                            var value = valueMap[entry.id];
                            if (value != null) {
                                Debug.LogError($"Exception thrown while deserialize object of type {entry.type}: {e.Message}");
                            }
                        }
                        Debug.LogException(e);
                    }
                }

                s_Entries = null;

                foreach (var entry in entries) {
                    try {
                        var value = valueMap[entry.id];
                        value.OnAfterMultiDeserialize(entry.json);
                    }
                    catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
            finally {
                valueMap.Clear();
                currentRoot = null;
                isDeserializing = false;
            }
        }

        public static string Serialize(JsonObject mainObject) {
            if (isSerializing) {
                throw new InvalidOperationException("Nested MultiJson serialization is not supported.");
            }

            try {
                isSerializing = true;

                serializedSet.Add(mainObject.objectId);
                serializationQueue.Add(mainObject);

                var idJsonList = new List<(string, string)>();

                // Not a foreach because the queue is populated by `JsonData<T>`s as we go.
                for (var i = 0; i < serializationQueue.Count; i++) {
                    var value = serializationQueue[i];
                    var json = value.Serialize();
                    idJsonList.Add((value.objectId, json));
                }

                if (jsonBlobs.TryGetValue(mainObject.objectId, out var blobs)) {
                    foreach (var blob in blobs) {
                        if (!idJsonList.Contains((blob.Key, blob.Value)))
                            idJsonList.Add((blob.Key, blob.Value));
                    }
                }


                idJsonList.Sort((x, y) =>
                    // Main object needs to be placed first
                    x.Item1 == mainObject.objectId ? -1 :
                    y.Item1 == mainObject.objectId ? 1 :
                    // We sort everything else by ID to consistently maintain positions in the output
                    x.Item1.CompareTo(y.Item1));


                const string k_NewLineString = "\n";
                var sb = new StringBuilder();
                foreach (var (id, json) in idJsonList) {
                    sb.Append(json);
                    sb.Append(k_NewLineString);
                    sb.Append(k_NewLineString);
                }

                return sb.ToString();
            }
            finally {
                serializationQueue.Clear();
                serializedSet.Clear();
                isSerializing = false;
            }
        }

        public static void PopulateValueMap(JsonObject mainObject) {
            if (isSerializing) {
                throw new InvalidOperationException("Nested MultiJson serialization is not supported.");
            }

            try {
                isSerializing = true;

                serializedSet.Add(mainObject.objectId);
                serializationQueue.Add(mainObject);

                // Not a foreach because the queue is populated by `JsonRef<T>`s as we go.
                for (var i = 0; i < serializationQueue.Count; i++) {
                    var value = serializationQueue[i];
                    value.Serialize();
                    valueMap[value.objectId] = value;
                }
            }
            finally {
                serializationQueue.Clear();
                serializedSet.Clear();
                isSerializing = false;
            }
        }
    }
}
