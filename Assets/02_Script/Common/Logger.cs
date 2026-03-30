using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LittleSword.Common
{
    public static class Logger
    {
        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void logError(object message)
        {
            Debug.LogError(message);
        }

        [Conditional("DEVELOP_MODE")]
        [Conditional("UNITY_EDITOR")]
        public static void logWarning(object message)
        {
            Debug.LogWarning(message);
        }
    }
}
