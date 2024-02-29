using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoneLib;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Representation;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using UnityEngine;
using Object = UnityEngine.Object;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using static Fusion5vs5Gamemode.Shared.Commons;
using SafeActions = BoneLib.SafeActions;

namespace Fusion5vs5Gamemode.Client.Combat;

public class WeaponModification
{
    public static readonly Dictionary<string, Attachments> AttachmentDatabase = new();
    private static GameObject? PicatinnySlot;
    private static GameObject? MuzzleSlot;
    private static GameObject? Dovetail;

    private static Dictionary<GameObject, List<GameObject>> ModifiedWeapons = new(new GoComparer());

    static WeaponModification()
    {
        Log();
        InitializeDatabase();
    }

    private static void EmptyCollections(LevelInfo obj)
    {
        ModifiedWeapons.Clear();
    }

    private static void InitializeDatabase()
    {
        Log();
        string barcode = CommonBarcodes.Guns.MK18Naked;
        Attachments attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0611f, -0.0281f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0611f, 0.05459976f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0611f, 0.137599f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0611f, 0.2203f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.0611f, 0.3031f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.0248f, 0.0321f, 0.30788f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02475f, 0.03243f, 0.30788f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.1338f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2168007f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2994995f), new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1"); // TODO new dependency :hollow:
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 5");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.PDRC;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("LaserPointer_01");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0494f, 0.0051f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0494f, 0.0668f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0494f, 0.1363f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0494f, 0.2004f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 4");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M16IronSights;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("WPN_M16_Exchangeables_frontsight");
        attachments.GameObjectsToRemove.Add("Attachment_FoldingRearSight (1)");
        attachments.GameObjectsToRemove.Add("WPN_M16_Receiver.001");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0613f, -0.0221f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0613f, 0.0561f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0613f, 0.1465f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4", new SerializedTransform(new Vector3(0f, 0.0613f, 0.235f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5", new SerializedTransform(new Vector3(0f, 0.0613f, 0.3202f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 6", new SerializedTransform(new Vector3(0f, 0.0613f, 0.4059f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.1461996f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 2", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.2144f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 3", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.2835f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 4", new SerializedTransform(new Vector3(-0.0286f, 0.0324f, 0.3492f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.1461996f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 2", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.2144f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 3", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.2835f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 4", new SerializedTransform(new Vector3(0.0284f, 0.0324f, 0.3492f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.1465f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.235f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, 0.004300003f, 0.3202f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 4", new SerializedTransform(new Vector3(0f, 0.0043f, 0.4059f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 6");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.FAB;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_clamp");
        attachments.GameObjectsToRemove.Add("attachment_Lazer");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.07173f, 0.05980012f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.07173f, 0.1212f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.07173f, 0.18597f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02402f, 0.0511f, 0.54196f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.CustomAction += o => o.transform.Find("FDefenceOne(Dmg)")?.gameObject.SetActive(true);
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1");
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 3");
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M4;
        attachments = new Attachments();
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0525f, 0.0397f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0525f, 0.0875f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0525f, 0.1346f), new Quaternion(0f, 0f, 0f, 1f)));
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.M590A1;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_HoloScope");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.06682f, 0.01500008f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.06682f, 0.0552f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.06682f, 0.1002f), new Quaternion(0f, 0f, 0f, 1f)));
        AttachmentDatabase.Add(barcode, attachments);

        barcode = CommonBarcodes.Guns.UMP;
        attachments = new Attachments();
        attachments.GameObjectsToRemove.Add("attachment_ForwardGrip");
        attachments.PicatinnySlotsToAdd.Add("Top 1", new SerializedTransform(new Vector3(0f, 0.0701f, -0.0489f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2", new SerializedTransform(new Vector3(0f, 0.0701f, 0.0001000036f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3", new SerializedTransform(new Vector3(0f, 0.0701f, 0.0499f), new Quaternion(0f, 0f, 0f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.15745f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 2", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.18894f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Left 3", new SerializedTransform(new Vector3(-0.025f, 0.0199f, 0.21869f), new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1", new SerializedTransform(new Vector3(0.02480002f, 0.0199f, 0.1562f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 2", new SerializedTransform(new Vector3(0.02480002f, 0.0199f, 0.18769f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 3", new SerializedTransform(new Vector3(0.0248f, 0.0199f, 0.21744f), new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1", new SerializedTransform(new Vector3(0f, -0.00691f, 0.15796f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2", new SerializedTransform(new Vector3(0f, -0.00691f, 0.18945f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3", new SerializedTransform(new Vector3(0f, -0.00691f, 0.2192f), new Quaternion(0f, 0f, 1f, 0f)));
        attachments.CustomAction += UMPAddBarrel;
        AttachmentDatabase.Add(barcode, attachments);
        
        barcode = CommonBarcodes.Guns.MP5;
        attachments = new Attachments();
        attachments.CustomAction += go =>
        {
            AssetBundle bundle = EmbeddedAssetBundle.LoadFromAssembly(Assembly.GetExecutingAssembly(), "Fusion5vs5Gamemode.Resources.fusion5vs5custom.bundle");
            Mesh mp5Body = bundle.LoadPersistentAsset<Mesh>("WPN_MP5");
            if (mp5Body == null) return;
            MeshFilter? filter = go.transform.Find("offset_MP5/WPN_MP5")?.gameObject.GetComponent<MeshFilter>();
            if (filter == null) return;
            filter.mesh = mp5Body;
        };
        AttachmentDatabase.Add(barcode, attachments);
    }

    private static void UMPAddBarrel(GameObject obj)
    {
        GameObject? root = obj.transform.Find("Fusion 5vs5 Custom Objects")?.gameObject;
        if (root == null) return;
        GameObject gripObj = new GameObject("RailGrip_snek");
        gripObj.transform.SetParent(root.transform);
        gripObj.transform.localPosition = new UnityEngine.Vector3(0, -0.0017f, 0.1882f);
        gripObj.transform.rotation = UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(0, -180, 0));
        GameObject gripPointObj = new GameObject("railgrip_grippoint_snek");
        gripPointObj.transform.SetParent(gripObj.transform);
        gripPointObj.transform.localPosition = new UnityEngine.Vector3(0, 0, 0);
        gripPointObj.transform.rotation = UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(0, 0, 180));
        InteractableIcon interactableIcon = gripObj.AddComponent<InteractableIcon>();
        interactableIcon.targetCenter = gripPointObj.transform;
        interactableIcon.gripIconType = GripIconType.SecondaryGrip;
        interactableIcon.LocalOverride = false;
        interactableIcon.IconSize = 0.025f;
        interactableIcon.AnimationDuration = 0.25f;
        SphereCollider sphereCollider = gripObj.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = 0.01f;
        TargetGrip targetGrip = gripObj.AddComponent<TargetGrip>();
        targetGrip.isThrowable = true;
        targetGrip.ignoreGripTargetOnAttach = false;
        targetGrip.primaryMovementAxis = new UnityEngine.Vector3(0, 0, 1);
        targetGrip.secondaryMovementAxis = new UnityEngine.Vector3(0, 1, 0);
        targetGrip.gripOptions = 0;
        targetGrip.priority = 1.2f;
        targetGrip.minBreakForce = float.PositiveInfinity;
        targetGrip.maxBreakForce = float.PositiveInfinity;
        targetGrip.defaultGripDistance = 0.09f;
        targetGrip.radius = 0.015f;
        targetGrip.targetTransform = gripPointObj.transform;
        targetGrip.rotationLimit = 120;
        targetGrip.rotationPriorityBuffer = 0;
        targetGrip.targetFlipOnPrimaryAxis = true;
        targetGrip.targetFlipOnTertiaryAxis = false;
        FusionSpawning.RequestSpawn(CommonBarcodes.Guns.MK18Naked, new SerializedTransform(Vector3.One, Quaternion.Identity), PlayerIdManager.LocalId.SmallId, (owner, spawnedBarcode, mk18) =>
        {
            CylinderGrip? cylinderGrip = mk18.transform.Find("BarrelGrip")?.gameObject.GetComponent<CylinderGrip>();
            if (cylinderGrip == null) return;
            targetGrip.handPose = Object.Instantiate(cylinderGrip.handPose);
            mk18.GetComponent<AssetPoolee>().Despawn();
        });
    }

    public static void ModifyWeapon(GameObject spawnedGo, byte owner)
    {
        Log(spawnedGo, owner);
        string? barcode = spawnedGo.GetComponent<AssetPoolee>()?.spawnableCrate._barcode;
        if (barcode == null) return;
        if (!AttachmentDatabase.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find("Fusion 5vs5 Custom Objects");
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
            ResetAttachmentSlots(spawnedGo, owner);
            return;
        }

        GameObject root = new GameObject("Fusion 5vs5 Custom Objects");
        root.transform.SetParent(spawnedGo.transform);
        root.transform.localPosition = UnityEngine.Vector3.zero;
        root.transform.localRotation = UnityEngine.Quaternion.identity;
        foreach (string path in attachments.GameObjectsToRemove)
        {
            Transform transform = spawnedGo.transform.Find(path);
            if (transform == null) continue;
            GameObject go = transform.gameObject;
            go.SetActive(false);
        }

        InitializePicatinnySlotAsync(owner, null);

        List<GameObject> addedSlots = new();
        MelonCoroutines.Start(CoWaitAndAddPicatinnySlots(root, attachments.PicatinnySlotsToAdd, () => PicatinnySlot != null, list =>
        {
            if (ModifiedWeapons.Count == 0)
            {
                // TODO unsubscribe again after removing items from list
                Hooking.OnLevelInitialized += EmptyCollections;
            }

            addedSlots.AddRange(list);
            ModifiedWeapons.Add(spawnedGo, addedSlots);
        }));

        MelonCoroutines.Start(CoWaitAndAddAttachmentsToPicatinnySlots(addedSlots, attachments.PicatinnyAttachmentsToAdd, true, owner, () => addedSlots.Count != 0));

        InitializeMuzzleSlotAsync(owner, () =>
        {
            foreach (SerializedTransform slot in attachments.MuzzleSlotsToAdd)
            {
                AddMuzzleSlot(root, slot);
            }
        });
        
        SafeActions.InvokeActionSafe(attachments.CustomAction, spawnedGo);
    }

    private static void InitializePicatinnySlotAsync(byte owner, Action? continueWith)
    {
        Log(owner, continueWith!);
        if (PicatinnySlot == null)
        {
            FusionSpawning.RequestSpawn("Rexmeck.GunAttachments.Spawnable.45CantedRail", new SerializedTransform(Vector3.One, Quaternion.Identity), owner, (b, s, source) =>
            {
                Transform tr = source.transform.Find("Sockets/Attachment_Rail_v2");
                if (tr == null) return;
                PicatinnySlot = Object.Instantiate(tr.gameObject);
                PicatinnySlot.transform.SetPositionAndRotation(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
                PicatinnySlot.gameObject.name = "PicatinnySlot";
                // destroy instead of despawning this, the attachment has references to the canted rail now
                Object.Destroy(source);

                SafeActions.InvokeActionSafe(continueWith);
            });
        }
        else
        {
            SafeActions.InvokeActionSafe(continueWith);
        }
    }

    private static List<GameObject> AddPicatinnySlots(GameObject root, Dictionary<string, SerializedTransform> picatinnySlotsToAdd)
    {
        Log(root, picatinnySlotsToAdd);

        List<GameObject> addedSlots = new();
        foreach (KeyValuePair<string, SerializedTransform> slot in picatinnySlotsToAdd)
        {
            GameObject? addedSlot = AddPicatinnySlot(root, slot.Value);
            if (addedSlot == null) continue;
            addedSlots.Add(addedSlot);
            addedSlot.name = slot.Key;
        }

        return addedSlots;
    }

    private static IEnumerator CoWaitAndAddPicatinnySlots(GameObject root, Dictionary<string, SerializedTransform> picatinnySlotsToAdd, Func<bool> startUponCondition, Action<List<GameObject>> handleReturnValue)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        List<GameObject> returnValue = AddPicatinnySlots(root, picatinnySlotsToAdd);
        SafeActions.InvokeActionSafe(handleReturnValue, returnValue);
    }

    private static GameObject? AddPicatinnySlot(GameObject root, SerializedTransform transform)
    {
        Log(root, transform);

        if (PicatinnySlot == null) return null;
        GameObject? obj = AddGameObject(PicatinnySlot, "", root);
        if (obj == null) return null;
        obj.transform.localPosition = transform.position.ToUnityVector3();
        obj.transform.localRotation = transform.rotation.ToUnityQuaternion();
        return obj;
    }

    private static void AddAttachmentsToPicatinnySlots(List<GameObject> slots, Dictionary<string, string> attachmentsToAdd, bool overwriteExisting, byte owner)
    {
        foreach (var pair in attachmentsToAdd)
        {
            string attachmentBarcodeToSpawn = pair.Key;
            string slotNameToAddTo = pair.Value;
            try
            {
                GameObject slot = slots.First(go => go.name.Equals(slotNameToAddTo));
                AddAttachmentToPicatinnySlot(slot, attachmentBarcodeToSpawn, overwriteExisting, owner);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    private static IEnumerator CoWaitAndAddAttachmentsToPicatinnySlots(List<GameObject> slots, Dictionary<string, string> attachmentsToAdd, bool overwriteExisting, byte owner, Func<bool> startUponCondition)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        AddAttachmentsToPicatinnySlots(slots, attachmentsToAdd, overwriteExisting, owner);
    }

    private static void AddAttachmentToPicatinnySlot(GameObject slot, string attachmentBarcode, bool overwriteExisting, byte owner)
    {
        GameObject? alreadyExisting = GetAttachment(slot);
        if (alreadyExisting != null)
        {
            if (overwriteExisting)
            {
                RemoveAttachmentFromPicatinnySlot(slot, true);
            }
            else
            {
                return;
            }
        }

        FusionSpawning.RequestSpawn(attachmentBarcode, new SerializedTransform(slot.transform.position, slot.transform.rotation), owner, (spawnedAttachmentOwner, spawnedAttachmentBarcode, spawnedAttachmentGo) =>
        {
            try
            {
                KeyReciever receiver = slot.GetComponent<KeyReciever>();
                InteractableHost host = spawnedAttachmentGo.GetComponent<InteractableHost>();
                if (receiver == null || host == null) return;
                receiver.OnInteractableHostEnter(host);
            }
            catch (InvalidOperationException)
            {
            }
        });
    }

    private static void RemoveAttachmentsFromPicatinnySlots(List<GameObject> slots, bool despawn)
    {
        foreach (var slot in slots)
        {
            RemoveAttachmentFromPicatinnySlot(slot, despawn);
        }
    }

    private static void RemoveAttachmentFromPicatinnySlot(GameObject slot, bool despawn)
    {
        GameObject? attachment = GetAttachment(slot);
        if (attachment == null) return;
        SimpleGripEvents? simpleGripEvents = attachment.GetComponentInChildren<SimpleGripEvents>();
        if (simpleGripEvents == null) return;
        simpleGripEvents.OnMenuTapDown?.Invoke();
        if (despawn) attachment.GetComponent<AssetPoolee>()?.Despawn();
    }

    private static void ResetAttachmentSlots(GameObject weapon, byte owner)
    {
        List<GameObject> slots = ModifiedWeapons[weapon];
        foreach (GameObject slot in slots)
        {
            if (SlotNeedsReset(weapon, slot)) RemoveAttachmentFromPicatinnySlot(slot, true);
        }

        AssetPoolee assetPoolee = weapon.GetComponent<AssetPoolee>();
        if (!AttachmentDatabase.TryGetValue(assetPoolee.spawnableCrate._barcode, out Attachments attachments))
            return;
        MelonCoroutines.Start(CoWaitAndAddAttachmentsToPicatinnySlots(slots, attachments.PicatinnyAttachmentsToAdd, false, owner, () =>
        {
            foreach (GameObject slot in slots)
            {
                if (SlotNeedsReset(weapon, slot)) return false;
            }

            return true;
        }));
    }

    private static bool SlotNeedsReset(GameObject weapon, GameObject slot)
    {
        GameObject? attachment = GetAttachment(slot);
        if (attachment == null) return false;
        AssetPoolee assetPoolee = weapon.GetComponent<AssetPoolee>();
        Attachments? attachments;
        if (!AttachmentDatabase.TryGetValue(assetPoolee.spawnableCrate._barcode, out attachments))
            return false;
        foreach (KeyValuePair<string, string> pair in attachments.PicatinnyAttachmentsToAdd)
        {
            string attachmentBarcode = pair.Key;
            string slotName = pair.Value;

            if (slotName.Equals(slot.name) && attachment.GetComponent<AssetPoolee>().spawnableCrate._barcode.Equals(attachmentBarcode))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private static void InitializeMuzzleSlotAsync(byte owner, Action? continueWith)
    {
        // TODO
    }

    private static void AddMuzzleSlot(GameObject root, SerializedTransform transform)
    {
        // TODO
    }

    private static GameObject? AddGameObject(GameObject source, string path, GameObject target)
    {
        Log(source, path, target);
        Transform transform;
        if (string.IsNullOrEmpty(path))
        {
            transform = source.transform;
        }
        else
        {
            transform = source.transform.Find(path);
        }

        if (transform == null) return null;
        return Object.Instantiate(transform.gameObject, target.transform, false);
    }


    private static GameObject? GetAttachment(GameObject slot)
    {
        if (slot == null) return null;
        GameObject? attachment = slot.transform.Find("ATTACHMENTV2")?.gameObject;
        return attachment;
    }

    public static void RemoveAttachmentSlots(string barcode, GameObject toEdit)
    {
        Log(barcode, toEdit);
        if (!AttachmentDatabase.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = toEdit.transform.Find("Fusion 5vs5 Custom Objects");
        if (customTransform == null) return;
        customTransform.gameObject.SetActive(false);

        foreach (string path in attachments.GameObjectsToRemove)
        {
            Transform transform = toEdit.transform.Find(path);
            if (transform == null) continue;
            GameObject go = transform.gameObject;
            go.SetActive(true);
        }
    }

    public class GoComparer : IEqualityComparer<GameObject>
    {
        public bool Equals(GameObject x, GameObject y)
        {
            return x.GetInstanceID() == y.GetInstanceID();
        }

        public int GetHashCode(GameObject obj)
        {
            return obj.GetHashCode();
        }
    }
}
