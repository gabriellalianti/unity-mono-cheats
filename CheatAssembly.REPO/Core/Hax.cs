using System.Collections.Generic;
using UnityEngine;
using CheatAssembly.REPO.Cheats;

namespace CheatAssembly.REPO.Core
{
    public class Hax : MonoBehaviour
    {
        private bool showMenu = true;
        private static List<string> logs = new List<string>();

        public static void Log(string msg)
        {
            logs.Add(msg);
            if (logs.Count > 10) logs.RemoveAt(0);
        }


        //public void Start()
        //{
        //    // Do once for initialization
        //    Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
        //    Cursor.visible = showMenu;
        //}

        // Do every tick
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // F1 to toggle GUI
                showMenu = !showMenu;
                Cursor.lockState = showMenu ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = showMenu;
            }

            // Cheats
            FlashlightCheat.Update();
            StableCameraCheat.Update();
            StaminaCheat.Update();
            HealthCheat.Update();
            EnemyCheats.Update();
        }

        public void OnGUI()
        {
            if (EnemyCheats.EnabledESP)
                EnemyCheats.OnGUI();

            if (!showMenu) return;

            UIHelper.Begin("Cheat Menu", 20, 20, 250, 300, 10, 20, 20);
            UIHelper.Space(50f);

            // Flashlight
            if (UIHelper.Button("Flashlight Boost", FlashlightCheat.Enabled))
            {
                FlashlightCheat.Toggle();
            }
            if (FlashlightCheat.Enabled)
            {
                UIHelper.Label("Flashlight Intensity");
                FlashlightCheat.Intensity = UIHelper.Slider(FlashlightCheat.Intensity, 1f, 10f);
            }

            // Stable Camera
            if (UIHelper.Button("Stable Camera: ", StableCameraCheat.Enabled))
            {
                StableCameraCheat.Toggle();
            }

            // Stamina
            if (UIHelper.Button("Infinite Stamina: ", StaminaCheat.Enabled))
            {
                StaminaCheat.Toggle();
            }

            // Health
            if (UIHelper.Button("Infinite Health/Invulnerability: ", HealthCheat.Enabled))
            {
                HealthCheat.Toggle();
            }

            // Enemy ESP
            if (UIHelper.Button("Enemy ESP: ", EnemyCheats.EnabledESP))
            {
                EnemyCheats.ToggleESP();
            }
            // Kill all Enemies
            if (UIHelper.Button("Kill all enemies"))
            {
                EnemyCheats.KillAllEnemies();
            }

            // Revive multiplayer
            if (UIHelper.Button("Revive players"))
            {
                ReviveCheat.ReviveAllPlayers();
            }

            // Logs
            if (UIHelper.Button("Clear logs"))
            {
                logs.Clear();
            }
            UIHelper.Label("Logs:");
            foreach (var log in logs)
                UIHelper.Label(log);

        }
    }
}
