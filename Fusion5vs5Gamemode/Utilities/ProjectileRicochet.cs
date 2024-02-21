using System;
using BoneLib.Nullables;
using LabFusion.NativeStructs;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities;

public static class ProjectileRicochet
{
    private static bool _Enabled;
    private static readonly object EnabledLock = new();

    public static void Enable()
    {
        lock (EnabledLock)
        {
            if (!_Enabled)
            {
                ProjectileTrace.OnProjectileImpactedSurface += OnProjectileImpactedSurface;
                _Enabled = true;
            }
        }
    }

    public static void Disable()
    {
        lock (EnabledLock)
        {
            if (_Enabled)
            {
                ProjectileTrace.OnProjectileImpactedSurface -= OnProjectileImpactedSurface;
                _Enabled = false;

            }
        }
    }

    private static void OnProjectileImpactedSurface(Projectile projectile, TriggerRefProxy proxy,
        Gun projectileOrigin, ImpactProperties receiver, Attack_ attack)
    {
        try
        {
            /*string goName = projectile.gameObject.name + " Ricochet Transform";
            if (!RicochetGOCache.TryGetValue(goName, out GameObject ricochetGO))
            {
                ricochetGO = new GameObject(goName);
                RicochetGOCache[goName] = ricochetGO;
            }

            Vector3 reflectDirection = Vector3.Reflect(attack.direction, attack.normal);
            ricochetGO.transform.SetPositionAndRotation(attack.origin, Quaternion.LookRotation(reflectDirection));
            */
#if DEBUG
            MelonLogger.Msg($"Firing ricochet from Projectile impact of instance {projectile.GetInstanceID()}");
#endif
            Spawnable spawnable = projectileOrigin.defaultCartridge.projectile.spawnable;
            AssetSpawner.Register(spawnable);
            AssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity,
                new BoxedNullable<Vector3>(Vector3.one), false,
                new BoxedNullable<int>(null), (Action<GameObject>)(go =>
                {
                    try
                    {
                        Vector3 reflectDirection = Vector3.Reflect(attack.direction, attack.normal);
                        go.transform.SetPositionAndRotation(attack.origin, Quaternion.LookRotation(reflectDirection));
                            
                        Projectile ricochetProjectile = go.GetComponent<Projectile>();
                        ricochetProjectile.SetBulletObject(projectile._data, go.transform, Vector3.zero,
                            Quaternion.identity,
                            null, proxy);
                    }
                    catch (Exception e)
                    {
#if DEBUG
                        MelonLogger.Msg(
                            $"Exception {e} in ProjectileRicochet.OnProjectileImpactedSurface(...) SpawnCrate() callback. Aborting.");
#endif
                    }
                }));
        }
        catch (Exception e)
        {
#if DEBUG
            MelonLogger.Msg(
                $"Exception {e} in ProjectileRicochet.OnProjectileImpactedSurface(...). Aborting.");
#endif
        }
    }
}