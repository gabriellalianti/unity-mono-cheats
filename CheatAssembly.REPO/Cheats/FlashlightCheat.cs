using System.Reflection;

namespace CheatAssembly.REPO.Cheats
{
    public class FlashlightCheat
    {
        private static bool enabled = false;
        private static float intensity = 5f;
        private static float? original = null;
        private static FieldInfo baseIntensityField = null;

        public static bool Enabled => enabled;
        public static float Intensity
        {
            get => intensity;
            set => intensity = value;
        }

        public static void Toggle()
        {
            enabled = !enabled;
        }

        public static void Update()
        {
            var flashlight = FlashlightController.Instance;
            if (flashlight == null)
            {
                // Hax.Log("FlaslightController instance not found");
                return;
            }

            if (enabled)
            {
                if (baseIntensityField == null)
                    baseIntensityField = typeof(FlashlightController).GetField("baseIntensity", BindingFlags.NonPublic | BindingFlags.Instance);

                if (original == null && baseIntensityField != null)
                    original = (float)baseIntensityField.GetValue(flashlight);

                if (baseIntensityField != null)
                    baseIntensityField.SetValue(flashlight, intensity);

                flashlight.spotlight.intensity = intensity;
                flashlight.spotlight.enabled = true;
                flashlight.halo.enabled = true;
                flashlight.LightActive = true;
            }
            else if (original != null)
            {
                if (baseIntensityField != null)
                    baseIntensityField.SetValue(flashlight, original.Value);

                flashlight.spotlight.intensity = original.Value;
                original = null;
            }
        }
    }
}
