using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Repair", "Sonnymack", 0.1)]
    public class repair : RustPlugin
    {
        #region Config
        private ConfigData configData;
        class ConfigData
        {
            [JsonProperty(PropertyName = "Cooldown for default (In seconds)")]
            public int defcooldown = 1800;
            [JsonProperty(PropertyName = "Cooldown for vip (In seconds)")]
            public int vipcooldown = 1000;
        }

        private bool LoadConfigVariables()
        {
            try
            {
                configData = Config.ReadObject<ConfigData>();
            }
            catch
            {
                return false;
            }
            SaveConfig(configData);
            return true;
        }

        void Init()
        {
            permission.RegisterPermission("repairs.use", this);
            permission.RegisterPermission("viprepairs.use", this);
            
            if (!LoadConfigVariables())
            {
                PrintError("An issue with the config file has been detected Please delete file, or check syntax and fix.");
                return;
            }
        }

        protected override void LoadDefaultConfig()
        {
            Puts("Generating new config file.");
            SaveConfig(configData);
            configData = new ConfigData();
        }

        void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion Config
        #region lang

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CommandCooldown"] = "You must wait <color=red>{0}</color> to use this command again!",
                ["NotHolding"] = "You must be holding an item!",
                ["Success"] = "You have repaired your current item!",
                ["NoPerm"] = "You don't have permission to repair your item!",
                ["CantRepair"] = "You must use an item that needs repairing!"
            }, this);
        }
        #endregion

        private Dictionary<string, DateTime> repairTimes = new Dictionary<string, DateTime>();
        private string CreateMessage(string langName, params object[] args)
        {
            if (args != null)
            {
                return string.Format(lang.GetMessage(langName, this), args);
            }
            else return lang.GetMessage(langName, this);
        }
        [ChatCommand("repair")]
        private void repairCMD(BasePlayer player, string Command, string[] args)
        {
            if (permission.UserHasPermission(player.UserIDString, "repairs.use") &! permission.UserHasPermission(player.UserIDString, "viprepairs.use"))
            {
                if (repairTimes.ContainsKey(player.UserIDString))
                {
                    var lastCommandUse = repairTimes[player.UserIDString];
                    var seconds = (DateTime.Now - lastCommandUse).Seconds;

                    if (seconds < configData.defcooldown)   
                    {
                        var time = configData.defcooldown - seconds;
                        Console.WriteLine($"{time} remaining");
                        player.ChatMessage(String.Format(lang.GetMessage("CommandCooldown", this, player.UserIDString), time));
                        return;
                    }
                }
                HeldEntity heldEntity = player.GetHeldEntity();
                if (heldEntity == null)
                {
                    player.ChatMessage(CreateMessage("NotHolding"));
                    return;
                }
                if (heldEntity != null && player.GetActiveItem().condition != heldEntity.MaxHealth())
                {
                    float cond = player.GetActiveItem().maxCondition;
                    repairTimes[player.UserIDString] = DateTime.Now;
                    player.GetActiveItem().condition = cond;
                    player.ChatMessage(CreateMessage("Success"));
                    return;
                }
                else
                {
                    player.ChatMessage(CreateMessage("CantRepair")); return;
                }
            }
            else if (permission.UserHasPermission(player.UserIDString, "viprepairs.use") & !permission.UserHasPermission(player.UserIDString, "repairs.use"))
            {
                if (repairTimes.ContainsKey(player.UserIDString))
                {
                    var lastCommandUse = repairTimes[player.UserIDString];
                    var seconds = (DateTime.Now - lastCommandUse).Seconds;

                    if (seconds < configData.vipcooldown)
                    {

                        var time = configData.vipcooldown - seconds;
                        Console.WriteLine($"{time} remaining");
                        string cooldown = string.Format(lang.GetMessage("CommandCooldown", this, player.UserIDString), time);
                        player.ChatMessage(String.Format(lang.GetMessage("CommandCooldown", this, player.UserIDString), time));
                        return;
                    }
                }
                HeldEntity heldEntity = player.GetHeldEntity();
                if (heldEntity == null)
                {
                    player.ChatMessage(CreateMessage("NotHolding"));
                    return;
                }
                if (heldEntity != null && player.GetActiveItem().condition != heldEntity.MaxHealth())
                {
                    float cond = player.GetActiveItem().maxCondition;
                    repairTimes[player.UserIDString] = DateTime.Now;
                    player.GetActiveItem().condition = cond;
                    player.ChatMessage(CreateMessage("Success"));
                    return;
                }
                else
                {
                    player.ChatMessage(CreateMessage("CantRepair")); return;
                }
            }
            else if (permission.UserHasPermission(player.UserIDString, "viprepairs.use") && permission.UserHasPermission(player.UserIDString, "repairs.use"))
            {
                if (repairTimes.ContainsKey(player.UserIDString))
                {
                    var lastCommandUse = repairTimes[player.UserIDString];
                    var seconds = (DateTime.Now - lastCommandUse).Seconds;

                    if (seconds < configData.vipcooldown)
                    {
                        var time = configData.vipcooldown - seconds;
                        Console.WriteLine($"{time} remaining");
                        string cooldown = string.Format(lang.GetMessage("CommandCooldown", this, player.UserIDString), time);
                        player.ChatMessage(String.Format(lang.GetMessage("CommandCooldown", this, player.UserIDString), time));
                        return;
                    }
                }
                HeldEntity heldEntity = player.GetHeldEntity();
                if (heldEntity == null)
                {
                    player.ChatMessage(CreateMessage("CommandCooldown"));
                    return;
                }
                if (heldEntity != null && player.GetActiveItem().condition != heldEntity.MaxHealth())
                {
                    float cond = player.GetActiveItem().maxCondition;
                    repairTimes[player.UserIDString] = DateTime.Now;
                    player.GetActiveItem().condition = cond;
                    player.ChatMessage(CreateMessage("Success"));
                    return;
                }
                else
                {
                    player.ChatMessage(CreateMessage("CantRepair")); return;
                }
            }
            else
            {
                player.ChatMessage(CreateMessage("NoPerm"));
                return;
            }
        }
    }
}
