using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace QuickCustomizer
{
    public class CustomizerUIComponent : MonoBehaviour
    {
        public CustomizerUIComponent(IntPtr ptr) : base(ptr) { }

        public static bool showMenu = false;
        private static bool showWelcomeBanner = true;
        private static int selectedTab = 0;
        private static readonly string[] tabNames = { "Colors", "Hats", "Visors", "Skins", "Pets", "Nameplates", "Profile" };
        private static Vector2 scrollPos = Vector2.zero;

        private static bool pendingWelcomeChat = false;
        private static float chatRetryTimer = 0f;
        private static int chatRetryCount = 0;

        private static List<string> unlockedHats = new List<string>();
        private static List<string> unlockedVisors = new List<string>();
        private static List<string> unlockedSkins = new List<string>();
        private static List<string> unlockedPets = new List<string>();
        private static List<string> unlockedNamePlates = new List<string>();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showMenu = !showMenu;
                if (showMenu)
                {
                    CacheCosmetics();
                }
            }

            if (pendingWelcomeChat)
            {
                chatRetryTimer -= Time.deltaTime;
                if (chatRetryTimer <= 0f)
                {
                    chatRetryTimer = 0.5f; 
                    chatRetryCount++;

                    if (SendWelcomeToChat() || chatRetryCount > 15)
                    {
                        pendingWelcomeChat = false; 
                    }
                }
            }

            FixFortegreenHand();
        }

        private static void FixFortegreenHand()
        {
            try
            {
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
                {
                    // Explicit cast added here to fix CS0266
                    byte colorId = (byte)PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId;
                    if (colorId == 18 || colorId > 18)
                    {
                        var renderers = UnityEngine.Object.FindObjectsOfType<SpriteRenderer>();
                        foreach (var sr in renderers)
                        {
                            if (sr != null && sr.gameObject.name.IndexOf("hand", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (sr.color.r > 0.7f && sr.color.g < 0.3f)
                                {
                                    sr.color = new Color(0.1f, 0.55f, 0.27f, sr.color.a);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static void ResetWelcomeBanner()
        {
            showWelcomeBanner = true;
            pendingWelcomeChat = true;
            chatRetryTimer = 0.5f;
            chatRetryCount = 0;
        }

        public static bool SendWelcomeToChat()
        {
            try
            {
                string msg = "[QuickCustomizer] Press F1 for menu. Chat commands: /color, /name, /hat, /visor, /skin, /pet, /blank, /random, /help";
                
                if (HudManager.Instance != null && HudManager.Instance.Chat != null && PlayerControl.LocalPlayer != null)
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static void CacheCosmetics()
        {
            try
            {
                unlockedHats.Clear();
                unlockedVisors.Clear();
                unlockedSkins.Clear();
                unlockedPets.Clear();
                unlockedNamePlates.Clear();

                if (HatManager.Instance != null)
                {
                    var hats = HatManager.Instance.GetUnlockedHats();
                    if (hats != null) foreach (var h in hats) if (h != null) unlockedHats.Add(h.ProductId);

                    var visors = HatManager.Instance.GetUnlockedVisors();
                    if (visors != null) foreach (var v in visors) if (v != null) unlockedVisors.Add(v.ProductId);

                    var skins = HatManager.Instance.GetUnlockedSkins();
                    if (skins != null) foreach (var s in skins) if (s != null) unlockedSkins.Add(s.ProductId);

                    var pets = HatManager.Instance.GetUnlockedPets();
                    if (pets != null) foreach (var p in pets) if (p != null) unlockedPets.Add(p.ProductId);

                    try
                    {
                        var namePlates = HatManager.Instance.GetUnlockedNamePlates();
                        if (namePlates != null) foreach (var np in namePlates) if (np != null) unlockedNamePlates.Add(np.ProductId);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, 1000, 40), "<size=14><color=yellow>[QuickCustomizer] Press 'F1' for Wardrobe | Use /color, /name, /hat, /visor, /skin, /pet, /blank, /random</color></size>");

            if (showWelcomeBanner && PlayerControl.LocalPlayer != null)
            {
                Rect welcomeRect = new Rect(Screen.width / 2f - 250, Screen.height / 2f - 140, 500, 280);
                GUI.Box(welcomeRect, "Welcome to QuickCustomizer!");
                
                GUI.BeginGroup(new Rect(welcomeRect.x + 20, welcomeRect.y + 35, welcomeRect.width - 40, welcomeRect.height - 45));
                GUI.Label(new Rect(0, 5, 460, 160), 
                    "<b>You're running QuickCustomizer!</b>\n\n" +
                    "• Press <b><color=yellow>F1</color></b> anytime for the wardrobe menu.\n" +
                    "• Chat commands: <color=cyan>/color</color>, <color=cyan>/name</color>, <color=cyan>/hat</color>, <color=cyan>/visor</color>, <color=cyan>/skin</color>, <color=cyan>/pet</color>, <color=cyan>/blank</color>, <color=cyan>/random</color>\n" +
                    "• Fortegreen hand fix & blank name support active!");

                if (GUI.Button(new Rect(130, 185, 200, 35), "Got it, let's play!"))
                {
                    showWelcomeBanner = false;
                }
                GUI.EndGroup();
            }

            if (!showMenu) return;

            Rect windowRect = new Rect(150, 130, 540, 460);
            GUI.Box(windowRect, "Wardrobe Customizer [QuickCustomizer]");

            if (PlayerControl.LocalPlayer == null)
            {
                GUI.BeginGroup(new Rect(windowRect.x + 10, windowRect.y + 25, windowRect.width - 20, windowRect.height - 35));
                GUI.Label(new Rect(10, 60, 500, 50), "<color=red><b>Please join a Lobby or Freeplay game first!</b>\nCosmetics and name customization require an active player instance.</color>");
                if (GUI.Button(new Rect(10, windowRect.height - 75, windowRect.width - 40, 32), "Close Menu"))
                {
                    showMenu = false;
                }
                GUI.EndGroup();
                return;
            }

            GUI.BeginGroup(new Rect(windowRect.x + 10, windowRect.y + 25, windowRect.width - 20, windowRect.height - 35));

            try
            {
                float tabWidth = (windowRect.width - 20) / 7f;
                for (int i = 0; i < tabNames.Length; i++)
                {
                    if (GUI.Button(new Rect(i * tabWidth, 10, tabWidth - 2, 30), tabNames[i]))
                    {
                        selectedTab = i;
                        scrollPos = Vector2.zero;
                    }
                }

                float contentY = 55;
                float contentHeight = 310;
                float contentWidth = windowRect.width - 25;

                scrollPos = GUI.BeginScrollView(
                    new Rect(0, contentY, contentWidth, contentHeight), 
                    scrollPos, 
                    new Rect(0, 0, contentWidth - 20, GetContentHeight())
                );

                if (selectedTab == 0)
                {
                    GUI.Label(new Rect(0, 0, 400, 25), "Select Color ID (0 - 18) [18 = Fortegreen, /color 31 for Fortegreen]:");
                    int columns = 6;
                    for (int i = 0; i < 19; i += columns)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            int colorId = i + j;
                            if (colorId < 19)
                            {
                                float btnX = j * 72;
                                float btnY = 30 + (i / columns) * 35;
                                string labelTxt = colorId == 18 ? "Fortegreen" : $"Color {colorId}";
                                if (GUI.Button(new Rect(btnX, btnY, 68, 30), labelTxt))
                                {
                                    CustomizerLogic.SetColor((byte)colorId);
                                    PlayerPrefs.SetInt("QC_SavedColor", colorId);
                                    PlayerPrefs.Save();
                                }
                            }
                        }
                    }
                }
                else if (selectedTab == 1) DrawItemGrid(unlockedHats, "Hat", (id) => { PlayerControl.LocalPlayer?.SetHat(id, 0); PlayerPrefs.SetString("QC_SavedHat", id); PlayerPrefs.Save(); });
                else if (selectedTab == 2) DrawItemGrid(unlockedVisors, "Visor", (id) => { PlayerControl.LocalPlayer?.SetVisor(id, 0); PlayerPrefs.SetString("QC_SavedVisor", id); PlayerPrefs.Save(); });
                else if (selectedTab == 3) DrawItemGrid(unlockedSkins, "Skin", (id) => { PlayerControl.LocalPlayer?.SetSkin(id, 0); PlayerPrefs.SetString("QC_SavedSkin", id); PlayerPrefs.Save(); });
                else if (selectedTab == 4) DrawItemGrid(unlockedPets, "Pet", (id) => { PlayerControl.LocalPlayer?.SetPet(id, 0); PlayerPrefs.SetString("QC_SavedPet", id); PlayerPrefs.Save(); });
                else if (selectedTab == 5) DrawItemGrid(unlockedNamePlates, "Nameplate", (id) => { CustomizerLogic.SetNamePlate(id); PlayerPrefs.SetString("QC_SavedNamePlate", id); PlayerPrefs.Save(); });
                else if (selectedTab == 6)
                {
                    GUI.Label(new Rect(0, 0, 400, 25), "<b>Choose Display Name:</b>");
                    string[] namePresets = { "SuperMarioTime", "SMT", "Crewmate", "Impostor", "Player", "Pro", "Guest", "Host", "Sus", "Blank", "???" };
                    int columns = 3;
                    float btnWidth = 145;
                    float btnHeight = 35;
                    float spacingX = 10;
                    float spacingY = 8;

                    for (int i = 0; i < namePresets.Length; i++)
                    {
                        int row = i / columns;
                        int col = i % columns;
                        float x = col * (btnWidth + spacingX);
                        float y = 32 + row * (btnHeight + spacingY);

                        string presetName = namePresets[i];
                        string nameToSet = presetName == "Blank" ? "\u3164" : presetName;

                        if (GUI.Button(new Rect(x, y, btnWidth, btnHeight), presetName))
                        {
                            CustomizerLogic.SetName(nameToSet);
                            PlayerPrefs.SetString("QC_SavedName", nameToSet);
                            PlayerPrefs.Save();
                        }
                    }
                }

                GUI.EndScrollView();

                if (GUI.Button(new Rect(10, windowRect.height - 75, windowRect.width - 40, 32), "Close Menu"))
                {
                    showMenu = false;
                }
            }
            catch (Exception ex)
            {
                GUI.Label(new Rect(10, 60, 400, 100), $"UI Error: {ex.Message}");
            }

            GUI.EndGroup();
        }

        private static float GetContentHeight()
        {
            if (selectedTab == 0) return 220;
            if (selectedTab == 6) return 280;
            List<string> list = selectedTab == 1 ? unlockedHats :
                                selectedTab == 2 ? unlockedVisors :
                                selectedTab == 3 ? unlockedSkins :
                                selectedTab == 4 ? unlockedPets : unlockedNamePlates;
            int rows = Mathf.CeilToInt((float)(list?.Count ?? 0) / 3f);
            return Mathf.Max(300, rows * 38 + 40);
        }

        private static void DrawItemGrid(List<string> items, string labelName, Action<string> equipAction)
        {
            GUI.Label(new Rect(0, 0, 400, 25), $"Select Unlocked {labelName} ({items.Count}):");
            if (items == null || items.Count == 0)
            {
                GUI.Label(new Rect(0, 35, 400, 25), $"No unlocked {labelName.ToLower()}s found.");
                return;
            }

            int columns = 3;
            float btnWidth = 150;
            float btnHeight = 32;
            float spacingX = 10;
            float spacingY = 6;

            for (int i = 0; i < items.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                float x = col * (btnWidth + spacingX);
                float y = 30 + row * (btnHeight + spacingY);

                string id = items[i];
                string displayName = id.Length > 16 ? id.Substring(0, 14) + ".." : id;

                if (GUI.Button(new Rect(x, y, btnWidth, btnHeight), displayName))
                {
                    equipAction?.Invoke(id);
                }
            }
        }
    }

    [BepInPlugin("com.quickcustomizer.mod", "QuickCustomizer", "4.0.0")]
    [BepInProcess("Among Us.exe")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony("com.quickcustomizer.mod");

        public override void Load()
        {
            Harmony.PatchAll();
            ClassInjector.RegisterTypeInIl2Cpp<CustomizerUIComponent>();
            Log.LogInfo("QuickCustomizer loaded successfully!");
        }
    }

    public static class CustomizerLogic
    {
        public static void SetColor(byte colorId)
        {
            if (PlayerControl.LocalPlayer == null) return;
            PlayerControl.LocalPlayer.CmdCheckColor(colorId);
        }

        public static void SetName(string newName)
        {
            if (newName == null) return;
            
            string processedName = newName;
            if (string.IsNullOrEmpty(processedName) || processedName.Equals("blank", StringComparison.OrdinalIgnoreCase) || processedName == "\u2800" || processedName == "\u200B" || processedName == "\u00AD")
            {
                processedName = "\u3164"; 
            }

            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
            {
                PlayerControl.LocalPlayer.Data.PlayerName = processedName;
                try
                {
                    if (PlayerControl.LocalPlayer.cosmetics != null && PlayerControl.LocalPlayer.cosmetics.nameText != null)
                    {
                        PlayerControl.LocalPlayer.cosmetics.nameText.text = processedName;
                    }
                }
                catch { }
            }
        }

        public static void SetNamePlate(string namePlateId)
        {
            if (PlayerControl.LocalPlayer == null) return;
            try
            {
                PlayerControl.LocalPlayer.SetNamePlate(namePlateId);
            }
            catch
            {
                try
                {
                    var type = PlayerControl.LocalPlayer.GetType();
                    var method = type.GetMethod("SetNamePlate", new Type[] { typeof(string) });
                    method?.Invoke(PlayerControl.LocalPlayer, new object[] { namePlateId });
                }
                catch { }
            }
        }

        public static bool SetCosmeticByQuery(string type, string query)
        {
            if (PlayerControl.LocalPlayer == null || HatManager.Instance == null) return false;
            try
            {
                if (type == "hat")
                {
                    foreach (var h in HatManager.Instance.GetUnlockedHats())
                    {
                        if (h != null && h.ProductId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            PlayerControl.LocalPlayer.SetHat(h.ProductId, 0);
                            PlayerPrefs.SetString("QC_SavedHat", h.ProductId);
                            PlayerPrefs.Save();
                            return true;
                        }
                    }
                }
                else if (type == "visor")
                {
                    foreach (var v in HatManager.Instance.GetUnlockedVisors())
                    {
                        if (v != null && v.ProductId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            PlayerControl.LocalPlayer.SetVisor(v.ProductId, 0);
                            PlayerPrefs.SetString("QC_SavedVisor", v.ProductId);
                            PlayerPrefs.Save();
                            return true;
                        }
                    }
                }
                else if (type == "skin")
                {
                    foreach (var s in HatManager.Instance.GetUnlockedSkins())
                    {
                        if (s != null && s.ProductId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            PlayerControl.LocalPlayer.SetSkin(s.ProductId, 0);
                            PlayerPrefs.SetString("QC_SavedSkin", s.ProductId);
                            PlayerPrefs.Save();
                            return true;
                        }
                    }
                }
                else if (type == "pet")
                {
                    foreach (var p in HatManager.Instance.GetUnlockedPets())
                    {
                        if (p != null && p.ProductId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            PlayerControl.LocalPlayer.SetPet(p.ProductId, 0);
                            PlayerPrefs.SetString("QC_SavedPet", p.ProductId);
                            PlayerPrefs.Save();
                            return true;
                        }
                    }
                }
                else if (type == "nameplate")
                {
                    foreach (var np in HatManager.Instance.GetUnlockedNamePlates())
                    {
                        if (np != null && np.ProductId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            SetNamePlate(np.ProductId);
                            PlayerPrefs.SetString("QC_SavedNamePlate", np.ProductId);
                            PlayerPrefs.Save();
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static void ApplySavedCustomization()
        {
            if (PlayerControl.LocalPlayer == null) return;

            try
            {
                if (PlayerPrefs.HasKey("QC_SavedColor"))
                {
                    SetColor((byte)PlayerPrefs.GetInt("QC_SavedColor"));
                }
                if (PlayerPrefs.HasKey("QC_SavedHat"))
                {
                    PlayerControl.LocalPlayer.SetHat(PlayerPrefs.GetString("QC_SavedHat"), 0);
                }
                if (PlayerPrefs.HasKey("QC_SavedVisor"))
                {
                    PlayerControl.LocalPlayer.SetVisor(PlayerPrefs.GetString("QC_SavedVisor"), 0);
                }
                if (PlayerPrefs.HasKey("QC_SavedSkin"))
                {
                    PlayerControl.LocalPlayer.SetSkin(PlayerPrefs.GetString("QC_SavedSkin"), 0);
                }
                if (PlayerPrefs.HasKey("QC_SavedPet"))
                {
                    PlayerControl.LocalPlayer.SetPet(PlayerPrefs.GetString("QC_SavedPet"), 0);
                }
                if (PlayerPrefs.HasKey("QC_SavedNamePlate"))
                {
                    SetNamePlate(PlayerPrefs.GetString("QC_SavedNamePlate"));
                }
                if (PlayerPrefs.HasKey("QC_SavedName"))
                {
                    SetName(PlayerPrefs.GetString("QC_SavedName"));
                }
            }
            catch { }
        }

        public static bool HandleChatCommand(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;

            if (message.StartsWith("/color ", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(message.Substring(7).Trim(), out int colorId))
                {
                    SetColor((byte)colorId);
                    PlayerPrefs.SetInt("QC_SavedColor", colorId);
                    PlayerPrefs.Save();
                    return true;
                }
            }
            else if (message.StartsWith("/name ", StringComparison.OrdinalIgnoreCase))
            {
                string newName = message.Substring(6).Trim();
                if (string.IsNullOrEmpty(newName) || newName.Equals("blank", StringComparison.OrdinalIgnoreCase))
                {
                    newName = "\u3164";
                }
                SetName(newName);
                PlayerPrefs.SetString("QC_SavedName", newName);
                PlayerPrefs.Save();
                return true;
            }
            else if (message.StartsWith("/hat ", StringComparison.OrdinalIgnoreCase)) return SetCosmeticByQuery("hat", message.Substring(5).Trim());
            else if (message.StartsWith("/visor ", StringComparison.OrdinalIgnoreCase)) return SetCosmeticByQuery("visor", message.Substring(7).Trim());
            else if (message.StartsWith("/skin ", StringComparison.OrdinalIgnoreCase)) return SetCosmeticByQuery("skin", message.Substring(6).Trim());
            else if (message.StartsWith("/pet ", StringComparison.OrdinalIgnoreCase)) return SetCosmeticByQuery("pet", message.Substring(5).Trim());
            else if (message.StartsWith("/nameplate ", StringComparison.OrdinalIgnoreCase)) return SetCosmeticByQuery("nameplate", message.Substring(11).Trim());
            else if (message.Equals("/blank", StringComparison.OrdinalIgnoreCase) || message.Equals("/noname", StringComparison.OrdinalIgnoreCase))
            {
                SetName("\u3164");
                PlayerPrefs.SetString("QC_SavedName", "\u3164");
                PlayerPrefs.Save();
                return true;
            }
            else if (message.Equals("/random", StringComparison.OrdinalIgnoreCase))
            {
                System.Random rand = new System.Random();
                byte randomColor = (byte)rand.Next(0, 19);
                SetColor(randomColor);
                PlayerPrefs.SetInt("QC_SavedColor", randomColor);

                string[] randomNames = { "SuperMarioTime", "Crewmate", "Impostor", "Pro", "Sus", "\u3164", "Guest" };
                string chosenName = randomNames[rand.Next(randomNames.Length)];
                SetName(chosenName);
                PlayerPrefs.SetString("QC_SavedName", chosenName);
                PlayerPrefs.Save();
                return true;
            }
            else if (message.Equals("/help", StringComparison.OrdinalIgnoreCase) || message.Equals("/info", StringComparison.OrdinalIgnoreCase))
            {
                CustomizerUIComponent.SendWelcomeToChat();
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerSpawnPatch
    {
        private static PlayerControl lastProcessedPlayer = null;
        private static bool uiCreated = false;

        public static void Postfix(PlayerControl __instance)
        {
            if (PlayerControl.LocalPlayer == null || __instance != PlayerControl.LocalPlayer) return;

            if (!uiCreated)
            {
                uiCreated = true;
                var uiObject = new GameObject("QuickCustomizerUIObject");
                uiObject.AddComponent<CustomizerUIComponent>();
                UnityEngine.Object.DontDestroyOnLoad(uiObject);
            }

            if (PlayerControl.LocalPlayer != lastProcessedPlayer)
            {
                lastProcessedPlayer = PlayerControl.LocalPlayer;
                CustomizerUIComponent.ResetWelcomeBanner(); 
                CustomizerLogic.ApplySavedCustomization();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
    public static class ColorCheckBypassPatch
    {
        public static bool Prefix(PlayerControl __instance, byte bodyColor)
        {
            __instance.RpcSetColor(bodyColor);
            return false;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatCommandPatch
    {
        public static bool Prefix(ChatController __instance)
        {
            try
            {
                if (__instance != null && __instance.freeChatField != null)
                {
                    string text = string.Empty;
                    var chatField = __instance.freeChatField;
                    var chatType = chatField.GetType();

                    var textProp = chatType.GetProperty("text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ?? 
                                   chatType.GetProperty("Text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (textProp != null)
                    {
                        text = textProp.GetValue(chatField) as string;
                    }
                    else
                    {
                        var textField = chatType.GetField("text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ?? 
                                        chatType.GetField("Text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (textField != null) text = textField.GetValue(chatField) as string;
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        text = text.Trim();
                        if (CustomizerLogic.HandleChatCommand(text))
                        {
                            if (textProp != null)
                            {
                                textProp.SetValue(chatField, string.Empty);
                            }
                            else
                            {
                                var textField = chatType.GetField("text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ?? 
                                                chatType.GetField("Text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                textField?.SetValue(chatField, string.Empty);
                            }

                            __instance.SetVisible(false);
                            return false; 
                        }
                    }
                }
            }
            catch { }
            return true;
        }
    }
}