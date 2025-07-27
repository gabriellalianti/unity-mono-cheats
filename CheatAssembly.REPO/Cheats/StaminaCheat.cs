using System.Reflection;

namespace CheatAssembly.REPO.Cheats
{
    public class StaminaCheat
    {
        private static bool enabled = false;
        private const float maxStaminaValue = 999;

        private static FieldInfo energyCurrentField = null;
        private static FieldInfo energyStartField = null;

        public static bool Enabled => enabled;

        public static void Toggle()
        {
            enabled = !enabled;

            var pc = PlayerController.instance;
            if (pc == null) return;

            // Restore original stamina if disabling
            if (energyCurrentField == null)
                energyCurrentField = typeof(PlayerController).GetField("EnergyCurrent", BindingFlags.Instance | BindingFlags.Public);
            if (energyStartField == null)
                energyStartField = typeof(PlayerController).GetField("EnergyStart", BindingFlags.Instance | BindingFlags.Public);
            if (energyCurrentField == null || energyStartField == null) return;
            if (!enabled)
            {
                float startValue = (float)energyStartField.GetValue(pc);
                energyCurrentField.SetValue(pc, startValue);
            }
        }
        
        public static void Update()
        {
            var pc = PlayerController.instance;
            if (pc == null) return;

            if (energyCurrentField == null)
            energyCurrentField = typeof(PlayerController).GetField("EnergyCurrent", BindingFlags.Instance | BindingFlags.Public);

            if (energyStartField == null)
                energyStartField = typeof(PlayerController).GetField("EnergyStart", BindingFlags.Instance | BindingFlags.Public);

            if (energyCurrentField == null || energyStartField == null) return;

            if (enabled)
            {
                float startValue = (float)energyStartField.GetValue(pc);
                energyCurrentField.SetValue(pc, maxStaminaValue);
            }
        }
    }
}
