#if !MELONLOADER
using Fusion5vs5Gamemode.SDK.Internal;
using LabFusion.MarrowIntegration;
using UnityEditor;
using UnityEngine;

// This is copy pasted from FusionMarrowBehaviourEditor. The reason we need to create our own abstract MonoBehaviour parent class is that the Fusion5vs5Gamemode MonoBehaviours need values injected via FieldInjector.dll and FusionMarrowBehaviour is already injected into IL2CPP which leads to an exception. 
namespace Fusion5vs5Gamemode.SDK.Editor
{
    [CustomEditor(typeof(Fusion5vs5GamemodeBehaviour), editorForChildClasses: true)]
    public class Fusion5vs5GamemodeBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5GamemodeBehaviour;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(Fusion5vs5GamemodeDescriptor), editorForChildClasses: false)]
    public class Fusion5vs5GamemodeDescriptorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5GamemodeDescriptor;
            if (behaviour == null)
                return;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();

            EditorGUILayout.Space(20);


            if (behaviour.CounterTerroristBuyZone == null)
            {
                EditorGUILayout.HelpBox("Counter Terrorist BuyZone must be set!", MessageType.Error);
            }
            else if (behaviour.CounterTerroristBuyZone.Team == Fusion5vs5GamemodeTeams.Terrorists)
            {
                EditorGUILayout.HelpBox("Counter Terrorist BuyZone has a Terrorist BuyZone assigned!",
                    MessageType.Error);
            }

            if (behaviour.TerroristBuyZone == null)
            {
                EditorGUILayout.HelpBox("Terrorist BuyZone must be set!", MessageType.Error);
            }
            else if (behaviour.TerroristBuyZone.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
            {
                EditorGUILayout.HelpBox("Terrorist BuyZone has a Counter Terrorist BuyZone assigned!",
                    MessageType.Error);
            }

            if (behaviour.CounterTerroristSpawnPoints.Count < 5)
            {
                EditorGUILayout.HelpBox(
                    "You must add at least 5 different Counter Terrorist Spawnpoints to the list above!",
                    MessageType.Error);
            }

            foreach (var spawnPoint in behaviour.CounterTerroristSpawnPoints)
            {
                if (spawnPoint.Team == Fusion5vs5GamemodeTeams.Terrorists)
                {
                    EditorGUILayout.HelpBox(
                        "Counter Terrorist Spawnpoint List has a Terrorist Spawnpoint assigned to it!",
                        MessageType.Error);
                    break;
                }
            }

            if (behaviour.TerroristSpawnPoints.Count < 5)
            {
                EditorGUILayout.HelpBox("You must add at least 5 different Terrorist Spawnpoints to the list above!",
                    MessageType.Error);
            }

            foreach (var spawnPoint in behaviour.TerroristSpawnPoints)
            {
                if (spawnPoint.Team == Fusion5vs5GamemodeTeams.CounterTerrorists)
                {
                    EditorGUILayout.HelpBox(
                        "Terrorist Spawnpoint List has a CounterTerrorist Spawnpoint assigned to it!",
                        MessageType.Error);
                    break;
                }
            }
        }
    }

    [CustomEditor(typeof(BuyZone), editorForChildClasses: false)]
    public class BuyZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as BuyZone;
            if (behaviour == null)
                return;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();

            EditorGUILayout.Space(20);

            Collider col = behaviour.gameObject.GetComponent<Collider>();
            if (col != null)
            {
                if (!col.isTrigger)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox(
                        "Warning, the collider attached to this GameObject is not set to \"Is Trigger\". Players and objects will collide with this collider.",
                        MessageType.Warning);
                    if (GUILayout.Button("Auto Fix",  new[] { GUILayout.ExpandHeight(true) }))
                        col.isTrigger = true;
                    GUILayout.EndHorizontal();
                }
            }

            if (behaviour.gameObject.layer != 27)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    "Warning, this GameObject's layer must be set to  \"Trigger\". Otherwise, the BuyZone will not work.",
                    MessageType.Warning);
                if (GUILayout.Button("Auto Fix",  new[] { GUILayout.ExpandHeight(true) }))
                    behaviour.gameObject.layer = 27;
                GUILayout.EndHorizontal();
            }
        }
    }

    [CustomEditor(typeof(Fusion5vs5Spawnpoint), editorForChildClasses: false)]
    [CanEditMultipleObjects]
    public class Fusion5vs5SpawnpointEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var behaviour = target as Fusion5vs5Spawnpoint;
            if (behaviour == null)
                return;
            if (behaviour.Comment != null)
            {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }
}
#endif