using System;
using System.Reflection;

namespace CheatAssembly.REPO.Cheats
{
    internal class HealthCheat
    {
        private static bool enabled = false;
        private const int maxHealthValue = 999;

        private static FieldInfo healthField = null;
        private static FieldInfo maxHealthField = null;
        private static FieldInfo invincibleTimerField = null;
        private static FieldInfo playerHealthPreviousField = null;

        private static int originalHealth;
        private static int originalMaxHealth;
        private static float originalInvincibleTimer;
        private static bool haveOriginals = false;

        public static bool Enabled => enabled;

        public static void Toggle()
        {
            enabled = !enabled;

            var ph = GetPlayerHealthInstance();
            if (ph == null) return;
            EnsureFieldInfos();

            if (enabled)
            {
                originalHealth = (int)healthField.GetValue(ph);
                originalMaxHealth = (int)maxHealthField.GetValue(ph);
                originalInvincibleTimer = (float)invincibleTimerField.GetValue(ph);
                haveOriginals = true;
            }
            else if (haveOriginals)
            {
                healthField.SetValue(ph, originalHealth);
                maxHealthField.SetValue(ph, originalMaxHealth);
                invincibleTimerField.SetValue(ph, originalInvincibleTimer);

                playerHealthPreviousField.SetValue(HealthUI.instance, int.MinValue);

                haveOriginals = false;
            }
        }

        public static void Update()
        {
            var ph = GetPlayerHealthInstance();
            if (ph == null) return;

            EnsureFieldInfos();
            if (!enabled) return;

            healthField.SetValue(ph, maxHealthValue);
            maxHealthField.SetValue(ph, maxHealthValue);
            invincibleTimerField.SetValue(ph, float.MaxValue);

            // Redraw UI
            playerHealthPreviousField.SetValue(HealthUI.instance, int.MinValue);
        }

        private static PlayerHealth GetPlayerHealthInstance()
        {
            var pcType = Type.GetType("PlayerController, Assembly-CSharp");
            var pcObj = UnityEngine.Object.FindObjectOfType(pcType, true) as PlayerController;
            if (pcObj == null) return null;

            var avatarField = pcType.GetField("playerAvatarScript", BindingFlags.Instance | BindingFlags.Public).GetValue(pcObj);
            return (PlayerHealth)avatarField
              .GetType()
              .GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public)
              .GetValue(avatarField);
        }

        private static void EnsureFieldInfos()
        {
            if (healthField == null)
                healthField = typeof(PlayerHealth).GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (maxHealthField == null)
                maxHealthField = typeof(PlayerHealth).GetField("maxHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (invincibleTimerField == null)
                invincibleTimerField = typeof(PlayerHealth).GetField("invincibleTimer", BindingFlags.Instance | BindingFlags.NonPublic);

            if (playerHealthPreviousField == null)
                playerHealthPreviousField = typeof(HealthUI).GetField("playerHealthPrevious", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}
