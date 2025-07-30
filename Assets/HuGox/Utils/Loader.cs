using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HuGox.Utils
{
    public interface IImageLoader
    {
        Sprite LoadImage(string path);
    }

    public interface IAudioLoader
    {
        AudioClip LoadAudio(string path);
    }

    public class ImageLoader : IImageLoader
    {
        public Sprite LoadImage(string path)
        {
            if (File.Exists(path))
            {
                return LoadPNGFromFolder(path);
            }

            return LoadPNGFromResources(path);
        }

        private Sprite LoadPNGFromFolder(string filePath)
        {
            Debug.Log("LoadPNGFromFolder: " + filePath);
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("LoadPNGFromFolder: filePath is null or empty.");
                return Resources.Load<Sprite>("image/NoImage");
            }

            filePath = filePath.Replace("/", "\\");
            Sprite sprite = null;

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }

            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("image/NoImage");
            }

            return sprite;
        }

        private Sprite LoadPNGFromResources(string resourcePath)
        {
            Debug.Log("LoadPNGFromResources: " + resourcePath);

            if (string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogWarning("LoadPNGFromResources: resourcePath is null or empty.");
                return Resources.Load<Sprite>("image/NoImage");
            }

            resourcePath = resourcePath.Replace("/", "\\");
            resourcePath = resourcePath.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");

            Sprite sprite = Resources.Load<Sprite>(resourcePath);

            if (sprite == null)
            {
                Debug.LogWarning($"Sprite not found at Resources/{resourcePath}, loading default placeholder image.");
                sprite = Resources.Load<Sprite>("image/NoImage");
            }

            return sprite;
        }
    }

    public class AudioLoader : IAudioLoader
    {
        public AudioClip LoadAudio(string path)
        {
            if (File.Exists(path))
            {
                return LoadAudioFromFolder(path);
            }

            return LoadAudioFromResources(path);
        }

        private AudioClip LoadAudioFromFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("LoadAudioFromFolder: filePath is null or empty.");
                return null;
            }

            filePath = filePath.Replace("/", "\\");
            AudioClip audioClip = null;

            if (File.Exists(filePath))
            {
                var pathArray = filePath.Split("\\");
                audioClip = LoadAudioClip("audio" + pathArray[^1]);
            }

            return audioClip;


            AudioClip LoadAudioClip(string name)
            {
                AudioClip clip = GetAudioFromFile(filePath);
                clip.name = name;
                return clip;
            }

            AudioClip GetAudioFromFile(string path)
            {
                UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
                var song = DownloadHandlerAudioClip.GetContent(req);
                return song;
            }
        }

        private AudioClip LoadAudioFromResources(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogWarning("LoadAudioFromResources: resourcePath is null or empty.");
                return null;
            }

            resourcePath = resourcePath.Replace("/", "\\");
            resourcePath = resourcePath.Replace(".ogg", "").Replace(".mp3", "").Replace(".wav", "");

            AudioClip audioClip = Resources.Load(resourcePath) as AudioClip;
            if (audioClip == null)
            {
                Debug.LogWarning($"Audio not found at Resources/{resourcePath}.");
            }

            return audioClip;
        }
    }
}