using System;
using System.Reflection;
using UnityEngine;

namespace CheatAssembly.REPO.Cheats
{
    public class StableCameraCheat
    {
        private static bool enabled = false;
        private static bool patched = false;

        public static bool Enabled => enabled;

        // Original values
        private static float bobUpLerpStrength;
        private static float bobSideLerpStrength;
        private static float tiltX, tiltZ, tiltXMax, tiltZMax;
        private static float strengthDefault;

        private static MonoBehaviour cameraBob, cameraTilt, cameraNoise;
        private static MonoBehaviour flashlightBob, flashlightTilt;

        public static void Toggle()
        {
            enabled = !enabled;
        }

        public static void Update()
        {
            if (!enabled)
            {
                if (patched)
                {
                    RestoreDefaults();
                    patched = false;
                }
                return;
            }

            if (!patched)
            {
                ApplyPatches();
                patched = true;
            }
        }

        private static void ApplyPatches()
        {
            cameraBob = FindComponent("CameraBob");
            cameraTilt = FindComponent("CameraTilt");
            cameraNoise = FindComponent("CameraNoise");
            flashlightBob = FindComponent("FlashlightBob");
            flashlightTilt = FindComponent("FlashlightTilt");

            if (cameraBob != null)
            {
                CacheAndSet(cameraBob, "bobUpLerpStrength", 0f, ref bobUpLerpStrength);
                CacheAndSet(cameraBob, "bobSideLerpStrength", 0f, ref bobSideLerpStrength);
            }

            if (cameraTilt != null)
            {
                CacheAndSet(cameraTilt, "tiltX", 0f, ref tiltX);
                CacheAndSet(cameraTilt, "tiltZ", 0f, ref tiltZ);
                CacheAndSet(cameraTilt, "tiltXMax", 0f, ref tiltXMax);
                CacheAndSet(cameraTilt, "tiltZMax", 0f, ref tiltZMax);
            }

            if (cameraNoise != null)
            {
                CacheAndSet(cameraNoise, "StrengthDefault", 0f, ref strengthDefault);
            }

            if (flashlightBob != null) flashlightBob.enabled = false;
            if (flashlightTilt != null) flashlightTilt.enabled = false;
        }

        private static void RestoreDefaults()
        {
            if (cameraBob != null)
            {
                Restore(cameraBob, "bobUpLerpStrength", bobUpLerpStrength);
                Restore(cameraBob, "bobSideLerpStrength", bobSideLerpStrength);
            }

            if (cameraTilt != null)
            {
                Restore(cameraTilt, "tiltX", tiltX);
                Restore(cameraTilt, "tiltZ", tiltZ);
                Restore(cameraTilt, "tiltXMax", tiltXMax);
                Restore(cameraTilt, "tiltZMax", tiltZMax);
            }

            if (cameraNoise != null)
            {
                Restore(cameraNoise, "StrengthDefault", strengthDefault);
            }

            if (flashlightBob != null) flashlightBob.enabled = true;
            if (flashlightTilt != null) flashlightTilt.enabled = true;
        }

        private static MonoBehaviour FindComponent(string typeName)
        {
            Type type = Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) return null;

            var instances = UnityEngine.Object.FindObjectsOfType(type);
            return instances.Length > 0 ? instances[0] as MonoBehaviour : null;
        }

        private static void CacheAndSet(MonoBehaviour instance, string fieldName, float newValue, ref float storage)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(float))
            {
                storage = (float)field.GetValue(instance);
                field.SetValue(instance, newValue);
            }
        }

        private static void Restore(MonoBehaviour instance, string fieldName, float value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(instance, value);
            }
        }
    }
}
