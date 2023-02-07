using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.UI;
using Terraria.Utilities;
using TerrariaApi.Server;
using TShockAPI;

namespace WeaponPlus
{
    public partial class WeaponPlus : TerrariaPlugin
    {
        /// <summary>
        /// 提供不含同步的生成物品方法
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pos"></param>
        /// <param name="randomBox"></param>
        /// <param name="Type"></param>
        /// <param name="Stack"></param>
        /// <param name="noBroadcast"></param>
        /// <param name="prefixGiven"></param>
        /// <param name="noGrabDelay"></param>
        /// <param name="reverseLookup"></param>
        /// <returns></returns>
        public static int MyNewItem(IEntitySource source, Vector2 pos, Vector2 randomBox, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
        {
            return MyNewItem(source, (int)pos.X, (int)pos.Y, (int)randomBox.X, (int)randomBox.Y, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
        }


        /// <summary>
        /// 提供不含同步的生成物品方法
        /// </summary>
        /// <param name="source"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Type"></param>
        /// <param name="Stack"></param>
        /// <param name="noBroadcast"></param>
        /// <param name="pfix"></param>
        /// <param name="noGrabDelay"></param>
        /// <param name="reverseLookup"></param>
        /// <returns></returns>
        public static int MyNewItem(IEntitySource source, int X, int Y, int Width, int Height, int Type, int Stack = 1, bool noBroadcast = false, int pfix = 0, bool noGrabDelay = false, bool reverseLookup = false)
        {
            if (WorldGen.gen)
            {
                return 0;
            }
            if (Main.rand == null)
            {
                Main.rand = new UnifiedRandom();
            }
            if (Main.tenthAnniversaryWorld)
            {
                if (Type == 58)
                {
                    Type = Main.rand.NextFromList(new short[3] { 1734, 1867, 58 });
                }
                if (Type == 184)
                {
                    Type = Main.rand.NextFromList(new short[3] { 1735, 1868, 184 });
                }
            }
            if (Main.halloween)
            {
                if (Type == 58)
                {
                    Type = 1734;
                }

                if (Type == 184)
                {
                    Type = 1735;
                }
            }
            if (Main.xMas)
            {
                if (Type == 58)
                {
                    Type = 1867;
                }

                if (Type == 184)
                {
                    Type = 1868;
                }
            }
            if (Type > 0 && Item.cachedItemSpawnsByType[Type] != -1)
            {
                Item.cachedItemSpawnsByType[Type] += Stack;
                return 400;
            }
            Main.item[400] = new Item();
            int num = 400;
            if (Main.netMode != 1)
            {
                num = Item.PickAnItemSlotToSpawnItemOn(reverseLookup, num);
            }
            Main.timeItemSlotCannotBeReusedFor[num] = 0;
            Main.item[num] = new Item();
            Item item = Main.item[num];
            item.SetDefaults(Type);
            item.Prefix(pfix);
            item.stack = Stack;
            item.position.X = X + Width / 2 - item.width / 2;
            item.position.Y = Y + Height / 2 - item.height / 2;
            item.wet = Collision.WetCollision(item.position, item.width, item.height);
            item.velocity.X = (float)Main.rand.Next(-30, 31) * 0.1f;
            item.velocity.Y = (float)Main.rand.Next(-40, -15) * 0.1f;
            if (Type == 859 || Type == 4743)
            {
                item.velocity *= 0f;
            }
            if (Type == 520 || Type == 521 || (item.type >= 0 && ItemID.Sets.NebulaPickup[item.type]))
            {
                item.velocity.X = (float)Main.rand.Next(-30, 31) * 0.1f;
                item.velocity.Y = (float)Main.rand.Next(-30, 31) * 0.1f;
            }
            item.active = true;
            item.timeSinceItemSpawned = ItemID.Sets.OverflowProtectionTimeOffset[item.type];
            Item.numberOfNewItems++;
            if (ItemSlot.Options.HighlightNewItems && item.type >= 0 && !ItemID.Sets.NeverAppearsAsNewInInventory[item.type])
            {
                item.newAndShiny = true;
            }
            else if (Main.netMode == 0)
            {
                item.playerIndexTheItemIsReservedFor = Main.myPlayer;
            }
            return num;
        }


        /// <summary>
        /// 将玩家背包里的部分东西和items的进行置换
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="model">0:将原物品转换成plus物品，1:将plus物品转换成原物品</param>
        public static void ReplaceWeaponsInBackpack(Player? player, WItem? item, int model = 0)
        {
            if (player == null || !player.active || item == null)
                return;
            int who = player.whoAmI;
            int stack;
            byte prefix;
            for (int i = 0; i < NetItem.InventoryIndex.Item2; i++)
            {
                if (player.inventory[i].netID == item.id)
                {
                    //先清理旧物品
                    stack = player.inventory[i].stack;
                    prefix = player.inventory[i].prefix;
                    player.inventory[i].TurnToAir();
                    TShock.Players[who].SendData(PacketTypes.PlayerSlot, "", who, i, 0, 0f, 0);
                    //生成新物品
                    if (model == 0)
                    {
                        int index = MyNewItem(null, player.Center, new Vector2(1, 1), item.id, stack);
                        Main.item[index].playerIndexTheItemIsReservedFor = who;
                        Main.item[index].prefix = prefix;
                        //伤害加成
                        int damage = (int)(item.orig_damage * 0.05f * item.damage_level);
                        damage = damage < item.damage_level ? item.damage_level : damage;
                        Main.item[index].damage += damage;

                        //大小加成
                        Main.item[index].scale += item.orig_scale * 0.05f * item.scale_level;

                        //击退加成
                        Main.item[index].knockBack += item.orig_knockBack * 0.05f * item.knockBack_level;

                        //攻速加成
                        Main.item[index].useAnimation = item.orig_useAnimation - item.useSpeed_level;
                        Main.item[index].useTime = (int)(item.orig_useTime * 1.0f / item.orig_useAnimation * Main.item[index].useAnimation);

                        //射速加成
                        Main.item[index].shootSpeed += item.orig_shootSpeed * 0.05f * item.shootSpeed_level;

                        TShock.Players[who].SendData(PacketTypes.ItemDrop, null, index);
                        TShock.Players[who].SendData(PacketTypes.ItemOwner, null, index);
                        TShock.Players[who].SendData(PacketTypes.TweakItem, null, index, 255, 63);
                    }
                    else if (model == 1)
                    {
                        int index = MyNewItem(null, player.Center, new Vector2(1, 1), item.id, stack);
                        Main.item[index].playerIndexTheItemIsReservedFor = who;
                        Main.item[index].prefix = prefix;
                        TShock.Players[who].SendData(PacketTypes.ItemDrop, null, index);
                        TShock.Players[who].SendData(PacketTypes.ItemOwner, null, index);
                    }
                }
            }
        }


        /// <summary>
        /// 对升级价格和能否升级进行分析和发送消息
        /// </summary>
        /// <param name="WItem"></param>
        /// <param name="whoAMI"></param>
        /// <returns></returns>
        public bool Deduction(WItem WItem, TSPlayer whoAMI, PlusType plusType)
        {
            if (!WItem.plusPrice(plusType, out long price))
            {
                whoAMI.SendMessage(LangTipsGet("当前该类型升级已达到上限，无法升级"), Color.Red);
                return false;
            }
            if (DeductCoin(whoAMI, price))
            {
                WItem.allCost += price;
                long num1 = Terraria.Utils.CoinsCount(out bool flag, whoAMI.TPlayer.inventory, new int[] { 58, 57, 56, 55, 54 });
                long num2 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank.item, new int[0]);
                long num3 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank2.item, new int[0]);
                long num4 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank3.item, new int[0]);
                long num5 = Terraria.Utils.CoinsCount(out flag, whoAMI.TPlayer.bank4.item, new int[0]);
                whoAMI.SendMessage(LangTipsGet("扣除钱币：") + cointostring(price, out List<Item> temp) + "   " + LangTipsGet("当前剩余：") + cointostring(num1 + num2 + num3 + num4 + num5, out temp), Color.Pink);
                return true;
            }
            else
            {
                whoAMI.SendInfoMessage(LangTipsGet("钱币不够！"));
                return false;
            }
        }


