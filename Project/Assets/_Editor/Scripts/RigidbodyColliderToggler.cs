using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AkaitoAi
{
    public class RigidbodyColliderToggler : MonoBehaviour
    {
        [SerializeField] private bool kinematicState = true;
        [SerializeField] private bool colliderState = true;

        void Reset()
        {
            UpdateRigidbodyStates();
            UpdateColliderStates();
        }

        public void ToggleRigidbodyKinematic()
        {
            // Flip the state first
            kinematicState = !kinematicState;
            UpdateRigidbodyStates();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void ToggleColliders()
        {
            // Flip the state first
            colliderState = !colliderState;
            UpdateColliderStates();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void UpdateRigidbodyStates()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = kinematicState;
            }
        }

        private void UpdateColliderStates()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = colliderState;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(RigidbodyColliderToggler))]
        public class RigidbodyColliderTogglerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                RigidbodyColliderToggler toggler = (RigidbodyColliderToggler)target;

                if (GUILayout.Button("Toggle Rigidbody Kinematic"))
                {
                    toggler.ToggleRigidbodyKinematic();
                }

                if (GUILayout.Button("Toggle Colliders"))
                {
                    toggler.ToggleColliders();
                }
            }
        }
#endif
    }
}