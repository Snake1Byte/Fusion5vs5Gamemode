using System;
using System.Collections.Generic;
using System.Linq;
using BoneLib;
using Fusion5vs5Gamemode.Utilities.HarmonyPatches;
using LabFusion.NativeStructs;
using MelonLoader;
using SLZ.AI;
using SLZ.Combat;
using SLZ.Marrow.Data;
using SLZ.Props.Weapons;
using UnityEngine;

namespace Fusion5vs5Gamemode.Utilities
{
    public static class ProjectileTrace
    {
        public static Action<Projectile, TriggerRefProxy, Gun, ImpactProperties, Attack_> OnProjectileImpactedSurface;
        public static Action<Projectile, TriggerRefProxy, ImpactProperties, Attack_> OnProjectileDamagedPlayer;

        private static Dictionary<int, Gun> FirePointOrigin = new Dictionary<int, Gun>();
        private static Dictionary<Projectile, Gun> ProjectileOrigin = new Dictionary<Projectile, Gun>();

        private static Dictionary<int, TriggerRefProxy> TriggerRefProxys =
            new Dictionary<int, TriggerRefProxy>();

        private static object dictionariesLock = new object();

        static ProjectileTrace()
        {
            Hooking.OnLevelLoading += EmptyDictionaries;
            GunPatches.OnGunFired += GunFired;
            ProjectilePatches.OnSetBulletObject += ProjectileDispatched;
            ImpactPropertiesPatches.OnAttackReceived += ProjectileImpactedSurface;
        }

        private static void EmptyDictionaries(LevelInfo obj)
        {
            EmptyDictionaries();
        }

        private static void EmptyDictionaries()
        {
            FirePointOrigin.Clear();
            ProjectileOrigin.Clear();
            TriggerRefProxys.Clear();
        }

        private static void GunFired(Gun gun)
        {
            lock (dictionariesLock)
            {
                FirePointOrigin[gun.firePointTransform.GetInstanceID()] = gun;
            }
        }

        private static void ProjectileDispatched(Projectile projectile, ProjectileData data, Transform startTransform,
            TriggerRefProxy proxy)
        {
            lock (dictionariesLock)
            {
                if (FirePointOrigin.TryGetValue(startTransform.GetInstanceID(), out Gun gun))
                {
                    ProjectileOrigin[projectile] = gun;
                    TriggerRefProxys[projectile.GetInstanceID()] = proxy;
                }
            }
        }

        private static void ProjectileDamagedPlayer(Attack_ attack)
        {
        }

        private static void ProjectileImpactedSurface(ImpactProperties receiver, Attack_ attack)
        {
            Projectile impactOrigin;
            Gun projectileOrigin;
            TriggerRefProxy proxy;
            lock (dictionariesLock)
            {
                try
                {
                    // Remove current null keys
                    foreach (var key in ProjectileOrigin.Keys)
                    {
                        if (key == null)
                        {
                            ProjectileOrigin.Remove(key);
                        }
                    }

                    impactOrigin = ProjectileOrigin.Keys.First(e => e._direction == attack.direction);
                    if (ProjectileOrigin.TryGetValue(impactOrigin, out Gun gun))
                    {
                        projectileOrigin = gun;
                    }
                    else
                    {
                        return;
                    }

                    if (TriggerRefProxys.TryGetValue(impactOrigin.GetInstanceID(), out TriggerRefProxy _proxy))
                    {
                        if (_proxy == null)
                        {
#if DEBUG
                            MelonLogger.Msg(
                                $"TriggerRefProxy component for Prejectile with GO name \"{impactOrigin.gameObject.name}\" was found to be null. Aborting.");
#endif
                            TriggerRefProxys.Remove(impactOrigin.GetInstanceID());
                            return;
                        }

                        proxy = _proxy;
                    }
                    else
                    {
#if DEBUG
                        MelonLogger.Msg(
                            $"No TriggerRefProxy component found for Prejectile with GO name \"{impactOrigin.gameObject.name}\".");
#endif
                        return;
                    }
                }
                catch (InvalidOperationException)
                {
#if DEBUG
                    MelonLogger.Msg(
                        $"No Projectile component found that impacted on a surface with direction {attack.direction}.");
#endif
                    return;
                }
            }

            if (OnProjectileImpactedSurface != null)
            {
                OnProjectileImpactedSurface.Invoke(impactOrigin, proxy, projectileOrigin, receiver, attack);
            }
        }
    }
}