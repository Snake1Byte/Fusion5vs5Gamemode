#if !MELONLOADER
using Fusion5vs5Gamemode.SDK.Internal;
using LabFusion.MarrowIntegration;
using UnityEditor;

// This is copy pasted from FusionMarrowBehaviourEditor. The reason we need to create our own abstract MonoBehaviour parent class is that the Fusion5vs5Gamemode MonoBehaviours need values injected via FieldInjector.dll and FusionMarrowBehaviour is already injected into IL2CPP which leads to an exception. 
namespace Fusion5vs5Gamemode.SDK.Editor {
    [CustomEditor(typeof(Fusion5vs5GamemodeBehaviour), editorForChildClasses: true)]
    public class Fusion5vs5GamemodeBehaviourEditor : UnityEditor.Editor {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5GamemodeBehaviour;
            if (behaviour.Comment != null) {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }
}
#endif