using System.Collections.Generic;
using LabFusion.Data;
using UnityEngine;

namespace Fusion5vs5Gamemode.Client.Combat;

public class Attachments
{
    public readonly List<SerializedTransform> PicatinnySlotsToAdd = new();
    public readonly List<SerializedTransform> MuzzleSlotsToAdd = new();
    public SerializedTransform? Dovetail;

    // string #1: Barcode to spawn. string #2: path of the GameObject inside of the spawnable denounced by the barcode.
    // Will get added to a custom GameObject inside of the GameObject associated with this Fusion5vs5Gamemode.Client.Combat.Attachments instance
    public readonly Dictionary<string, string> GameObjectsToAdd = new();
    public readonly List<string> GameObjectsToRemove = new();
    
    public bool HasDovetail => Dovetail != null;
}