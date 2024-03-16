using RenderPipelineGraph.Runtime.Volumes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RenderPipelineGraph.Runtime.RenderHelper {
    public class TAAHelper {
        public static Matrix4x4 jitterMat(Camera camera) {
            float actualWidth = camera.pixelWidth;
            float actualHeight = camera.pixelHeight;

            var volume = VolumeManager.instance.stack.GetComponent<TemporalAA>();
            int frameIndex = Time.frameCount;
            if (!volume.useTAA.value) return Matrix4x4.identity;
            
            float jitterX = HaltonSequence.Get((frameIndex & 1023) + 1, 2) - 0.5f;
            float jitterY = HaltonSequence.Get((frameIndex & 1023) + 1, 3) - 0.5f;

            var jitter = new Vector2(jitterX, jitterY);
            jitter *= volume.sampleScale.value;
            
            // Debug.Log(jitter);
            float offsetX = jitter.x * (2.0f / actualWidth);
            float offsetY = jitter.y * (2.0f / actualHeight);
            return Matrix4x4.Translate(new Vector3(offsetX,offsetY,0));
        }
    }
}
