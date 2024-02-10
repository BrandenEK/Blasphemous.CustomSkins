using Blasphemous.ModdingAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Blasphemous.CustomSkins;

internal class CustomSkins : BlasMod
{
    public CustomSkins() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }

    protected override void OnInitialize()
    {
        LocalizationHandler.RegisterDefaultLanguage("en");
    }

    public bool AllowedSkinButtons { get; private set; }

    private readonly Dictionary<string, SkinInfo> _customSkins = new();
    private readonly List<GameObject> _skinButtons = new();

    public void AddSkinButton(GameObject obj)
    {
        _skinButtons.Add(obj);
    }

    public List<GameObject> AllowSkinButtons()
    {
        AllowedSkinButtons = true;
        return _skinButtons;
    }

    public SkinInfo GetSkinInfo(string id)
    {
        return _customSkins.TryGetValue(id, out var info) ? info : null;
    }

    public IEnumerable<SkinInfo> GetAllSkinInfos()
    {
        return _customSkins.Values;
    }

    public void LoadCustomSkins()
    {
        Dictionary<string, Sprite> skinData = LoadSkins();

        foreach (string skinText in skinData.Keys)
        {
            SkinInfo skinInfo = JsonConvert.DeserializeObject<SkinInfo>(skinText);
            if (_customSkins.ContainsKey(skinInfo.id))
            {
                LogWarning($"Rejecting duplicate skin: {skinInfo.id}");
                continue;
            }

            skinInfo.texture = skinData[skinText];
            _customSkins.Add(skinInfo.id, skinInfo);
            Log($"Loading custom skin: {skinInfo.id} by {skinInfo.author}");
        }
    }

    internal Dictionary<string, Sprite> LoadSkins()
    {
        string skinsPath = Path.GetFullPath("Modding/skins/");
        Dictionary<string, Sprite> customSkins = new Dictionary<string, Sprite>();
        string[] skinFolders = Directory.GetDirectories(skinsPath);

        for (int i = 0; i < skinFolders.Length; i++)
        {
            if (GetSkinFiles(skinFolders[i], out string skinInfo, out Sprite skinTexture))
            {
                if (!customSkins.ContainsKey(skinInfo))
                    customSkins.Add(skinInfo, skinTexture);
            }
        }

        return customSkins;
    }

    private bool GetSkinFiles(string path, out string skinInfo, out Sprite skinTexture)
    {
        skinInfo = null; skinTexture = null;
        if (!File.Exists(path + "/info.txt") || !File.Exists(path + "/texture.png")) return false;

        skinInfo = File.ReadAllText(path + "/info.txt");
        byte[] bytes = File.ReadAllBytes(path + "/texture.png");
        Texture2D tex = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Point;
        skinTexture = Sprite.Create(tex, new Rect(0, 0, 256, 1), new Vector2(0.5f, 0.5f));

        return true;
    }
}
