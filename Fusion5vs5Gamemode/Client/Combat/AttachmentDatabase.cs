using System;
using System.Collections.Generic;
using LabFusion.Data;
using UnityEngine;
using System.Numerics;
using Fusion5vs5Gamemode.Utilities;
using LabFusion.Extensions;
using LabFusion.Utilities;
using SLZ.Marrow.Pool;
using TriangleNet;
using Object = UnityEngine.Object;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using static Fusion5vs5Gamemode.Shared.Commons;
using Log = TriangleNet.Log;

namespace Fusion5vs5Gamemode.Client.Combat;

public class AttachmentDatabase
{
    public static readonly Dictionary<string, Attachments> Database = new();
    private static GameObject? PicatinnySlot;
    private static GameObject? MuzzleSlot;
    private static GameObject? Dovetail;

    static AttachmentDatabase()
    {
        Log();
        InitializeDatabase();
    }

    private static void InitializeDatabase()
    {
        Log();
        string barcode = BoneLib.CommonBarcodes.Guns.MK18HoloForegrip;
        Attachments attachments = new Attachments();
        attachments.GameObjectsToAdd.Add(BoneLib.CommonBarcodes.Guns.MK18Naked, "BarrelGrip");
        attachments.GameObjectsToRemove.Add("attachment_ForwardGrip");
        attachments.GameObjectsToRemove.Add("redDotSight");
        attachments.GameObjectsToRemove.Add("BarrelGrip");
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.0611f, 0.05459976f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.0611f, -0.0281f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.0611f, 0.137599f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.0611f, 0.2203f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.003900051f, 0.1338f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2168007f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0f, 0.003900051f, 0.2994995f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(-0.0248f, 0.0321f, 0.30788f),
            new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(0.02475f, 0.03243f, 0.30788f),
            new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        Database.Add(barcode, attachments);
    }

    public static void AddAttachmentSlots(string barcode, GameObject spawnedGo, byte owner)
    {
        Log(barcode, spawnedGo, owner);
        if (!Database.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find("Fusion5vs5 Custom Objects");
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
            return;
        }

        GameObject root = new GameObject("Fusion5vs5 Custom Objects");
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

        foreach (var pair in attachments.GameObjectsToAdd)
        {
            string barcodeToSpawn = pair.Key;
            string goToCopy = pair.Value;
            FusionSpawning.RequestSpawn(barcodeToSpawn, new SerializedTransform(Vector3.One, Quaternion.Identity),
                owner, (b, s, source) =>
                {
                    AddGameObject(source, goToCopy, root);
                    AssetPoolee poolee;
                    if ((poolee = source.GetComponentInChildren<AssetPoolee>()) != null)
                    {
                        AssetSpawner.Despawn(poolee);
                    }
                });
        }

        foreach (SerializedTransform slot in attachments.PicatinnySlotsToAdd)
        {
            if (PicatinnySlot == null)
            {
                InitializePicatinnySlot(owner, () =>
                {
                    if (PicatinnySlot != null)
                    {
                        AddPicatinnySlot(root, slot);
                    }
                });
            }
            else
            {
                AddPicatinnySlot(root, slot);
            }
        }

        foreach (SerializedTransform slot in attachments.MuzzleSlotsToAdd)
        {
            if (MuzzleSlot == null)
            {
                InitializeMuzzleSlot(owner, () =>
                {
                    if (MuzzleSlot != null) AddGameObject(MuzzleSlot, "", root);
                });
            }
            else
            {
                AddGameObject(MuzzleSlot, "", root);
            }
        }
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

    private static void InitializePicatinnySlot(byte owner, Action continueWith)
    {
        Log(owner, continueWith);
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
                AssetPoolee poolee;
                if ((poolee = source.GetComponentInChildren<AssetPoolee>()) != null)
                {
                    AssetSpawner.Despawn(poolee);
                }

                continueWith();
            });
    }

    private static void AddPicatinnySlot(GameObject root, SerializedTransform transform)
    {
        Log(root, transform);
        
        if (PicatinnySlot == null) return;
        GameObject? obj = AddGameObject(PicatinnySlot, "", root);
        if (obj == null) return;
        obj.transform.localPosition = transform.position.ToUnityVector3();
        obj.transform.localRotation = transform.rotation.ToUnityQuaternion();
    }

    private static void InitializeMuzzleSlot(byte owner, Action continueWith)
    {
        // TODO
    }

    public static void RemoveAttachmentSlots(string barcode, GameObject toEdit)
    {
        Log(barcode, toEdit);
        if (!Database.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = toEdit.transform.Find("Fusion5vs5 Custom Objects");
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
}