        /// <summary>
        /// 获取随机好看的颜色
        /// </summary>
        /// <returns></returns>
        public Color getRandColor()
        {
            return new Color(Main.rand.Next(60, 255), Main.rand.Next(60, 255), Main.rand.Next(60, 255));
        }


        /// <summary>
        /// 尝试从玩家身上扣除 coin 个铜币
        /// </summary>
        /// <param name="one"></param>
        /// <param name="coin"></param>
        /// <returns></returns>
        public bool DeductCoin(TSPlayer one, long coin)
        {
            bool flag;
            long num1 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.inventory, new int[] { 58, 57, 56, 55, 54 });
            long num2 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank.item, new int[0]);
            long num3 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank2.item, new int[0]);
            long num4 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank3.item, new int[0]);
            long num5 = Terraria.Utils.CoinsCount(out flag, one.TPlayer.bank4.item, new int[0]);
            if (num1 + num2 + num3 + num4 + num5 < coin)
                return false;

            long temp = 0L;
            //一个一个找钱币并清空该槽位钱币，若 >= 花费，停止并统计总清空的钱币数temp
            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventoryIndex.Item2)
                {
                    if (one.TPlayer.inventory[i].IsACoin && i != 54 && i != 55 && i != 56 && i != 57 && i != 58)
                    {
                        temp += one.TPlayer.inventory[i].value / 5L * one.TPlayer.inventory[i].stack;
                        one.TPlayer.inventory[i].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                        if (temp >= coin) break;
                    }
                }
                else if (i >= NetItem.MiscDyeIndex.Item2 && i < NetItem.PiggyIndex.Item2)
                {
                    int numt = i - NetItem.PiggyIndex.Item1;
                    if (one.TPlayer.bank.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank.item[numt].value / 5L * one.TPlayer.bank.item[numt].stack;
                        one.TPlayer.bank.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                        if (temp >= coin) break;
                    }
                }
                else if (i >= NetItem.PiggyIndex.Item2 && i < NetItem.SafeIndex.Item2)
                {
                    int numt = i - NetItem.SafeIndex.Item1;
                    if (one.TPlayer.bank2.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank2.item[numt].value / 5L * one.TPlayer.bank2.item[numt].stack;
                        one.TPlayer.bank2.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                        if (temp >= coin) break;
                    }
                }
                else if (i >= NetItem.TrashIndex.Item2 && i < NetItem.ForgeIndex.Item2)
                {
                    int numt = i - NetItem.ForgeIndex.Item1;
                    if (one.TPlayer.bank3.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank3.item[numt].value / 5L * one.TPlayer.bank3.item[numt].stack;
                        one.TPlayer.bank3.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                        if (temp >= coin) break;
                    }
                }
                else if (i >= NetItem.ForgeIndex.Item2 && i < NetItem.VoidIndex.Item2)
                {
                    int numt = i - NetItem.VoidIndex.Item1;
                    if (one.TPlayer.bank4.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank4.item[numt].value / 5L * one.TPlayer.bank4.item[numt].stack;
                        one.TPlayer.bank4.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                        if (temp >= coin) break;
                    }
                }
            }

            temp -= coin;
            List<Item> items;

            if (num2 > 0)
            {
                for (int i = NetItem.MiscDyeIndex.Item2; i < NetItem.PiggyIndex.Item2; i++)
                {
                    int numt = i - NetItem.PiggyIndex.Item1;
                    if (one.TPlayer.bank.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank.item[numt].value / 5L * one.TPlayer.bank.item[numt].stack;
                        one.TPlayer.bank.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
                cointostring(temp, out items);
                for (int i = NetItem.MiscDyeIndex.Item2; i < NetItem.PiggyIndex.Item2; i++)
                {
                    int numt = i - NetItem.PiggyIndex.Item1;
                    if (one.TPlayer.bank.item[numt].IsAir && items.Count > 0)
                    {
                        one.TPlayer.bank.item[numt] = items.First();
                        items.RemoveAt(0);
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
            }
            else if (num3 > 0)
            {
                for (int i = NetItem.PiggyIndex.Item2; i < NetItem.SafeIndex.Item2; i++)
                {
                    int numt = i - NetItem.SafeIndex.Item1;
                    if (one.TPlayer.bank2.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank2.item[numt].value / 5L * one.TPlayer.bank2.item[numt].stack;
                        one.TPlayer.bank2.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
                cointostring(temp, out items);
                for (int i = NetItem.PiggyIndex.Item2; i < NetItem.SafeIndex.Item2; i++)
                {
                    int numt = i - NetItem.SafeIndex.Item1;
                    if (one.TPlayer.bank2.item[numt].IsAir && items.Count > 0)
                    {
                        one.TPlayer.bank2.item[numt] = items.First();
                        items.RemoveAt(0);
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
            }
            else if (num4 > 0)
            {
                for (int i = NetItem.TrashIndex.Item2; i < NetItem.ForgeIndex.Item2; i++)
                {
                    int numt = i - NetItem.ForgeIndex.Item1;
                    if (one.TPlayer.bank3.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank3.item[numt].value / 5L * one.TPlayer.bank3.item[numt].stack;
                        one.TPlayer.bank3.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
                cointostring(temp, out items);
                for (int i = NetItem.TrashIndex.Item2; i < NetItem.ForgeIndex.Item2; i++)
                {
                    int numt = i - NetItem.ForgeIndex.Item1;
                    if (one.TPlayer.bank3.item[numt].IsAir && items.Count > 0)
                    {
                        one.TPlayer.bank3.item[numt] = items.First();
                        items.RemoveAt(0);
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
            }
            else if (num5 > 0)
            {
                for (int i = NetItem.ForgeIndex.Item2; i < NetItem.VoidIndex.Item2; i++)
                {
                    int numt = i - NetItem.VoidIndex.Item1;
                    if (one.TPlayer.bank4.item[numt].IsACoin)
                    {
                        temp += one.TPlayer.bank4.item[numt].value / 5L * one.TPlayer.bank4.item[numt].stack;
                        one.TPlayer.bank4.item[numt].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
                cointostring(temp, out items);
                for (int i = NetItem.ForgeIndex.Item2; i < NetItem.VoidIndex.Item2; i++)
                {
                    int numt = i - NetItem.VoidIndex.Item1;
                    if (one.TPlayer.bank4.item[numt].IsAir && items.Count > 0)
                    {
                        one.TPlayer.bank4.item[numt] = items.First();
                        items.RemoveAt(0);
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < NetItem.InventoryIndex.Item2; i++)
                {
                    if (one.TPlayer.inventory[i].IsACoin && i != 54 && i != 55 && i != 56 && i != 57 && i != 58)
                    {
                        temp += one.TPlayer.inventory[i].value / 5L * one.TPlayer.inventory[i].stack;
                        one.TPlayer.inventory[i].TurnToAir();
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
                cointostring(temp, out items);
                for (int i = 0; i < NetItem.InventoryIndex.Item2; i++)
                {
                    if (one.TPlayer.inventory[i].IsAir && items.Count > 0 && i != 54 && i != 55 && i != 56 && i != 57 && i != 58)
                    {
                        one.TPlayer.inventory[i] = items.First();
                        items.RemoveAt(0);
                        one.SendData(PacketTypes.PlayerSlot, "", one.Index, i);
                    }
                }
            }

            if (items.Count > 0)
            {
                foreach (var v in items)
                {
                    int num = Item.NewItem(new EntitySource_DebugCommand(), one.TPlayer.Center, new Vector2(5, 5), v.type, v.stack, true, 0, true, false);
                    Main.item[num].playerIndexTheItemIsReservedFor = one.Index;
                    one.SendData(PacketTypes.ItemDrop, "", num, 1f, 0f, 0f, 0);
                    one.SendData(PacketTypes.ItemOwner, null, num, 0f, 0f, 0f, 0);
                }
            }
            return true;
        }


        /// <summary>
        /// 将 long 硬币数转化成 xx铂 xx金 xx银 xx铜 的字符串 和 item[]
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="Model">0代表图标字符串，1代表纯文本</param>
        /// <returns></returns>
        public static string cointostring(long coin, out List<Item> items)
        {
            items = new List<Item>();
            long copper = coin % 100;  //71
            coin /= 100;
            long silver = coin % 100; //72
            coin /= 100;
            long gold = coin % 100; //73
            coin /= 100;
            long platinum = coin; //74

            Item temp1 = TShock.Utils.GetItemById(74);
            temp1.stack = (int)platinum;
            items.Add(temp1);
            Item temp2 = TShock.Utils.GetItemById(73);
            temp2.stack = (int)gold;
            items.Add(temp2);
            Item temp3 = TShock.Utils.GetItemById(72);
            temp3.stack = (int)silver;
            items.Add(temp3);
            Item temp4 = TShock.Utils.GetItemById(71);
            temp4.stack = (int)copper;
            items.Add(temp4);

            return $"{platinum}[i:74],  {gold}[i:73],  {silver}[i:72],  {copper}[i:71]";
        }
    }
}
