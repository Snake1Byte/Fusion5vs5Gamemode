using System.Collections.Generic;
using LabFusion.Data;
using UnityEngine;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Fusion5vs5Gamemode.Client.Combat;

public class AttachmentDatabase
{
    public static readonly Dictionary<string, Attachments> Database = new();
    private static GameObject? PicatinnySlot;
    private static GameObject? MuzzleSlot;
    private static GameObject? Dovetail;

    static AttachmentDatabase()
    {
        InitializeDatabase();
    }

    private static void InitializeDatabase()
    {
        string barcode = BoneLib.CommonBarcodes.Guns.MK18HoloForegrip;
        Attachments attachments = new Attachments();
        attachments.GameObjectsToAdd.Add(BoneLib.CommonBarcodes.Guns.MK18Naked, "BarrelGrip");
        attachments.GameObjectsToRemove.Add("attachment_ForwardGrip");
        attachments.GameObjectsToRemove.Add("redDotSight");
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.652618f, -32.73531f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.652618f, -32.81801f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.652618f, -32.65231f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.652618f, -32.56961f),
            new Quaternion(0f, 0f, 3.576278E-07f, 1f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.595418f, -32.65611f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.595418f, -32.57311f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2f, 2.595418f, -32.49041f),
            new Quaternion(0f, 0f, 1f, -3.576278E-07f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(1.9752f, 2.623618f, -32.48203f),
            new Quaternion(0f, 0f, 0.7071068f, 0.7071068f)));
        attachments.PicatinnySlotsToAdd.Add(new SerializedTransform(new Vector3(2.02475f, 2.623948f, -32.48203f),
            new Quaternion(0f, 0f, -0.7071068f, 0.7071068f)));
        Database.Add(barcode, attachments);
    }

    public static void AddAttachmentSlots(string barcode, GameObject spawnedGo)
    {
        if (!Database.TryGetValue(barcode, out Attachments attachments)) return;
        Transform customTransform = spawnedGo.transform.Find("Fusion5vs5 Custom Objects");
        if (customTransform != null)
        {
            customTransform.gameObject.SetActive(true);
            return;
        }

        GameObject root = new GameObject("Fusion5vs5 Custom Objects");
        root.transform.SetParent(spawnedGo.transform);
        foreach (string path in attachments.GameObjectsToRemove)
        {
            Transform transform = spawnedGo.transform.Find(path);
            if (transform == null) continue;
            GameObject go = transform.gameObject;
            go.SetActive(false);
        }
    }

    public static void RemoveAttachmentSlots(string barcode, GameObject spawnedGo)
    {
        if (!Database.ContainsKey(barcode)) return;
        Transform customTransform = spawnedGo.transform.Find("Fusion5vs5 Custom Objects");
        if (customTransform == null) return;
        customTransform.gameObject.SetActive(false);
    }
}