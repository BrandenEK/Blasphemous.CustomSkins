﻿using Blasphemous.ModdingAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Blasphemous.CustomSkins;

/// <summary>
/// Handles loading skins and adding them to the UI selection
/// </summary>
public class CustomSkins : BlasMod
{
    internal CustomSkins() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }

    /// <summary>
    /// Registers handlers
    /// </summary>
    protected override void OnInitialize()
    {
        LocalizationHandler.RegisterDefaultLanguage("en");
    }

    internal bool AllowedSkinButtons { get; private set; }

    private readonly Dictionary<string, SkinInfo> _customSkins = new();
    private readonly List<GameObject> _skinButtons = new();

    internal void AddSkinButton(GameObject obj)
    {
        _skinButtons.Add(obj);
    }

    internal List<GameObject> AllowSkinButtons()
    {
        AllowedSkinButtons = true;
        return _skinButtons;
    }

    internal SkinInfo GetSkinInfo(string id)
    {
        return _customSkins.TryGetValue(id, out var info) ? info : null;
    }

    internal IEnumerable<SkinInfo> GetAllSkinInfos()
    {
        return _customSkins.Values;
    }

    internal void LoadCustomSkins()
    {
        foreach (SkinInfo info in LoadAllSkins())
        {
            if (_customSkins.ContainsKey(info.id))
            {
                ModLog.Warn($"Rejecting duplicate skin: {info.id}");
                continue;
            }

            _customSkins.Add(info.id, info);
            ModLog.Info($"Loading custom skin: {info.id} by {info.author}");
        }
    }

    private IEnumerable<SkinInfo> LoadAllSkins()
    {
        string skinsDir = Path.Combine(FileHandler.ModdingFolder, "skins");
        Directory.CreateDirectory(skinsDir);

        return Directory.GetDirectories(skinsDir).Select(LoadSkin).Where(x => x != null);
    }

    private SkinInfo LoadSkin(string path)
    {
        string infoPath = Path.Combine(path, "info.txt");
        string texturePath = Path.Combine(path, "texture.png");

        if (!File.Exists(infoPath) || !File.Exists(texturePath))
            return null;

        string text = File.ReadAllText(infoPath);
        byte[] bytes = File.ReadAllBytes(texturePath);

        var tex = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(bytes);
        tex.filterMode = FilterMode.Point;

        SkinInfo info = JsonConvert.DeserializeObject<SkinInfo>(text);
        info.texture = Sprite.Create(tex, new Rect(0, 0, 256, 1), new Vector2(0.5f, 0.5f));

        return info;
    }
}
