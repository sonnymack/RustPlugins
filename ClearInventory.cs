using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("ClearInventory", "sonnymack", "1.0.0")]
    [Description("Allows players to clear their inventory.")]
    class ClearInventory : RustPlugin
    {
        private void Init()
        {
            permission.RegisterPermission("clearinv.use", this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "You don't have permission to use this command.",
                ["InventoryCleared"] = "Your inventory has been cleared."
            }, this);
        }

        [ChatCommand("clear")]
        private void ClearCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, "clearinv.use"))
            {
                SendReply(player, Lang("NoPermission", player.UserIDString));
                return;
            }

            player.inventory.Strip();
            SendReply(player, Lang("InventoryCleared", player.UserIDString));
        }

        private string Lang(string key, string userId) => lang.GetMessage(key, this, userId);
    }
}
