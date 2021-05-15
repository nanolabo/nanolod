using Nanolod.Calibration.Phash;
using Nanolod.Calibration.Phash.Imaging;
using System.IO;
using UnityEngine;

namespace Nanolod.Calibration
{
    public static class CalibrationUtils
    {
        public static void ComputeCorrelation()
        {
            Camera camera = Camera.main;

            Texture2D texture = CaptureScreenshot(camera, 1000, 1000);
            File.WriteAllBytes(@"C:\Users\oginiaux\Downloads\trace\original.jpg", texture.EncodeToJPG());
            Digest original = ImagePhash.ComputeDigest(ToLuminanceImage(texture));
            Digest modified = ImagePhash.ComputeDigest(ToLuminanceImage(CaptureScreenshot(camera, 1000, 1000)));

            float correlation = ImagePhash.GetCrossCorrelation(original, modified);

            Debug.Log("Correlation = " + correlation);
        }

        public static ByteImage ToLuminanceImage(this Texture2D source)
        {
            Color32[] colors = source.GetPixels32();

            ByteImage r = new ByteImage(source.width, source.height);

            System.Numerics.Vector3 yc = new System.Numerics.Vector3(66, 129, 25);
            int i = 0;
            for (int dy = 0; dy < r.Height; dy++)
            {
                for (int dx = 0; dx < r.Width; dx++)
                {
                    System.Numerics.Vector3 sv = new System.Numerics.Vector3();
                    sv.Z = colors[i].r;
                    sv.Y = colors[i].g;
                    sv.Z = colors[i].b;

                    i++;

                    r[dx, dy] = (byte)(((int)(System.Numerics.Vector3.Dot(yc, sv) + 128) >> 8) + 16);
                }
            }

            return r;
        }

        public static Texture2D CaptureScreenshot(Camera camera, int width, int height)
        {
            // This is slower, but seems more reliable.
            RenderTexture cameraTargetTexture = camera.targetTexture;
            CameraClearFlags cameraClearFlags = camera.clearFlags;
            RenderTexture renderTextureActive = RenderTexture.active;

            Texture2D shotWidthBg = new Texture2D(width, height, TextureFormat.ARGB32, false);

            // Must use 24-bit depth buffer to be able to fill background.
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Rect grabArea = new Rect(0, 0, width, height);

            RenderTexture.active = renderTexture;
            camera.targetTexture = renderTexture;
            camera.clearFlags = CameraClearFlags.SolidColor;

            camera.backgroundColor = Color.black;
            camera.Render();
            shotWidthBg.ReadPixels(grabArea, 0, 0);
            shotWidthBg.Apply();

            // Revert properties back
            camera.clearFlags = cameraClearFlags;
            camera.targetTexture = cameraTargetTexture;
            RenderTexture.active = renderTextureActive;
            RenderTexture.ReleaseTemporary(renderTexture);

            return shotWidthBg;
        }
    }
}
