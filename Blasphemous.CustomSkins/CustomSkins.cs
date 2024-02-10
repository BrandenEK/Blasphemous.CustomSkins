using Blasphemous.ModdingAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        foreach (SkinInfo info in LoadAllSkins())
        {
            if (_customSkins.ContainsKey(info.id))
            {
                LogWarning($"Rejecting duplicate skin: {info.id}");
                continue;
            }

            _customSkins.Add(info.id, info);
            Log($"Loading custom skin: {info.id} by {info.author}");
        }
    }

    private IEnumerable<SkinInfo> LoadAllSkins()
    {
        string skinsPath = Path.GetFullPath("Modding/skins/");
        return Directory.GetDirectories(skinsPath).Select(LoadSkin);
    }

    private SkinInfo LoadSkin(string path)
    {
        if (!File.Exists(path + "/info.txt") || !File.Exists(path + "/texture.png"))
            return null;

        string text = File.ReadAllText(path + "/info.txt");
        byte[] bytes = File.ReadAllBytes(path + "/texture.png");

        Texture2D tex = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Point;

        SkinInfo info = JsonConvert.DeserializeObject<SkinInfo>(text);
        info.texture = Sprite.Create(tex, new Rect(0, 0, 256, 1), new Vector2(0.5f, 0.5f));

        return info;
    }
}
