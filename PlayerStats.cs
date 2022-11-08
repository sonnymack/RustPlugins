using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using System.Text;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("PlayerStats", "Sonnymack", 0.1)]
    [Description("A way to get all the players gameplay statistics.")]
    public class PlayerStats : RustPlugin
    {
        //TODO: Finish UI, Add every player stats into one single data file instead of have one file for each player, Fix the stats.wipe command as it basically crashes the server
        //Remove all debugging text

        PLRStats stats;
        public string BG = "Background"; 
        static Dictionary<ulong, PLRStats> cachedPlayerStats = new Dictionary<ulong, PLRStats>();

        void Init()
        {
            stats = Interface.Oxide.DataFileSystem.ReadObject<PLRStats>("PlayerStats");
        }

        [ConsoleCommand("stats.wipe")]
        private void wipeStats(ConsoleSystem.Arg arg)
        {
           
            if (!arg.IsRcon) return;
            PrintWarning("Wiping all player stats! Count: ", GetAllPlayers().Count);
            GetAllPlayers().ForEach(ID => PLRStats.Reset(ID));
            Puts("Stats Wiped");
        }
        [ChatCommand("top")]
        private void topCommand(BasePlayer player, string Command, string[] args) //OrderByDescending(x => x.Value).Take(5)
        {
            Puts("Top command");
            if (args.Length < 1)
            {
                Puts("Lowered Args");
                SendMessage(player, "You need to specify a statistic, such as /top kills etc");
                return;
            }
            switch (args[0])
            {
                case "kills":
                    GetTopKills(player);
                    break;

                case "deaths":
                    GetTopDeaths(player);
                    break;

                case "kdr":
                    GetTopKDR(player);
                    break;

                case "barrels":
                case "barrels hit":
                    GetTopBarrels(player);
                    break;

                case "structures":
                case "structures placed":
                    GetTopStructures(player);
                    break;

                case "bullets":
                case "bullets fired":
                    GetTopBulletsFired(player);
                    break;

                case "helicopters":
                case "helicopters killed":
                    GetTopHeliKilled(player);
                    break;

                case "structures upgraded":
                    GetTopStructuresUpgraded(player);
                    break;

                default:
                    GetTopKills(player);
                    break;
            }
        }
        [ChatCommand("Test")]
        void test(BasePlayer player)
        {
            if (!player.IsAdmin) return;
            foreach (KeyValuePair<ulong, PLRStats> keyValuePair in cachedPlayerStats)
            {
                SendMessage(player, keyValuePair.Key.ToString());
            }
        }
        
        [ChatCommand("stats")]
        private void statscommand(BasePlayer player, string command, string[] args)
        {
            var target = player.userID;
            BasePlayer targetPlr = null;
            if (args.Length > 0)
            {
                var nameOrId = args[0];
                targetPlr = BasePlayer.FindAwakeOrSleeping(nameOrId);
                if (targetPlr == null)
                {
                    player.ChatMessage("Player not found!");
                    return;
                }
                target = targetPlr.userID;
                

                if (cachedPlayerStats.ContainsKey(target))
                {
                    PLRStats.TryLoad(target);
                    if (targetPlr != null)
                    {
                        Puts("Found Player");
                        player.ChatMessage($"Stats for {targetPlr.displayName} \n\nKills › {cachedPlayerStats[target].PlayerKills} \nDeaths › {cachedPlayerStats[target].Deaths}\nKDR › {cachedPlayerStats[target].KDR}\nBarrels Hit › {cachedPlayerStats[target].BarrelsHit}\nStructures Placed › {cachedPlayerStats[target].StructuresPlaced}\nBullets Fired › {cachedPlayerStats[target].BulletsFired}\nHelicopters Killed › {cachedPlayerStats[target].HelicoptersKilled}\nStructures Upgraded › {cachedPlayerStats[target].StructuresUpgraded}");
                    }
                    else player.ChatMessage("Player not found!");
                }
                else Puts("Couldn't Load Players stuff"); return;
            }
            else
            {
                var plr = player.userID;
                PLRStats.TryLoad(plr);
                if (cachedPlayerStats.ContainsKey(plr))
                {

                    if (player != null)
                    {
                        Puts($"Fetching {player.displayName}'s stats");
                        player.ChatMessage($"Stats for {player.displayName} \n\nKills › {cachedPlayerStats[player.userID].PlayerKills} \nDeaths › {cachedPlayerStats[player.userID].Deaths}\nKDR › {cachedPlayerStats[player.userID].KDR}\nBarrels Hit › {cachedPlayerStats[player.userID].BarrelsHit}\nStructures Placed › {cachedPlayerStats[player.userID].StructuresPlaced}\nBullets Fired › {cachedPlayerStats[player.userID].BulletsFired}\nHelicopters Killed › {cachedPlayerStats[target].HelicoptersKilled}\nStructures Upgraded › {cachedPlayerStats[target].StructuresUpgraded}\n\n<color=#828282>»</color> You can find other players stats by doing /stats (playerName).");
                    }
                    else return;
                }
                else Puts("Couldn't Load Players stuff"); return;
            }
        }
        void GetTopKills(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Kills\n\n");
            Dictionary<BasePlayer, int> TopKills = new Dictionary<BasePlayer, int>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var playerKills = stats.Value.PlayerKills;

                if (playerKills > 0)
                {
                    Puts("Kills over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopKills.ContainsKey(findPlayer))
                    {
                        TopKills.Add(findPlayer, playerKills);
                        Puts("Adding playing to top kills list");
                    }
                    foreach (KeyValuePair<BasePlayer, int> topFivePlayers in TopKills.OrderByDescending(x => x.Value).Take(5))
                    {
                       Puts("Getting all the top players stuff as a test"); // OrderByDescending(x => x.Value).Take(5);
                       PLRStats.TryLoad(topFivePlayers.Key.userID);

                       result.Append($"{topFivePlayers.Key.displayName} with {TopKills[findPlayer]} Kills");
                    }
                        SendMessage(player, result.ToString());
                        Puts("Sent message");
                    return;
                }
                
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of kills!");
                    return;
                }
            }
        }
        void GetTopDeaths(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Deaths\n\n");
            Dictionary<BasePlayer, int> TopDeaths = new Dictionary<BasePlayer, int>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var playerDeaths = stats.Value.Deaths;
                 
                if (playerDeaths > 0)
                {
                    Puts("Deaths over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopDeaths.ContainsKey(findPlayer))
                    {
                        TopDeaths.Add(findPlayer, playerDeaths);
                        Puts("Adding player to the top deaths list");
                    }
                     foreach (KeyValuePair<BasePlayer, int> topFivePlayer in TopDeaths.OrderByDescending(x => x.Value).Take(5))
                    {
                        Puts("Getting all the top players");
                        PLRStats.TryLoad(topFivePlayer.Key.userID);
    
                        result.Append($" {topFivePlayer.Key.displayName} with {TopDeaths[findPlayer]} Deaths");
                    }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of deaths!");
                    return;
                }
            }
        }
        void GetTopKDR(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player KDR\n\n");
            Dictionary<BasePlayer, float> TopKDR = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var playerKDR = stats.Value.KDR;
                if (playerKDR > 0f)
                {
                    Puts("KDR over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopKDR.ContainsKey(findPlayer))
                    {
                        TopKDR.Add(findPlayer, playerKDR);
                        Puts("Adding player to the top KDR list");
                    }
                    foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopKDR.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($" {topFivePlayer.Key.displayName} with {TopKDR[findPlayer]} KDR");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of KDR!");
                    return;
                }
            }
        }
        void GetTopBarrels(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Barrels Hit\n\n");
            Dictionary<BasePlayer, float> TopBarrels = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var barrels = stats.Value.BarrelsHit;
                if (barrels> 0)
                {
                    Puts("Barrels Hit over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopBarrels.ContainsKey(findPlayer))
                    {
                        TopBarrels.Add(findPlayer, barrels);
                        Puts("Adding player to the top Barrels list");
                    }
                    foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopBarrels.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($" {topFivePlayer.Key.displayName} with {TopBarrels[findPlayer]} Barrels Hit");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of Barrels Hit!");
                    return;
                }
            }
        }
        void GetTopStructures(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Structures Placed\n\n");
            Dictionary<BasePlayer, float> TopStructures = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var structures = stats.Value.BarrelsHit;
                if (structures > 0)
                {
                    Puts("Structures Placed over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopStructures.ContainsKey(findPlayer))
                    {
                        TopStructures.Add(findPlayer, structures);
                        Puts("Adding player to the top structures list");
                    }
                    foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopStructures.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($" {topFivePlayer.Key.displayName} with {TopStructures[findPlayer]} Structures Placed");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of Structures Placed!");
                    return;
                }
            }
        }
        void GetTopBulletsFired(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Bullets Fired\n\n");
            Dictionary<BasePlayer, float> TopBullets = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats) 
            {
                var bullets = stats.Value.BulletsFired;
                if (bullets > 0)
                {
                    Puts("Structures Placed over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopBullets.ContainsKey(findPlayer))
                    {
                        TopBullets.Add(findPlayer, bullets);
                        Puts("Adding player to the top bullets fired list");
                    }
                    foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopBullets.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($"{topFivePlayer.Key.displayName} with {TopBullets[findPlayer]} Bullets Fired");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of Bullets Fired!");
                    return;
                }
            }
        }
        void GetTopHeliKilled(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Patrol Helicopters Killed\n\n");
            Dictionary<BasePlayer, float> TopHeli = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var heli = stats.Value.HelicoptersKilled;
                if (heli > 0)
                {
                    Puts("Helicopters Killed over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopHeli.ContainsKey(findPlayer))
                    {
                        TopHeli.Add(findPlayer, heli);
                        Puts("Adding player to the top heli killed list");
                    }
                    foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopHeli.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($"{topFivePlayer.Key.displayName} with {TopHeli[findPlayer]} Helicopters Killed");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of Helicopters Killed!");
                    return;
                }
            }
        }
        void GetTopStructuresUpgraded(BasePlayer player)
        {
            StringBuilder result = new StringBuilder("Top Player Structures Upgraded\n\n");
            Dictionary<BasePlayer, float> TopStructures = new Dictionary<BasePlayer, float>();

            foreach (KeyValuePair<ulong, PLRStats> stats in cachedPlayerStats)
            {
                var structure = stats.Value.HelicoptersKilled;
                if (structure > 0)
                {
                    Puts("Helicopters Killed over 0");
                    BasePlayer findPlayer = BasePlayer.FindByID(stats.Value.PlayerID);

                    if (!TopStructures.ContainsKey(findPlayer))
                    {
                        TopStructures.Add(findPlayer, structure);
                        Puts("Adding player to the top bullets fired list");
                    }
                     foreach (KeyValuePair<BasePlayer, float> topFivePlayer in TopStructures.OrderByDescending(x => x.Value).Take(5))
                        {
                            Puts("Getting all the top players");
                            PLRStats.TryLoad(topFivePlayer.Key.userID);

                            result.Append($"{topFivePlayer.Key.displayName} with {TopStructures[findPlayer]} Structures Upgraded");
                        }
                    SendMessage(player, result.ToString());
                    Puts("Sent Message");
                    return;
                }
                else
                {
                    SendMessage(player, "Nobody on the server has top amount of Structures Upgraded!");
                    return;
                }
            }
        }
        void SendMessage(BasePlayer player, string message, params object[] args)
        {
            PrintToChat(player, message, args);
        }
        //Deaths & Kills
        object OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            BasePlayer killer = info?.Initiator as BasePlayer;
            Puts("Player DEADED");
            if (player != null)
            {
                if (player == killer) return null;
                Puts("player isn't null");
                if (player.IsNpc || killer.IsNpc) return null;
                 
                PLRStats.TryLoad(player.userID);
                if (cachedPlayerStats.ContainsKey(player.userID))
                {
                    cachedPlayerStats[player.userID].Deaths++;
                    cachedPlayerStats[player.userID].Save(player.userID);
                    Puts("Adding cached Stats");
                }
                else { PLRStats.TryLoad(player.userID); PLRStats.TryLoad(killer.userID); return null; } 
                if (cachedPlayerStats.ContainsKey(killer.userID))
                {
                    cachedPlayerStats[killer.userID].PlayerKills++;
                    cachedPlayerStats[player.userID].Save(player.userID);
                    Puts("Adding Cached Kills");
                }
                else return null;
            }
            return null;
        }

        void OnEntityBuilt(Planner plan, GameObject go)
        {
            var player = plan.GetOwnerPlayer();
            if (player != null)
            {
                if (cachedPlayerStats.ContainsKey(player.userID))
                {
                    cachedPlayerStats[player.userID].StructuresPlaced++;    
                    cachedPlayerStats[player.userID].Save(player.userID);
                }
                else return;
            }
            else return;
        }
        public List<ulong> GetAllPlayers()
        {
            List<ulong> PlayersID = new List<ulong>();
            BasePlayer.activePlayerList.ToList().ForEach(player => PlayersID.Add(ulong.Parse(player.UserIDString)));
            return PlayersID;
        }

        void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (player != null)
            {
                if (cachedPlayerStats.ContainsKey(player.userID))
                {
                    cachedPlayerStats[player.userID].BulletsFired++;
                    cachedPlayerStats[player.userID].Save(player.userID);
                }
                else return;
            }
        }


        void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (info != null)
            {
                if (entity.PrefabName == "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-1.prefab" || entity.PrefabName == "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-2.prefab" || entity.PrefabName == "assets/bundled/prefabs/radtown/loot_barrel_1.prefab" || entity.PrefabName == "assets/bundled/prefabs/radtown/loot_barrel_2.prefab" || entity.PrefabName == "assets/bundled/prefabs/radtown/oil_barrel.prefab")
                {
                    var player = entity.lastAttacker as BasePlayer;
                    if (player != null)
                    {
                        if (cachedPlayerStats.ContainsKey(player.userID))
                        {
                            cachedPlayerStats[player.userID].BarrelsHit++;
                            cachedPlayerStats[player.userID].Save(player.userID);
                        }
                        else return;
                    }
                    return;
                }
                else return;
            }
            return;
        }
        object OnStructureUpgrade(BaseCombatEntity entity, BasePlayer player, BuildingGrade.Enum grade)
        {
            if (player != null)
            {
                if (cachedPlayerStats.ContainsKey(player.userID))
                {
                    cachedPlayerStats[player.userID].StructuresUpgraded++;
                    cachedPlayerStats[player.userID].Save(player.userID);
                }
                else return null;
            }
            return null;
        }
        private bool IsBarrel(BaseCombatEntity entity)
        {
            if (entity.ShortPrefabName == "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-1.prefab" || entity.ShortPrefabName == "assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-2.prefab" || entity.ShortPrefabName == "assets/bundled/prefabs/radtown/loot_barrel_1.prefab" || entity.ShortPrefabName == "assets/bundled/prefabs/radtown/loot_barrel_2.prefab" || entity.ShortPrefabName == "assets/bundled/prefabs/radtown/oil_barrel.prefab")
            {
                return true;
            }
            return false;
        }
        object OnHelicopterKilled(CH47HelicopterAIController heli)
        {
            BasePlayer player = heli.lastAttacker as BasePlayer;

            if (player != null)
            {
                if (cachedPlayerStats.ContainsKey(player.userID))
                {
                    cachedPlayerStats[player.userID].HelicoptersKilled++;
                    cachedPlayerStats[player.userID].Save(player.userID);
                }
                else return null;
            }
            return null;
        }

        private void OnPlayerInit(BasePlayer player) => PLRStats.TryLoad(player.userID);

        private void OnServerShutDown() => Unload();

        private void Unload()
        {
            foreach (var data in cachedPlayerStats) data.Value.Save(data.Key);
        }
        private void OnServerInitialized()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList) OnPlayerInit(player);
            Puts("stats.wipe Command Initialized");
            Puts("stats Command Initialized");
        }

        #region players File

        private class PLRStats
        {
            public ulong PlayerID;
            public int PlayerKills = 0;
            public int Deaths = 0;
            public float KDR { get; set; }

            public int StructuresPlaced = 0;
            public int BulletsFired = 0;
            public int BarrelsHit = 0;
            public int StructuresUpgraded = 0;
            public int HelicoptersKilled = 0;

            public PLRStats()
            {

            }
            public PLRStats(BasePlayer player)
            {

            }
            public void ManageKDR(PLRStats stats)
            {
                stats.KDR =+ Deaths == 0 ? PlayerKills : (float)Math.Round(((float)PlayerKills) / Deaths, 2);
            }
            internal void Save(ulong id) => Interface.Oxide.DataFileSystem.WriteObject(($"PlayerStats/{id}"), this, true);
            internal static void TryLoad(ulong id)
            {
                if (cachedPlayerStats.ContainsKey(id)) return;

                PLRStats data = Interface.Oxide.DataFileSystem.ReadObject<PLRStats>($"PlayerStats/{id}");

                if (data == null) data = new PLRStats();

                cachedPlayerStats.Add(id, data);
                cachedPlayerStats[id].PlayerID = id;
            }
            internal static void Reset(ulong id)
            {
                PLRStats data = Interface.Oxide.DataFileSystem.ReadObject<PLRStats>($"PlayerStats/{id}");

                if (data == null) return;

                data = new PLRStats();
                data.Save(id);

            }
        }
        #endregion
        [ConsoleCommand("closeUI")]
        private void closeUI(ConsoleSystem.Arg consoleArg)
        {
            var element = UI.CreateOverlayContainer(BG, "0 0 0 0", "0.129 0.06", "0.871 0.979", true);
            BasePlayer player = consoleArg.Connection.player as BasePlayer;
            CuiHelper.DestroyUi(player, BG);
        }
        #region UIHelpers
        public class UI
        {
            public static CuiElementContainer CreateElementContainer(string panelName, string color, string aMin, string aMax, bool cursor = false)
            {
                var NewElement = new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = { Color = color },
                            RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                            CursorEnabled = cursor
                        },
                        new CuiElement().Parent,
                        panelName
                    }
                };
                return NewElement;
            }

            public static CuiElementContainer CreateOverlayContainer(string panelName, string color, string aMin, string aMax, bool cursor = false)
            {
                var NewElement = new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = { Color = color },
                            RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                            CursorEnabled = cursor
                        },
                        new CuiElement().Parent = "Overlay",
                        panelName
                    }
                };
                return NewElement;
            }

            public static void CreatePanel(ref CuiElementContainer container, string panel, string color, string aMin, string aMax, bool cursor = false)
            {
                container.Add(new CuiPanel
                {
                    Image = { Color = color },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor
                },
                    panel);
            }
            public static void CreateBlur(ref CuiElementContainer container, string panel, string color, string aMin, string aMax, bool cursor = false)
            {
                container.Add(new CuiPanel
                {
                    Image = { Color = color, Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat" },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor
                },
                    panel);
            }

            public static void CreateLabel(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiLabel
                {
                    Text = { Color = color, FontSize = size, Align = align, FadeIn = 1.0f, Text = text },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax }
                },
                    panel);
            }

            public static void CreateButton(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiButton
                {
                    Button = { Color = color, Command = command, FadeIn = 1.0f },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Text = { Text = text, FontSize = size, Align = align }
                },
                    panel);
            }

            public static void LoadImage(ref CuiElementContainer container, string panel, string img, string aMin, string aMax)
            {
                if (img.StartsWith("http") || img.StartsWith("www"))
                {
                    container.Add(new CuiElement
                    {
                        Parent = panel,
                        Components =
                        {
                            new CuiRawImageComponent { Url = img, Sprite = "assets/content/textures/generic/fulltransparent.tga" },
                            new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax }
                        }
                    });
                }
                else
                {
                    container.Add(new CuiElement
                    {
                        Parent = panel,
                        Components =
                        {
                            new CuiRawImageComponent { Png = img, Sprite = "assets/content/textures/generic/fulltransparent.tga" },
                            new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax }
                        }
                    });
                }
            }

            public static void CreateTextOutline(ref CuiElementContainer element, string panel, string colorText, string colorOutline, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                element.Add(new CuiElement
                {
                    Parent = panel,
                    Components =
                    {
                        new CuiTextComponent { Color = colorText, FontSize = size, Align = align, Text = text },
                        new CuiOutlineComponent { Distance = "1 1", Color = colorOutline },
                        new CuiRectTransformComponent { AnchorMax = aMax, AnchorMin = aMin }
                    }
                });
            }
        }
        #endregion
    }
}