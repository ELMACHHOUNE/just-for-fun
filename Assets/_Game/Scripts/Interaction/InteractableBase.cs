using System.Collections.Generic;
using UnityEngine;

namespace LWalida
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        string GetPromptText();
        void Interact();
    }

    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        public static readonly List<InteractableBase> Registry = new List<InteractableBase>();

        [SerializeField] protected string displayName = "Object";
        [SerializeField] protected float interactionRadius = 2.4f;
        [SerializeField] public bool showPromptWhenLocked = true;

        public string DisplayName => displayName;
        public float InteractionRadius => interactionRadius;
        public virtual bool CanInteract => true;

        protected virtual void OnEnable()
        {
            if (!Registry.Contains(this)) Registry.Add(this);
        }

        protected virtual void OnDisable()
        {
            Registry.Remove(this);
        }

        public virtual string GetPromptText()
        {
            return "E \u2014 " + displayName;
        }

        public abstract void Interact();

        public void SetupDisplay(string name, float radius = 2.4f)
        {
            displayName = name;
            interactionRadius = radius;
        }

        public bool IsInRange(Vector3 position, float extra = 0f)
        {
            return (position - transform.position).sqrMagnitude <=
                   (interactionRadius + extra) * (interactionRadius + extra);
        }

        public float SqrDistanceTo(Vector3 position)
        {
            return (position - transform.position).sqrMagnitude;
        }
    }
}

