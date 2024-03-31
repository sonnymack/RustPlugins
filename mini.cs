using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("mini", "Sonnymack", 3.0)]
    [Description("Allows players with permission to spawn in a minicopter")]
    public class mini : CovalencePlugin
    {
        #region Variables
        private const string m = "assets/content/vehicles/minicopter/minicopter.entity.prefab";
        private const int time = 3600;

        private Dictionary<string, DateTime> spawnTimes = new Dictionary<string, DateTime>();
        #endregion

        #region Configuration
         ConfigData configData;
        class ConfigData
        {
            [JsonProperty(PropertyName = "Fuel to spawn in minicopter (Default = 50)")]
            public int fuelAmount = 50;
            [JsonProperty(PropertyName = "Fuel to spawn in minicopter (Default = 100)")]
            public int vipFuelAmount = 100;
            [JsonProperty(PropertyName = "mini.use Cooldown timer (default 1000)")]
            public int vipCooldown = 1000;
            [JsonProperty(PropertyName = "minivip.use Cooldown timer (default 500)")]
            public int vipPlusCooldown = 500;
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
        void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion

        #region lang

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CommandCooldown"] = "You must wait <color=red>{0}</color> to use this command again!",
                ["NoPerm"] = "You don't have permission to spawn in a minicopter!",
                ["Success"] = "A minicopter has been spawned for you!",
            }, this);
        }
        #endregion

        #region Hooks
        void Init()
        {
            permission.RegisterPermission("mini.use", this);
            permission.RegisterPermission("minivip.use", this);
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
        #endregion

        #region Main
        [Command("mini")]
        private void miniCommand(IPlayer player, string Command, string[] args)
        {
            if (spawnTimes.ContainsKey(player.Id))
            {
                var lastTime = spawnTimes[player.Id];
                var secondsSinceMini = (DateTime.Now - lastTime).Seconds; 
               if (secondsSinceMini < configData.vipPlusCooldown && player.HasPermission("mini.use") && player.HasPermission("minivip.use"))
                {
                    player.Reply(String.Format(lang.GetMessage("CommandCooldown", this, player.Id), configData.vipPlusCooldown - secondsSinceMini));
                    return;
                }
                else if (secondsSinceMini < configData.vipPlusCooldown && player.HasPermission("minivip.use"))
                {
                    player.Reply(String.Format(lang.GetMessage("CommandCooldown", this, player.Id), configData.vipPlusCooldown - secondsSinceMini));

                    return;
                }
                else if (secondsSinceMini < configData.vipCooldown && player.HasPermission("mini.use"))
                {
                    player.Reply(String.Format(lang.GetMessage("CommandCooldown", this, player.Id), configData.vipCooldown - secondsSinceMini));
                    
                    return;
                }
            }
            var spawnPos = new Vector3(player.Position().X + 1.5f, player.Position().Y, player.Position().Z);
            Minicopter ent = GameManager.server.CreateEntity(m, spawnPos) as Minicopter;

            ent.Spawn();

            if (player.HasPermission("mini.use") & !player.HasPermission("minivip.use"))
            {
                int fuel = configData.fuelAmount;
                StorageContainer fuelContainer = ent.GetFuelSystem().GetFuelContainer();
                fuelContainer.inventory.AddItem(fuelContainer.allowedItem, fuel);

                spawnTimes[player.Id] = DateTime.Now;
                player.Reply(String.Format(lang.GetMessage("Success", this, player.Id)));
                return;
            }
            else if (player.HasPermission("minivip.use") && player.HasPermission("mini.use"))
            {
                int fuel = configData.vipFuelAmount;
                StorageContainer fuelContainer = ent.GetFuelSystem().GetFuelContainer();
                fuelContainer.inventory.AddItem(fuelContainer.allowedItem, fuel);

                spawnTimes[player.Id] = DateTime.Now;
                player.Reply(String.Format(lang.GetMessage("Success", this, player.Id)));
                return;
            }
            else
            {
                player.Reply(String.Format(lang.GetMessage("NoPerm", this, player.Id)));
            }
        }
        #endregion
    }
}
