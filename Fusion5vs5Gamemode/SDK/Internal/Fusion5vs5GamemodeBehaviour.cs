using System;
using UnityEngine;

// This is copy pasted from FusionMarrowBehaviour. The reason we need to create our own abstract MonoBehaviour parent class is that the Fusion5vs5Gamemode MonoBehaviours need values injected via FieldInjector.dll and FusionMarrowBehaviour is already injected into IL2CPP which leads to an exception. 

namespace Fusion5vs5Gamemode.SDK.Internal {
#if MELONLOADER
    public class Fusion5vs5GamemodeBehaviour : MonoBehaviour {
#else
    public abstract class Fusion5vs5GamemodeBehaviour : MonoBehaviour {
#endif
#if MELONLOADER
        public Fusion5vs5GamemodeBehaviour(IntPtr intPtr) : base(intPtr) { }

        private Transform _transform;
        private bool _hasTransform;
        public Transform Transform {
            get {
                if (!_hasTransform) {
                    _transform = transform;
                    _hasTransform = true;
                }

                return _transform;
            }
        }
#else
        public virtual string Comment => null;
#endif
    }
}