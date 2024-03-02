using System.Reflection;
using BoneLib;
using LabFusion.Data;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities;

public static class Resources
{
    public static Assembly Fusion5vs5Assembly { get; }

    private const string BundlePath = "Fusion5vs5Gamemode.Resources.fusion5vs5custom.bundle";
    public static AssetBundle? Bundle { get; }

    public const string Mp5BodyPath = "WPN_MP5";
    public static Mesh? Mp5Body { get; private set; }
    
    public const string PicatinnyRailMp5Path = "Picatinny Rail MP5";
    public static GameObject? PicatinnyRailMp5 { get; private set; }

    static Resources()
    {
        Fusion5vs5Assembly = Assembly.GetExecutingAssembly();
        Bundle = EmbeddedAssetBundle.LoadFromAssembly(Fusion5vs5Assembly, BundlePath);
        Mp5Body = Bundle.LoadPersistentAsset<Mesh>(Mp5BodyPath);
        PicatinnyRailMp5 = Bundle.LoadPersistentAsset<GameObject>(PicatinnyRailMp5Path);
    }
}
