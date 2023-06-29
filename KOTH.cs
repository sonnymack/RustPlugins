using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Koth", "Sonnymack", 2.0)]
    public class KOTH : RustPlugin
    {
        string zoneID = "1234";

        [PluginReference]
        private readonly Plugin ZoneManager;
        private string PanelBackground = "PanelBackground";

        public static RelationshipManager.PlayerTeam rteam = RelationshipManager.ServerInstance.CreateTeam();
        public static RelationshipManager.PlayerTeam bteam = RelationshipManager.ServerInstance.CreateTeam();

        public ulong rteamID = rteam.teamID;
        public ulong bteamID = bteam.teamID;

        public Vector3 blueSpawn;
        public Vector3 redSpawn;
        public Vector3 Spawn;
        
        public string redTeamS = "RedTeam";
        public string blueTeamS = "BlueTeam";


        public bool gameRunning = false;
        public bool closedGUI = false;

        private Storedplayers players; //Get players from players file
        private KothPoints points;
        void Init()
        {
            Puts("Plugin Loaded!");
            players = Interface.Oxide.DataFileSystem.ReadObject<Storedplayers>("KothPlayers");
            points = Interface.Oxide.DataFileSystem.ReadObject<KothPoints>("KothPoints");
            if (permission.GroupExists(blueTeamS) && (permission.GroupExists(redTeamS)))
            {
                return;
            }
            else
            {
                if (!permission.GroupExists(redTeamS))
                {
                    permission.CreateGroup(redTeamS, redTeamS, 1);
                }

                if (!permission.GroupExists(blueTeamS))
                {
                    permission.CreateGroup(blueTeamS, blueTeamS, 1);
                }
            }
        }
        [ChatCommand("startKoth")]
        private void startKoth(BasePlayer player) 
        {
            if (gameRunning == false)
            {
                bteam.teamName = "BlueGroup";

                rteam.teamName = "RedGroup";

                Puts("Game Starting!");
                PickTeam(player);
                Puts("TeamPicked");
                TeleportPlayer(player);
                Puts("Teleported Player!");
                gameRunning = true;

                bool IsInZone = (bool)ZoneManager.Call("IsPlayerInZone", zoneID, player);

                var bPoints = points.BlueTeamPoints;
                var rPoints = points.RedTeamPoints;

                Timer mytimer = timer.Repeat(5f, 0, () =>
                {
                    Puts("Started Time");
                    while (IsInZone == true)
                    {
                        if (permission.UserHasGroup(player.UserIDString, blueTeamS))
                        {
                            bPoints =+ 1;

                            Interface.Oxide.DataFileSystem.WriteObject("KOTHPoints", players); //might not wrk
                        }
                        else if (permission.UserHasGroup(player.UserIDString, redTeamS))
                        {
                            rPoints =+ 1;
                            Interface.Oxide.DataFileSystem.WriteObject("KOTHPoints", players);
                        }
                        else return;
                    }
                });
                if (IsInZone == false) timer.Destroy(ref mytimer);

                while (gameRunning == true)
                {
                    ManageScores(player);
                }
            }
            else SendMessage(player, "Game is running already!");
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            //player.Teleport(Spawn);
            timer.Once(2f, () =>
            {
                showGui(player);
                if (gameRunning == true)
                {
                    PickTeam(player);
                }
                else return;
            });
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {

            if (TeamExists(bteamID) && TeamExists(rteamID))
            {
                var pID = player.userID;
                bteam.RemovePlayer(pID);
                rteam.RemovePlayer(pID);
            }
            else return;
            if (players.BluePlayers.ContainsKey(player.displayName)) // Check oxide group
            {
                players.BluePlayers.Remove(player.displayName);
            }
            else return;

            if (players.RedPlayers.ContainsKey(player.displayName))
            {
                players.RedPlayers.Remove(player.displayName);
            }
            else return;
            player.Hurt(150);
        }

        [ChatCommand("play")]
        private void PickTeam(BasePlayer player)
        {
            foreach (var p in BasePlayer.activePlayerList)
            {
                Puts("Foreach");
                int roll = UnityEngine.Random.Range(1, 2);
                if (roll == 1)
                {
                    AddPlayer(player, "RedTeam");
                    Puts("Red Team");
                }
                if (roll == 2)
                {
                    AddPlayer(player, "BlueTeam");
                    Puts("Blue Team");
                }
                else return;
            }
        }

        #region Helpers
        private void AddPlayer(BasePlayer player, string Group)
        {

            if (Group == "BlueTeam")
            {
                Puts("BlueTest");
                if (TeamExists(bteamID))
                {
                    Puts("Team Exists");
                    var bteam = RelationshipManager.ServerInstance.FindTeam(bteamID);

                    var pID = player.UserIDString;

                    permission.AddUserGroup(pID, blueTeamS);
                    if (bteam == null) return;
                    bteam.AddPlayer(player);

                    players.BluePlayers.Add(player.displayName, player.userID);
                    Interface.Oxide.DataFileSystem.WriteObject("KOTHPlayers", players);
                }
                else return;
            }
            if (Group == "RedTeam")
            {
                Puts("RedTest");
                if (TeamExists(rteamID))
                {
                    Puts("Team Exists");
                    var rteam = RelationshipManager.ServerInstance.FindTeam(rteamID);

                    var pID = player.UserIDString;

                    permission.AddUserGroup(pID, redTeamS);
                    if (rteam == null) return;
                    rteam.AddPlayer(player);

                    players.RedPlayers.Add(player.displayName, player.userID);
                }
                else return;
            }
            else return;
        }   

        private bool TeamExists(ulong groupID)
        {
            var group = RelationshipManager.ServerInstance.FindTeam(groupID);
           // if (group == null) return false;
            if (group.teamID == rteamID && group != null)
            {
                Puts("Found Red Group");
                return true;
            }
            else if (group.teamID == bteamID && group != null)
            {
                Puts("Found Blue Group");
                return true;
            }
            else return false;
        }
        #endregion
        private void TeleportPlayer(BasePlayer player)
        {
            if (players.BluePlayers.ContainsValue(player.userID))
            {
                player.Teleport(blueSpawn);
            }
            else if (players.RedPlayers.ContainsValue(player.userID))
            {
                player.Teleport(redSpawn);
                
            }
            else return;
        }
        bool BlueWinning()
        {
            var bPoints = points.BlueTeamPoints;
            var rPoints = points.RedTeamPoints;
            if (bPoints == 100 && bPoints > rPoints)
            {
                return true;
            }
            return false;
        }
        bool RedWinning()
        {
            var bPoints = points.BlueTeamPoints;
            var rPoints = points.RedTeamPoints;
            if (rPoints == 100 && rPoints > bPoints)
            {
                return true;
            }
            return false;
        }

        private void ManageScores(BasePlayer player)
        {
            bool bluewin = BlueWinning();
            bool redwin = RedWinning();     
            if (bluewin == true)
            {
                EndKoth(player);    
                return;
            }
            else if (redwin == true)
            {
                EndKoth(player);
                return;
            }
            else return;
        }

        private void EndKoth(BasePlayer player)
        {
            Server.Broadcast("Ending the KOTH game!");
            if (TeamExists(bteamID))
            {
                var bteam = RelationshipManager.ServerInstance.FindTeam(bteamID);
                bteam.Disband();
            }
            if (TeamExists(rteamID))
            {
                var rteam = RelationshipManager.ServerInstance.FindTeam(rteamID);
                rteam.Disband();
            }
            else return;
            Puts("Game Ending!");
            foreach (var p in BasePlayer.activePlayerList) // do only for players in side the game zone
            {
                p.Teleport(Spawn);
                Server.Broadcast("KOTH Ended!");
                if (BlueWinning() == true)
                {
                    Server.Broadcast("Blue team won!");
                }
                if (RedWinning() == true)
                {
                    Server.Broadcast("Red team won!");
                }
                else Puts("no team won!");

                foreach (var i in p.inventory.AllItems())
                {
                    i.Remove();
                }
                timer.Once(100f, () =>
                {
                    startKoth(player);
                    showLeaderboard(player);
                });
                gameRunning = false;
            }
        }

        #region GuiStartMenu
        [ChatCommand("joinGUI")]
        private void showGui(BasePlayer player)
        {
            CuiHelper.AddUi(player, PanelBackground);
            var element = UI.CreateOverlayContainer(PanelBackground, "0 0 0 0.76", "0.205 0.178", "0.791 0.872", true);
            UI.CreateBlur(ref element, PanelBackground, "0 0 0 0.5", "0 0", "1 1");
            UI.CreateLabel(ref element, PanelBackground, "1 0.88 1 1", "KOTH : 1.1", 30, "0.448 0.785", "0.566 0.832");
            UI.CreateLabel(ref element, PanelBackground, "1 1 1 1", "Welcome to King Of The Hill! This is in Early Access! Please report all bugs to:", 20, "0.242 0.601", "0.784 0.633");
            UI.CreateLabel(ref element, PanelBackground, "1 1 1 1", "<color=#03b6fc>https://discord.gg/Sw5uV84bRNM</color>", 20, "0.388 0.508", "0.623 0.54");

            UI.CreateButton(ref element, PanelBackground, "0.88 0.88 0.88 1", "Join Game", 20, "0.453 0.221", "0.531 0.297", "closeGUI");

        }

        [ConsoleCommand("closeGUI")]
        private void consoleUI_ForceClose(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Connection.player as BasePlayer;
            HideGui(player);
            closedGUI = true;
            startKoth(player);

        }
        private void HideGui(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, PanelBackground);
        }
        #endregion

        #region leaderboard
        private void showLeaderboard(BasePlayer player)
        {
            int WiningPoints = 1321; // Connect to players File
            CuiHelper.AddUi(player, PanelBackground);
            var element = UI.CreateOverlayContainer(PanelBackground, "0 0 0 0.76", "0.205 0.178", "0.791 0.872", true);
            UI.CreateBlur(ref element, PanelBackground, "0 0 0 0.5", "0 0", "1 1");
            UI.CreateLabel(ref element, PanelBackground, "1 0.88 1 1", "Game Ended", 30, "0.448 0.785", "0.566 0.832");
            UI.CreateLabel(ref element, PanelBackground, "1 1 1 1", $"Game Ended! {WiningPoints} Team won!", 20, "0.242 0.601", "0.784 0.633");
            UI.CreateLabel(ref element, PanelBackground, "1 1 1 1", "Join our Discord: <color=#03b6fc>https://discord.gg/Sw5uV84bRNM</color>", 20, "0.388 0.508", "0.623 0.54");
        }
        private void closeLeaderboard(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, PanelBackground);
        }
        #endregion

        #region GuiHelpers
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

        #region players File
        private class Storedplayers
        {
            public Dictionary<string, ulong> BluePlayers = new Dictionary<string, ulong>();
            public Dictionary<string, ulong> RedPlayers = new Dictionary<string, ulong >();

            public Storedplayers()
            {
            }   
        }

        private class KothPoints
        {
            public int BlueTeamPoints;
            public int RedTeamPoints;

            public int OverallPoints;
            public KothPoints()
            {
            }
            public KothPoints(BasePlayer player)
            {
            }
        }
        #endregion
        private void SendMessage(BasePlayer player, string message)
        {
            player.SendMessage(message);
        }
    }
}
