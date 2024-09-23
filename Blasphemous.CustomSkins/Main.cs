using BepInEx;

namespace Blasphemous.CustomSkins;

[BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
[BepInDependency("Blasphemous.ModdingAPI", "2.4.1")]
internal class Main : BaseUnityPlugin
{
    public static CustomSkins CustomSkins { get; private set; }

    private void Start()
    {
        CustomSkins = new CustomSkins();
    }
}
