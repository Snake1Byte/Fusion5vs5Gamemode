using System.Reflection;
using BoneLib;
using BoneLib.Nullables;
using FieldInjector;
using Fusion5vs5Gamemode.SDK;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Marrow.Data;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;
using UnityEngine;
using CommonBarcodes = BoneLib.CommonBarcodes;
using Module = LabFusion.SDK.Modules.Module;

namespace Fusion5vs5Gamemode;

public class Main : MelonMod
{
    public override void OnInitializeMelon()
    {
        SerialisationHandler.Inject<Fusion5vs5GamemodeDescriptor>();
        SerialisationHandler.Inject<Invoke5vs5UltEvent>();
        ModuleHandler.LoadModule(Assembly.GetExecutingAssembly());
        GamemodeRegistration.LoadGamemodes(Assembly.GetExecutingAssembly());
        ImpactPropertiesPatches.Patch();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Spawnable spawnable = new Spawnable
            {
                crateRef = new SpawnableCrateReference(CommonBarcodes.Guns.AKM)
            };
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            AssetSpawner.Register(spawnable);
            AssetSpawner.Spawn(spawnable, position, rotation, new BoxedNullable<Vector3>(null), false, new BoxedNullable<int>(null));
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SpawnableCrateReference crateRef = new SpawnableCrateReference(CommonBarcodes.Guns.AKM);
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            HelperMethods.SpawnCrate(crateRef, position, rotation, Vector3.one, false, null);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Spawnable spawnable = new Spawnable
            {
                crateRef = new SpawnableCrateReference(CommonBarcodes.Guns.AKM)
            };
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            AssetSpawner.Register(spawnable);
            NullableMethodExtensions.PoolManager_Spawn(spawnable, position, rotation, null);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            Vector3 position = RigData.RigReferences.RigManager.physicsRig.m_pelvis.position + RigData.RigReferences.RigManager.physicsRig.m_pelvis.forward;
            Quaternion rotation = RigData.RigReferences.RigManager.physicsRig.m_pelvis.rotation;
            PooleeUtilities.RequestSpawn(CommonBarcodes.Guns.AKM, new SerializedTransform(position, rotation), PlayerIdManager.LocalId.SmallId);
        }
    }

    public const string NAME = "Fusion5vs5Gamemode";
    public const string VERSION = "0.0.1";
    public const string AUTHOR = "Snake1Byte";
}

public class FusionModule : Module
{
    public static FusionModule? Instance { get; private set; }

    public override void OnModuleLoaded()
    {
        Instance = this;
    }
}