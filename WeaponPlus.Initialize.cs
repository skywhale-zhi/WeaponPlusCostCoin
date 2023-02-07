using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace WeaponPlus
{
    public partial class WeaponPlus : TerrariaPlugin
    {
        private void OnReload(ReloadEventArgs e)
        {
            config = Config.LoadConfig();
        }

        private void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            if (wPlayers[args.Who] == null || !wPlayers[args.Who].isActive)
            {
                wPlayers[args.Who] = new WPlayer(TShock.Players[args.Who]);
                wPlayers[args.Who].hasItems = DB.ReadDBGetWItemsFromOwner(TShock.Players[args.Who].Name).ToList();

                foreach (var v in wPlayers[args.Who].hasItems)
                {
                    ReplaceWeaponsInBackpack(Main.player[args.Who], v);
                }
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;

            try
            {
                if (wPlayers[args.Who] != null)
                {
                    DB.WriteDB(wPlayers[args.Who].hasItems.ToArray());
                }
            }
            catch
            { }
        }


        /// <summary>
        /// 升级物品
        /// </summary>
        /// <param name="args"></param>
        private void PlusItem(CommandArgs args)
        {
            string tips1 = "几乎所有的武器和弹药都能强化，但是强化结果会无效化词缀，作为补偿，前三次强化价格降低 80%\n" +
                           "强化绑定一类武器，即同 ID 武器，而不是单独的一个物品。强化与人物绑定，不可分享，扔出即失效，只在背包，猪猪等个人私有库存内起效。\n" +
                           "当你不小心扔出或其他原因导致强化无效，请使用指令 /plus load 来重新获取。每次重新获取都会从当前背包中查找并强制拿出来重给，请注意捡取避免丢失。\n" +
                           "重新获取时重给的物品是单独给予，不会被其他玩家捡走，每次进入服务器时会强制重新获取。\n" +
                           "第一个物品栏是强化栏，指令只对该物品栏内的物品起效，强化完即可将武器拿走换至其他栏位，功能类似于哥布林的重铸槽。";
            string tips2 = "输入 /plus    查看当前该武器的等级状态\n" +
                           "输入 /plus load    将当前身上所有已升级的武器重新获取\n" +
                           "输入 /plus [damage/da/伤害] [up/down]    升级/降级当前武器的伤害等级\n" +
                           "输入 /plus [scale/sc/大小] [up/down]   升级/降级当前武器或射弹的体积等级 ±5%\n" +
                           "输入 /plus [knockback/kn/击退] [up/down]    升级/降级当前武器的击退等级 ±5%\n" +
                           "输入 /plus [usespeed/us/用速] [up/down]    升级/降级当前武器的使用速度等级\n" +
                           "输入 /plus [shootspeed/sh/飞速] [up/down]    升级/降级当前武器的射弹飞行速度等级，影响鞭类武器范围±5%\n" +
                           "输入 /plus clear    清理当前武器的所有等级，可以回收一点消耗物\n" +
                           "输入 /clearallplayersplus    将数据库中所有玩家的所有强化物品全部清理，管理员专属";

            // help
            if (args.Parameters.Count == 1 && args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (!args.Player.Active)
                    args.Player.SendInfoMessage($"{tips1}\n{tips2}");
                else
                {
                    args.Player.SendMessage(tips1, new Color(255, 128, 255));
                    args.Player.SendMessage(tips2, getRandColor());
                }
                return;
            }

            //判断玩家状态
            if (!args.Player.Active)
            {
                args.Player.SendInfoMessage("该指令必须在游戏内使用");
                return;
            }

            //wplayer当前玩家
            WPlayer wPlayer = wPlayers[args.Player.Index];
            //改玩家物品栏第一个物品
            Item firstItem = args.Player.TPlayer.inventory[0];
            //该玩家升级武器中是否含有第一个物品
            WItem? select = wPlayer.hasItems.Find(x => x.id == firstItem.netID);
            if (select == null)
                select = new WItem(firstItem.netID, args.Player.Name);

            //判断武器是否合理
            if (firstItem == null || firstItem.IsAir || TShock.Utils.GetItemById(firstItem.type).damage <= 0 || firstItem.accessory || firstItem.netID == 0)
            {
                if (!(args.Parameters.Count == 1 && args.Parameters[0].Equals("load", StringComparison.OrdinalIgnoreCase)))
                {
                    args.Player.SendInfoMessage("请在第一个物品栏内放入武器而不是其他什么东西或空");
                    return;
                }
            }

            //原 plus
            if (args.Parameters.Count == 0)
            {
                args.Player.SendMessage($"当前物品：{firstItem.Name}   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", getRandColor());
            }
            // load 和 clear
            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("load", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var v in wPlayer.hasItems)
                    {
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, v, 0);
                    }
                    args.Player.SendInfoMessage("您当前的升级武器已重新读取");
                }
                else if (args.Parameters[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    if (select.Level == 0)
                    {
                        args.Player.SendInfoMessage("当前武器没有任何等级，不用回炉重做");
                    }
                    else
                    {
                        string sss = cointostring(select.allCost / 2, out List<Item> coins);
                        foreach (var v in coins)
                        {
                            int num = Item.NewItem(new EntitySource_DebugCommand(), args.Player.TPlayer.Center, new Vector2(5, 5), v.type, v.stack, true, 0, true, false);
                            Main.item[num].playerIndexTheItemIsReservedFor = args.Player.Index;
                            args.Player.SendData(PacketTypes.ItemDrop, "", num, 1f, 0f, 0f, 0);
                            args.Player.SendData(PacketTypes.ItemOwner, null, num, 0f, 0f, 0f, 0);
                        }
                        wPlayer.hasItems.RemoveAll(x => x.id == firstItem.netID);
                        DB.DeleteDB(args.Player.Name, firstItem.netID);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 1);
                        select = null;
                        args.Player.SendMessage("完全重置成功！钱币回收：" + sss, new Color(0, 255, 0));
                    }
                }
                else
                    args.Player.SendInfoMessage("请输入 /plus help    --查看指令帮助");
            }
            // up 和 down
            else if (args.Parameters.Count == 2)
            {
                int update = 0;
                if (args.Parameters[1].Equals("up", StringComparison.OrdinalIgnoreCase))
                    update = 1;
                else if (args.Parameters[1].Equals("down", StringComparison.OrdinalIgnoreCase))
                    update = -1;

                if (args.Parameters[0].Equals("damage", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("da", StringComparison.OrdinalIgnoreCase) || args.Parameters[0] == "伤害")
                {
                    if (update == 1)
                    {
                        if (!Deduction(select, args.Player, PlusType.damage))
                            return;
                        select.damage_level++;
                        if (!wPlayer.hasItems.Exists(x => x.id == select.id))
                        {
                            wPlayer.hasItems.Add(select);
                        }
                        DB.WriteDB(select);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                        args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 升级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                    }
                    else if (update == -1)
                    {
                        if (select.damage_level == 0)
                            args.Player.SendInfoMessage("当前 0 级无需回置");
                        else
                        {
                            //判断扣钱操作
                            //TODO
                            select.damage_level--;
                            select.checkDB();
                            DB.WriteDB(select);
                            ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                            args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 降级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                        }
                    }
                }
                else if (args.Parameters[0].Equals("scale", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("sc", StringComparison.OrdinalIgnoreCase) || args.Parameters[0] == "大小")
                {
                    if (update == 1)
                    {
                        //判断扣钱操作
                        if (!Deduction(select, args.Player, PlusType.scale))
                            return;
                        select.scale_level++;
                        if (!wPlayer.hasItems.Exists(x => x.id == select.id))
                            wPlayer.hasItems.Add(select);
                        DB.WriteDB(select);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                        args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 升级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                    }
                    else if (update == -1)
                    {
                        if (select.scale_level == 0)
                            args.Player.SendInfoMessage("当前 0 级无需回置");
                        else
                        {
                            //判断扣钱操作
                            //TODO
                            select.scale_level--;
                            select.checkDB();
                            DB.WriteDB(select);
                            ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                            args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 降级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                        }
                    }
                }
                else if (args.Parameters[0].Equals("knockBack", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("kn", StringComparison.OrdinalIgnoreCase) || args.Parameters[0] == "击退")
                {
                    if (update == 1)
                    {
                        //判断扣钱操作
                        if (!Deduction(select, args.Player, PlusType.knockBack))
                            return;
                        select.knockBack_level++;
                        if (!wPlayer.hasItems.Exists(x => x.id == select.id))
                            wPlayer.hasItems.Add(select);
                        DB.WriteDB(select);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                        args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 升级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                    }
                    else if (update == -1)
                    {
                        if (select.knockBack_level == 0)
                            args.Player.SendInfoMessage("当前 0 级无需回置");
                        else
                        {
                            //判断扣钱操作
                            //TODO
                            select.knockBack_level--;
                            select.checkDB();
                            DB.WriteDB(select);
                            ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                            args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 降级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                        }
                    }
                }
                else if (args.Parameters[0].Equals("useSpeed", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("us", StringComparison.OrdinalIgnoreCase) || args.Parameters[0] == "用速")
                {
                    if (update == 1)
                    {
                        //判断扣钱操作
                        if (!Deduction(select, args.Player, PlusType.useSpeed))
                            return;
                        select.useSpeed_level++;
                        if (!wPlayer.hasItems.Exists(x => x.id == select.id))
                            wPlayer.hasItems.Add(select);
                        DB.WriteDB(select);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                        args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 升级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                    }
                    else if (update == -1)
                    {
                        if (select.useSpeed_level == 0)
                            args.Player.SendInfoMessage("当前 0 级无需回置");
                        else
                        {
                            //判断扣钱操作
                            //TODO
                            select.useSpeed_level--;
                            select.checkDB();
                            DB.WriteDB(select);
                            ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                            args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 降级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                        }
                    }
                }
                else if (args.Parameters[0].Equals("shootSpeed", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("sh", StringComparison.OrdinalIgnoreCase) || args.Parameters[0] == "飞速")
                {
                    if (update == 1)
                    {
                        //判断扣钱操作
                        if (!Deduction(select, args.Player, PlusType.shootSpeed))
                            return;
                        select.shootSpeed_level++;
                        if (!wPlayer.hasItems.Exists(x => x.id == select.id))
                            wPlayer.hasItems.Add(select);
                        DB.WriteDB(select);
                        ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                        args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 升级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                    }
                    else if (update == -1)
                    {
                        if (select.shootSpeed_level == 0)
                            args.Player.SendInfoMessage("当前 0 级无需回置");
                        else
                        {
                            //判断扣钱操作
                            //TODO
                            select.shootSpeed_level--;
                            select.checkDB();
                            DB.WriteDB(select);
                            ReplaceWeaponsInBackpack(args.Player.TPlayer, select, 0);
                            args.Player.SendMessage($"{Lang.GetItemNameValue(select.id)} 降级成功   共计消耗：{cointostring(select.allCost, out List<Item> temp)}\n{select.ItemMess()}", new Color(0, 255, 0));
                        }
                    }
                }
                else
                {
                    args.Player.SendInfoMessage("请输入 /plus help    --查看指令帮助");
                }
            }
            else
            {
                args.Player.SendInfoMessage("请输入 /plus help    --查看指令帮助");
            }
        }


        /// <summary>
        /// 清理数据库中的强化物品
        /// </summary>
        /// <param name="args"></param>
        private void ClearPlusItem(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                if (DB.DeleteDBAll())
                {
                    foreach (var p in wPlayers)
                    {
                        if (p != null && p.isActive)
                        {
                            foreach (var v in p.hasItems)
                            {
                                ReplaceWeaponsInBackpack(p.me!.TPlayer, v, 1);
                            }
                            p.hasItems.Clear();
                        }
                    }
                    args.Player.SendSuccessMessage("所有玩家的所有强化数据全部清理成功！");
                }
                else
                    args.Player.SendErrorMessage("所有玩家的所有强化数据清理失败！！");
            }
            else
                args.Player.SendInfoMessage("输入 /clearallplayersplus   将数据库中强化物品全部清理");
        }
    }
}
