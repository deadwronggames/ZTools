using System.IO;
using UnityEngine;

namespace DeadWrongGames.ZTools
{
    public class CaptureTextureToPng : MonoBehaviour
    {
        [SerializeField] int _resolutionWidth = 4096;
        [SerializeField] int _resolutionHeight = 4096;
        [SerializeField] string _fileName = "TextureCapture.png";
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Capture();
            }
        }
        
        /// <summary>
        /// HOW TO USE
        /// URP
        /// Use orthogonal camera at Plane with material so that top and bottom just captures it.
        /// Attach this script to Camera.
        /// Play and press 'P'
        /// Can use scene in assets folder.
        /// </summary>
        private void Capture()
        {
            // Setup render texture
            RenderTexture rt = new(_resolutionWidth, _resolutionHeight, 24);
            Camera cam = GetComponent<Camera>();
            cam.targetTexture = rt;
            
            // Render the camera view
            Texture2D screenshot = new(_resolutionWidth, _resolutionHeight, TextureFormat.RGBA32, false);
            cam.Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, _resolutionWidth, _resolutionHeight), 0, 0);
            screenshot.Apply();
            
            // Reset
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            
            // Save PNG
            byte[] bytes = screenshot.EncodeToPNG();
            string savePath = Path.Combine(Application.dataPath, "Output_CaptureTexture");
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            File.WriteAllBytes(Path.Combine(savePath, $"{_fileName}.png"), bytes);
            Debug.Log($"Saved capture to {savePath}");
        }
    }
}