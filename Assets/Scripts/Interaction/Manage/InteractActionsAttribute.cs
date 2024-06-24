using System;
using System.Linq;
using UnityEngine;

namespace Interaction
{
    public class InteractActionsAttribute : PropertyAttribute
    {
        public Type[] types;
        
        public InteractActionsAttribute()
        {
            // Get all the types that implement the IInteractAction interface
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IInteractAction).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToArray();
        }
    }
}