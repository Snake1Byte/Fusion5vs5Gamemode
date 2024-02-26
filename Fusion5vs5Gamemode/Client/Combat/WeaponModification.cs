using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoneLib;
using Fusion5vs5Gamemode.Utilities;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.Data;
using LabFusion.Extensions;
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
        attachments.PicatinnySlotsToAdd.Add("Top 1",
            new SerializedTransform(new Vector3(0f, 0.0611f, -0.0281f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 2",
            new SerializedTransform(new Vector3(0f, 0.0611f, 0.05459976f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 3",
            new SerializedTransform(new Vector3(0f, 0.0611f, 0.137599f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 4",
            new SerializedTransform(new Vector3(0f, 0.0611f, 0.2203f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Top 5",
            new SerializedTransform(new Vector3(0f, 0.0611f, 0.3031f), new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add("Left 1",
            new SerializedTransform(new Vector3(-0.0248f, 0.0321f, 0.30788f),
                new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Right 1",
            new SerializedTransform(new Vector3(0.02475f, 0.03243f, 0.30788f),
                new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 1",
            new SerializedTransform(new Vector3(0f, 0.003900051f, 0.1338f),
                new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 2",
            new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2168007f),
                new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add("Bottom 3",
            new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2994995f),
                new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightRear", "Top 1"); // TODO find a crate that lives inside of attachment pack, not weapon pack
        attachments.PicatinnyAttachmentsToAdd.Add("Rexmeck.WeaponPack.Spawnable.DDIronSightFront", "Top 5");


        AttachmentDatabase.Add(barcode, attachments);
    }

    public static void AddAttachmentSlots(string barcode, GameObject spawnedGo, byte owner)
    {
        Log(barcode, spawnedGo, owner);
        if (!AttachmentDatabase.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find("Fusion 5vs5 Custom Objects");
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
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


        List<GameObject> addedSlots = new();
        InitializePicatinnySlotAsync(owner, () =>
        {
            foreach (KeyValuePair<string, SerializedTransform> slot in attachments.PicatinnySlotsToAdd)
            {
                GameObject? addedSlot = AddPicatinnySlot(root, slot.Key, slot.Value);
                if (addedSlot != null) addedSlots.Add(addedSlot);
            }

            foreach (var pair in attachments.PicatinnyAttachmentsToAdd)
            {
                string barcodeToSpawn = pair.Key;
                string slotToAddTo = pair.Value;
                FusionSpawning.RequestSpawn(barcodeToSpawn, new SerializedTransform(Vector3.One, Quaternion.Identity),
                    owner, (ownerAttachment, spawnedAttachmentBarcode, spawnedAttachment) =>
                    {
                        try
                        {
                            GameObject slot = addedSlots.First(go => go.name.Equals(slotToAddTo));
                            spawnedAttachment.transform.SetPositionAndRotation(slot.transform.position,
                                slot.transform.rotation);
                            KeyReciever receiver = slot.GetComponent<KeyReciever>();
                            InteractableHost host = spawnedAttachment.GetComponent<InteractableHost>();
                            if (receiver == null || host == null) return;
                            receiver.OnInteractableHostEnter(host);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    });
            }
        });

        InitializeMuzzleSlotAsync(owner, () =>
        {
            foreach (SerializedTransform slot in attachments.MuzzleSlotsToAdd)
            {
                AddMuzzleSlot(root, slot);
            }
        });

        if (ModifiedWeapons.Count == 0)
        {
            Hooking.OnLevelInitialized += EmptyCollections;
            AssetPooleePatches.OnAssetDespawn += ResetAttachmentSlotsBeforeDespawn;
        }

        ModifiedWeapons.Add(spawnedGo, addedSlots);
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

    private static void InitializePicatinnySlotAsync(byte owner, Action? continueWith)
    {
        Log(owner, continueWith!);
        if (PicatinnySlot == null)
        {
            FusionSpawning.RequestSpawn("Rexmeck.GunAttachments.Spawnable.45CantedRail",
                new SerializedTransform(Vector3.One, Quaternion.Identity), owner,
                (b, s, source) =>
                {
                    Transform tr = source.transform.Find("Sockets/Attachment_Rail_v2");
                    if (tr == null) return;
                    PicatinnySlot = Object.Instantiate(tr.gameObject);
                    PicatinnySlot.transform.SetPositionAndRotation(UnityEngine.Vector3.zero,
                        UnityEngine.Quaternion.identity);
                    PicatinnySlot.gameObject.name = "PicatinnySlot";
                    AssetPoolee
                        poolee; // TODO destroy instead of despawning this, the attachment has references to the canted rail now
                    if ((poolee = source.GetComponentInChildren<AssetPoolee>()) != null)
                    {
                        AssetSpawner.Despawn(poolee);
                    }

                    SafeActions.InvokeActionSafe(continueWith);
                });
        }
        else
        {
            SafeActions.InvokeActionSafe(continueWith);
        }
    }

    private static GameObject? AddPicatinnySlot(GameObject root, string name, SerializedTransform transform)
    {
        Log(root, name, transform);

        if (PicatinnySlot == null) return null;
        GameObject? obj = AddGameObject(PicatinnySlot, "", root);
        if (obj == null) return null;
        obj.transform.localPosition = transform.position.ToUnityVector3();
        obj.transform.localRotation = transform.rotation.ToUnityQuaternion();
        obj.name = name;
        return obj;
    }

    private static void InitializeMuzzleSlotAsync(byte owner, Action? continueWith)
    {
        // TODO
    }

    private static void AddMuzzleSlot(GameObject root, SerializedTransform transform)
    {
        // TODO
    }

    public static void ResetAttachmentSlotsBeforeDespawn(AssetPoolee go)
    {
        Log(go);
        if (!ModifiedWeapons.ContainsKey(go.gameObject)) return;
        MelonLogger.Msg(
            $"ResetAttachmentSlotsBeforeDespawn called on {go.gameObject.name} and id {go.gameObject.GetInstanceID()}");
        GameObject weapon = go.gameObject;
        List<GameObject> slots = ModifiedWeapons[weapon];
        foreach (GameObject slot in slots)
        {
            GameObject? attachment = slot.transform.Find("ATTACHMENTV2")?.gameObject;
            if (attachment == null) continue;
            MelonLogger.Msg(
                $"ResetAttachmentSlotsBeforeDespawn: attachment {attachment.name} found on slot {slot.name}");
            if (!AttachmentDatabase.TryGetValue(go.spawnableCrate._barcode, out Attachments attachments)) return;
            bool getRid = true;
            MelonLogger.Msg($"ResetAttachmentSlotsBeforeDespawn: weapon found inside attachment database");
            foreach (KeyValuePair<string, string> pair in attachments.PicatinnyAttachmentsToAdd)
            {
                string attachmentBarcode = pair.Key;
                string slotName = pair.Value;
                if (slotName.Equals(slot.name) && attachment.GetComponent<AssetPoolee>().spawnableCrate._barcode
                        .Equals(attachmentBarcode))
                {
                    MelonLogger.Msg($"ResetAttachmentSlotsBeforeDespawn: attachment {attachment.name} found inside {slot.name}. this is correct.");
                    getRid = false;
                    break;
                }
            }

            if (!getRid) continue;
            MelonLogger.Msg($"ResetAttachmentSlotsBeforeDespawn: attachment {attachment.name} does not belong in {slot.name}. getting rid of it.");
            GameObject? detachGo = attachment.transform.FindChild("Detach")?.gameObject;
            if (detachGo == null) continue;
            MelonLogger.Msg($"ResetAttachmentSlotsBeforeDespawn: detachGo found. activating it.");
            detachGo.SetActive(true);
        }
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