using System;
using System.Diagnostics;
using UnityEngine;

namespace Snowy.Menu
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MenuIndexAttribute : PropertyAttribute { }
}