using System.Collections.Generic;
using System.Reflection;

namespace CheatAssembly.REPO.Cheats
{
    public static class ReviveCheat
    {
        private static List<PlayerAvatar> playerList = new List<PlayerAvatar>();

        public static void ReviveAllPlayers()
        {
            playerList.Clear();
            var players = SemiFunc.PlayerGetList();
            // Hax.Log("revive found " + players.Count + " players");

            if (players == null || players.Count == 0) return;

            foreach (var player in players)
            {
                // Hax.Log("in loop1");
                if (player == null) continue;
                var deathHead = player.GetType().GetField("playerDeathHead", BindingFlags.Public | BindingFlags.Instance);
                var playerHealthField = player.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (deathHead == null && playerHealthField == null)
                {
                    // Hax.Log("deathhead and playerhealthfield both null");
                    continue;
                }

                if (deathHead != null)
                {
                    // Hax.Log("deathhead not null");
                    var reviveInstance = deathHead.GetValue(player);
                    if (reviveInstance != null)
                    {
                        // Hax.Log("revive instance not null");
                        var inExtractionPointField = reviveInstance.GetType().GetField("inExtractionPoint", BindingFlags.NonPublic | BindingFlags.Instance);
                        var reviveMethod = reviveInstance.GetType().GetMethod("Revive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (inExtractionPointField != null)
                        {
                            inExtractionPointField.SetValue(reviveInstance, true);
                            // Hax.Log("extr true");
                        }
                        if (reviveMethod != null)
                        {
                            reviveMethod.Invoke(reviveInstance, null);
                            // Hax.Log("REVIVE INVOKED");
                        }
                    }
                }

                // add health
                if (playerHealthField == null) continue;
                var playerHealthInstance = playerHealthField.GetValue(player);
                if (playerHealthInstance != null)
                {
                    // Hax.Log("playerhealthinstance not null");
                    var maxHealthField = playerHealthInstance.GetType().GetField("maxHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var healthField = playerHealthInstance.GetType().GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    int maxHealth = maxHealthField != null ? (int)maxHealthField.GetValue(playerHealthInstance) : 100;

                    if (healthField != null)
                    {
                        healthField.SetValue(playerHealthInstance, maxHealth);
                        // Hax.Log("healthfield not null");
                    }
                }
            }
        }
    }
}
