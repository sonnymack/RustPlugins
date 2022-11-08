using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("mini", "Sonnymack", 1)]
    [Description("Command that spawns a minicopter in")]
    public class mini : CovalencePlugin
    {
        private const string m = "assets/content/vehicles/minicopter/minicopter.entity.prefab";
        private const int time = 3600;
        private Dictionary<string, DateTime> spawnTimes = new Dictionary<string, DateTime>();

        [Command("mini"), Permission("mini.use")]
        private void miniCommand(IPlayer player, string Command, string[] args)
        {
            if (spawnTimes.ContainsKey(player.Id))
            {
                var lastTiME = spawnTimes[player.Id];
                var SecondsSinceGift = (DateTime.Now - lastTiME).Seconds;
                if (SecondsSinceGift < time)
                {
                    player.Reply($"You need to wait {time - SecondsSinceGift} more seconds.");
                    return;
                }
            }
            var spawnPos = new Vector3(player.Position().X + 1.5f, player.Position().Y, player.Position().Z);
            var ent = GameManager.server.CreateEntity(m, spawnPos);

            ent.Spawn();
            spawnTimes[player.Id] = DateTime.Now;
            player.Reply("Minicopter has been spawned for you!");
        }
    }
}
