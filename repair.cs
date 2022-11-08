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
            [JsonProperty(PropertyName = "Cooldown (In seconds)")]
            public int cooldown = 1800;
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
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CommandCooldown"] = $"You must wait <color=red>{0} - {1}</color> to use this command again!",
                ["NotHolding"] = "You must be holding an item!",
                ["Success"] = "You have repaired your current item!",
                ["CantRepair"]= "You must use an item that needs repairing!"
            }, this);
        }
        #endregion

        private Dictionary<string, DateTime> repairTimes = new Dictionary<string, DateTime>();
        private string CreateMessage(string langName, params object[] args)
        {
            if (args == null)
                return lang.GetMessage(langName, this);
            return string.Format(lang.GetMessage(langName, this), args);
        }
        [ChatCommand("repair"), Permission("repair.use")]
        private void repairCMD(BasePlayer player, string Command, string[] args)
        {
            if (repairTimes.ContainsKey(player.UserIDString))
            {
                var lastCommandUse = repairTimes[player.UserIDString];
                var seconds = (DateTime.Now - lastCommandUse).Seconds;

                if (seconds < configData.cooldown)
                {
                    CreateMessage("CommandCooldown", configData.cooldown - seconds);
                    return;
                }
            }
            HeldEntity heldEntity = player.GetHeldEntity();
            if (heldEntity == null)
            {
                CreateMessage("CommandCooldown");
                return;
            }
            if (heldEntity != null && player.GetActiveItem().condition != heldEntity.MaxHealth())
            {
                float cond = player.GetActiveItem().maxCondition;
                repairTimes[player.UserIDString] = DateTime.Now;
                player.GetActiveItem().condition = cond;
                CreateMessage("Success"); 
                return;
            }
            else
            {
                CreateMessage("CantRepair");
            }
        }
    }
}
