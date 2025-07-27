using System;
using System.Collections.Generic;
using System.Reflection;
using CheatAssembly.REPO.Core;
using UnityEngine;

namespace CheatAssembly.REPO.Cheats
{
    public static class EnemyCheats
    {
        private static List<Enemy> enemies = new List<Enemy>();
        private static GUIStyle labelStyle;
        private static Camera camera;
        private static float scaleX, scaleY;

        private static bool enabledESP = false;

        public static bool EnabledESP => enabledESP;

        public static void ToggleESP()
        {
            enabledESP = !enabledESP;
            if (enabledESP)
            {
                RefreshEnemyList();
                Hax.Log("Enemies found: " + enemies.Count.ToString());
            }
        }

        public static void Update()
        {
            if (!EnabledESP) return;
            RefreshEnemyList();
        }

        public static void OnGUI()
        {
            if (!EnabledESP || enemies.Count == 0) return;

            if (camera == null) camera = Camera.main;
            scaleX = (float)Screen.width / camera.pixelWidth;
            scaleY = (float)Screen.height / camera.pixelHeight;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.red },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            foreach (var enemy in enemies)
            {
                // Hax.Log("in enemy loop");
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.CenterTransform == null)
                {
                    // Hax.Log("null");
                    continue;
                }
                    

                Vector3 footpos = enemy.transform.position;
                Vector3 headpos = enemy.transform.position + Vector3.up * 2f;

                Vector3 screenFootPos = camera.WorldToScreenPoint(footpos);
                Vector3 screenHeadPos = camera.WorldToScreenPoint(headpos);

                if (screenFootPos.z <= 0 || screenHeadPos.z <= 0)
                {
                    // Hax.Log("behind the camera");
                    continue;
                }

                float footX = screenFootPos.x * scaleX;
                float footY = Screen.height - (screenFootPos.y * scaleY);
                float headY = Screen.height - (screenHeadPos.y * scaleY);

                float height = Mathf.Abs(footY - headY);
                float distance = screenFootPos.z;
                float width = Mathf.Clamp((200f / distance) * scaleX, 30f, height * 1.2f);
                height = Mathf.Clamp(height, 40f, 400f);

                float boxX = footX - width / 2f;
                float boxY = headY;
                DrawBox(boxX, boxY, width, height, Color.red);

                // Display name + distance
                string enemyName = "Enemy";
                var parent = enemy.gameObject.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                if (parent != null)
                {
                    var nameField = parent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                    if (nameField != null)
                        enemyName = nameField.GetValue(parent) as string ?? "Enemy";
                }

                float actualDistance = Vector3.Distance(camera.transform.position, enemy.gameObject.transform.position);
                string label = $"{enemyName} [{actualDistance:F1}m]";

                float labelWidth = 100f;
                float labelHeight = labelStyle.CalcHeight(new GUIContent(label), labelWidth);
                float labelX = footX - labelWidth / 2f;
                float labelY = boxY - labelHeight;

                GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), label, labelStyle);
            }
        }

        private static void DrawBox(float x, float y, float w, float h, Color color)
        {
            // Hax.Log("in DrawBox");
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, w, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + h, w, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 1, h), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + w, y, 1, h), Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void RefreshEnemyList()
        {
            enemies.Clear();

            var type = Type.GetType("EnemyDirector, Assembly-CSharp");
            var instance = type?.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (instance == null) return;

            var spawnedField = type.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
            var spawned = spawnedField?.GetValue(instance) as IEnumerable<object>;
            if (spawned == null) return;

            foreach (var e in spawned)
            {
                if (e == null) continue;
                var field = e.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance);

                var enemy = field?.GetValue(e) as Enemy;
                if (enemy != null)
                    enemies.Add(enemy);
            }
        }
        public static void KillAllEnemies()
        {
            RefreshEnemyList();

            if (enemies.Count == 0)
            {
                Hax.Log("No enemies found");
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                var healthField = enemy.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) continue;

                var healthInstance = healthField.GetValue(enemy);
                if (healthInstance == null) continue;

                var hurtMethod = healthInstance.GetType().GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (hurtMethod == null) continue;

                hurtMethod.Invoke(healthInstance, new object[] { 9999, Vector3.zero });

                string enemyName = "Enemy";
                var parent = enemy.gameObject.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                if (parent != null)
                {
                    var nameField = parent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                    if (nameField != null)
                        enemyName = nameField.GetValue(parent) as string ?? "Enemy";
                }

                Hax.Log("Killed enemy: " + enemyName);
            }
        }
    }
}
