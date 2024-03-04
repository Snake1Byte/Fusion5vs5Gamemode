using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BoneLib;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Data;
using LabFusion.Extensions;
using MelonLoader;
using SLZ.Interaction;
using SLZ.Marrow.Pool;
using UnityEngine;
using Object = UnityEngine.Object;
using static Fusion5vs5Gamemode.Shared.Commons;
using SafeActions = BoneLib.SafeActions;

namespace Fusion5vs5Gamemode.Client.Combat;

public static class WeaponModification
{
    public const string ROOT_NAME = "Fusion 5vs5 Custom Objects";
    private static GameObject? _PicatinnySlot;
    private static GameObject? _MuzzleSlot;
    private static GameObject? _Dovetail;

    private static Dictionary<GameObject, List<GameObject>> _ModifiedWeapons = new(new GoComparer());

    private static void EmptyCollections(LevelInfo obj)
    {
        _ModifiedWeapons.Clear();
    }

    public static void ModifyWeapon(GameObject spawnedGo, byte owner)
    {
        Log(spawnedGo, owner);
        string? barcode = spawnedGo.GetComponent<AssetPoolee>()?.spawnableCrate._barcode;
        if (barcode == null) return;
        if (!AssetDatabase.AttachmentDatabase.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find(ROOT_NAME);
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
            ResetAttachmentSlots(spawnedGo, owner);
            return;
        }

        GameObject root = new GameObject(ROOT_NAME);
        root.transform.SetParent(spawnedGo.transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        foreach (string path in attachments.GameObjectsToRemove)
        {
            Transform transform = spawnedGo.transform.Find(path);
            if (transform == null) continue;
            GameObject go = transform.gameObject;
            go.SetActive(false);
        }

        InitializePicatinnySlotAsync(owner, null);

        List<GameObject> addedSlots = new();
        MelonCoroutines.Start(CoWaitAndAddPicatinnySlots(root, attachments.PicatinnySlotsToAdd, () => _PicatinnySlot != null, list =>
        {
            if (_ModifiedWeapons.Count == 0)
            {
                // TODO unsubscribe again after removing items from list
                Hooking.OnLevelInitialized += EmptyCollections;
            }

            addedSlots.AddRange(list);
            _ModifiedWeapons.Add(spawnedGo, addedSlots);
        }));

        MelonCoroutines.Start(CoWaitAndAddAttachmentsToPicatinnySlots(addedSlots, attachments.PicatinnyAttachmentsToAdd, true, owner, () => addedSlots.Count != 0));

        InitializeMuzzleSlotAsync(owner, () =>
        {
            foreach (SerializedTransform slot in attachments.MuzzleSlotsToAdd)
            {
                AddMuzzleSlot(root, slot);
            }
        });

        SafeActions.InvokeActionSafe(attachments.CustomActionUponSpawn, spawnedGo); // TODO add this in a coroutine too
    }

    private static void InitializePicatinnySlotAsync(byte owner, Action? continueWith)
    {
        Log(owner, continueWith!);
        if (_PicatinnySlot == null)
        {
            FusionSpawning.RequestSpawn("Rexmeck.GunAttachments.Spawnable.45CantedRail", new SerializedTransform(Vector3.one, Quaternion.identity), owner, (b, s, syncId, source) =>
            {
                Transform tr = source.transform.Find("Sockets/Attachment_Rail_v2");
                if (tr == null) return;
                _PicatinnySlot = Object.Instantiate(tr.gameObject);
                _PicatinnySlot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                _PicatinnySlot.gameObject.name = "PicatinnySlot";
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

        if (_PicatinnySlot == null) return null;
        GameObject? obj = AddGameObject(_PicatinnySlot, "", root);
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
                MelonCoroutines.Start(CoWaitAndAddAttachmentToPicatinnySlot(slot, attachmentBarcode, false, owner, () => GetAttachment(slot) == null));
            }
        }
        else
        {
            FusionSpawning.RequestSpawn(attachmentBarcode, new SerializedTransform(slot.transform.position, slot.transform.rotation), owner, (spawnedAttachmentOwner, spawnedAttachmentBarcode, syncId, spawnedAttachmentGo) =>
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
    }

    private static IEnumerator CoWaitAndAddAttachmentToPicatinnySlot(GameObject slot, string attachmentBarcode, bool overwriteExisting, byte owner, Func<bool> startUponCondition)
    {
        while (!startUponCondition.Invoke())
            yield return null;
        AddAttachmentToPicatinnySlot(slot, attachmentBarcode, overwriteExisting, owner);
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
        List<GameObject> slots = _ModifiedWeapons[weapon];
        foreach (GameObject slot in slots)
        {
            if (SlotNeedsReset(weapon, slot)) RemoveAttachmentFromPicatinnySlot(slot, true);
        }

        AssetPoolee assetPoolee = weapon.GetComponent<AssetPoolee>();
        if (!AssetDatabase.AttachmentDatabase.TryGetValue(assetPoolee.spawnableCrate._barcode, out Attachments attachments))
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
        if (!AssetDatabase.AttachmentDatabase.TryGetValue(assetPoolee.spawnableCrate._barcode, out Attachments attachments))
            return false;
        foreach (KeyValuePair<string, string> pair in attachments.PicatinnyAttachmentsToAdd)
        {
            string attachmentBarcode = pair.Key;
            string slotName = pair.Value;

            if (slotName.Equals(slot.name) && attachment.GetComponent<AssetPoolee>().spawnableCrate._barcode.Equals(attachmentBarcode))
            {
                return false;
            }

            return true;
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
