using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace RenderPipelineGraph.Editor.AssetFile {
    [Obsolete("no use")]
    public class RPGImporter : ScriptedImporter {

        public const string Extension = "rpg";
        public override void OnImportAsset(AssetImportContext ctx) {
            // var importLog = new AssetImportErrorLog(ctx);
            string path = ctx.assetPath;
            var assetGuid = AssetDatabase.AssetPathToGUID(path);
            var textGraph = File.ReadAllText(path, Encoding.UTF8);
            
        } 
    }
}
