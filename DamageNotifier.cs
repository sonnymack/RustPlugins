using System.Text;
using System.Collections.Generic;
using System.Linq;
namespace Oxide.Plugins
{
    [Info("Damage Notifier", "Sonnymack", 0.1)]
    public class DamageNotifier : RustPlugin
    {

        public Dictionary<BasePlayer, int> HeliHits = new Dictionary<BasePlayer, int>();
        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {

            BasePlayer player = info.InitiatorPlayer;
            ItemDefinition itemDefinition = info.Weapon?.GetEntity()?.GetComponent<BaseProjectile>()?.primaryMagazine.ammoType;
            if (entity.PrefabName == "patrolhelicopter" || entity.PrefabName == "patrol_helicopter" || entity.ShortPrefabName == "patrol_helicopter" || entity.ShortPrefabName == "patrolhelicopter" || entity.ShortPrefabName == "bradleyapc" || entity.PrefabName == "bradleyapc")
            {
                if (player != null && itemDefinition != null)
                {

                    if (itemDefinition.shortname != "ammo.rifle")
                    {
                        return;
                    }


                    int damages = (int)info.damageTypes.Total();
                    if (!HeliHits.ContainsKey(info.InitiatorPlayer))
                    {
                        HeliHits.Add(info.InitiatorPlayer, damages);
                    }
                    else
                    {
                        HeliHits[info.InitiatorPlayer] += damages;
                    }
                }
            }

        }
    
        [ChatCommand("test")]
        private void testCommand(BasePlayer player)
        {
            if (player.IsAdmin == false) return; 
            Server.Broadcast($"Helicopter Takedown <color=#c7c7c7>›</color>\n\n<color=#c7c7c7><size=15>•</size></color><color=#fcd303> {player.displayName}</color> : <color=#fcd303>321321312</color> \n");
            player.ChatMessage($"This is the HeliHits damages! {HeliHits[player]}");
        }
        void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity.PrefabName == "patrolhelicopter" || entity.PrefabName == "patrol_helicopter" || entity.ShortPrefabName == "patrol_helicopter" || entity.ShortPrefabName == "patrolhelicopter")
            {
                if (info == null) { Puts("null"); return; }
                StringBuilder result = new StringBuilder("Helicopter Takedown <color=#c7c7c7>›</color>\n\n");
                Puts("Death");
                foreach (KeyValuePair<BasePlayer, int> hitInfo in HeliHits.OrderByDescending(x => x.Value).Take(5)) // Get players with their score ordered from the best to the worst
                {
                    BasePlayer player = hitInfo.Key;
                    int DamageDone = hitInfo.Value;
                   
                    result.Append($"<color=#c7c7c7><size=15>•</size></color><color=#fcd303> {player.displayName}</color> : <color=#fcd303>{DamageDone}</color> \n");
                }
                Server.Broadcast(result.ToString());
                foreach(KeyValuePair<BasePlayer, int > Players in HeliHits)
                {
                    Puts("Removing all players");
                    BasePlayer plr = Players.Key;
                    Puts($"Removing {plr.displayName}");
                    HeliHits.Remove(plr);
                    Puts("Removed player");
                }
            }
        }
    }
}