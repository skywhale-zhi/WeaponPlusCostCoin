using Microsoft.Xna.Framework;
using System.Text;
using Terraria;
using TShockAPI;

namespace WeaponPlus
{
    /// <summary>
    /// 强化类型
    /// </summary>
    public enum PlusType
    {
        damage, scale, knockBack, useSpeed, shootSpeed
    }

    public class WItem
    {
        #region 表内数据
        public int id;
        /// <summary>
        /// 物品所有者
        /// </summary>
        public string owner;
        /// <summary>
        /// 标签
        /// </summary>
        public int lable;
        /// <summary>
        /// 物品总等级，对他赋值是无效的
        /// </summary>
        public int Level
        {
            get { return damage_level + scale_level + knockBack_level + useSpeed_level + shootSpeed_level; }
        }

        public int damage_level;
        public int scale_level;
        public int knockBack_level;
        public int useSpeed_level;
        public int shootSpeed_level;
        /// <summary>
        /// 升级点数总的花费
        /// </summary>
        public long allCost;
        #endregion


        //原始数据记录
        public readonly int orig_damage;
        public readonly float orig_scale;
        public readonly float orig_knockBack;
        public readonly int orig_useAnimation;
        public readonly int orig_useTime;
        public readonly float orig_shootSpeed;
        public readonly Color orig_color;
        public readonly int orig_shoot;


        public WItem(int ID, string owner = "")
        {
            Item temp = TShock.Utils.GetItemById(ID);
            this.id = ID;
            this.owner = owner;
            this.damage_level = 0;
            this.scale_level = 0;
            this.knockBack_level = 0;
            this.useSpeed_level = 0;
            this.shootSpeed_level = 0;
            this.allCost = 0;

            orig_damage = temp.damage;
            orig_scale = temp.scale;
            orig_knockBack = temp.knockBack;
            orig_useAnimation = temp.useAnimation;
            orig_useTime = temp.useTime;
            orig_shootSpeed = temp.shootSpeed;
            orig_color = temp.color;
            orig_shoot = temp.shoot;
        }


        /// <summary>
        /// 如果该物品的 Level <= 0，那么从数据库中移除本物品，同时尝试清理玩家的 hasitem
        /// </summary>
        public void checkDB()
        {
            if (string.IsNullOrWhiteSpace(owner))
                return;
            if (Level <= 0)
            {
                try
                {
                    WeaponPlus.DB.DeleteDB(owner, id);
                    var list = TSPlayer.FindByNameOrID(owner);
                    if (list.Count == 1)
                    {
                        WPlayer? temp = WeaponPlus.wPlayers[list[0].Index];
                        if (temp != null && temp.isActive)
                        {
                            temp.hasItems.RemoveAll(x => x.id == id);
                        }
                    }
                }
                catch { }
            }
        }


        /// <summary>
        /// 获取当前武器的状态信息
        /// </summary>
        /// <returns></returns>
        public string ItemMess()
        {
            StringBuilder sb = new StringBuilder();
            long price;
            if (Level > 0)
                sb.AppendLine($"{WeaponPlus.LangTipsGet("当前总等级：")}{Level}   {WeaponPlus.LangTipsGet("剩余强化次数：")}{WeaponPlus.config.最多升级次数_MaximunofLevel - Level} {WeaponPlus.LangTipsGet("次")}    {WeaponPlus.LangTipsGet("伤害等级：")}{damage_level}, {WeaponPlus.LangTipsGet("大小等级：")}{scale_level}, {WeaponPlus.LangTipsGet("击退等级：")}{knockBack_level}, {WeaponPlus.LangTipsGet("攻速等级：")}{useSpeed_level}, {WeaponPlus.LangTipsGet("射弹飞行速度等级：")}{shootSpeed_level}");
            else
                sb.AppendLine(WeaponPlus.LangTipsGet("未升级过，无任何加成"));
            float u;
            int damage = (int)(orig_damage * 0.05f * damage_level);
            if (damage < damage_level)
            {
                damage = damage_level;
            }
            u = damage * 1.0f / orig_damage;
            sb.Append($"{WeaponPlus.LangTipsGet("当前状态：")}{WeaponPlus.LangTipsGet("伤害")} +{u:0.00%}，{WeaponPlus.LangTipsGet("大小")} +{0.05f * scale_level:0.00%}，{WeaponPlus.LangTipsGet("击退")} +{0.05f * knockBack_level:0.00%}，");
            u = orig_useAnimation * 1.0f / (orig_useAnimation - useSpeed_level) - 1;
            sb.AppendLine($"{WeaponPlus.LangTipsGet("攻速")}+{u:0.00%}，{WeaponPlus.LangTipsGet("射弹飞速")}+{0.05f * shootSpeed_level:0.00%}");

            if (Level < WeaponPlus.config.最多升级次数_MaximunofLevel)
            {
                sb.AppendLine($"{WeaponPlus.LangTipsGet("伤害升至下一级需：")}{(plusPrice(PlusType.damage, out price) ? WeaponPlus.cointostring(price, out List<Item> temp) : WeaponPlus.LangTipsGet("当前已满级"))}");
                sb.AppendLine($"{WeaponPlus.LangTipsGet("大小升至下一级需：")}{(plusPrice(PlusType.scale, out price) ? WeaponPlus.cointostring(price, out temp) : WeaponPlus.LangTipsGet("当前已满级"))}");
                sb.AppendLine($"{WeaponPlus.LangTipsGet("击退升至下一级需：")}{(plusPrice(PlusType.knockBack, out price) ? WeaponPlus.cointostring(price, out temp) : WeaponPlus.LangTipsGet("当前已满级"))}");
                sb.AppendLine($"{WeaponPlus.LangTipsGet("攻速升至下一级需：")}{(plusPrice(PlusType.useSpeed, out price) ? WeaponPlus.cointostring(price, out temp) : WeaponPlus.LangTipsGet("当前已满级"))}");
                sb.Append($"{WeaponPlus.LangTipsGet("射弹飞速升至下一级需：")}{(plusPrice(PlusType.shootSpeed, out price) ? WeaponPlus.cointostring(price, out temp) : WeaponPlus.LangTipsGet("当前已满级"))}");
            }
            else
            {
                sb.Append(WeaponPlus.LangTipsGet("已达到最大武器总等级"));
            }
            return sb.ToString();
        }


        /// <summary>
        /// 计算当前物品升级至下一级需要的点数
        /// </summary>
        /// <param name="plus"> 选择强化类型进行价格计算 </param>
        /// <returns> 能否继续升级 </returns>
        public bool plusPrice(PlusType plus, out long price)
        {
            Item temp = TShock.Utils.GetItemById(id);
            price = 0;
            if(Level >= WeaponPlus.config.最多升级次数_MaximunofLevel)
            {
                return false;
            }
            float cost;
            float itemvalue = temp.value;
            if (temp.maxStack == 1 && temp.value < 5000)
                itemvalue = 5000;
            else if (temp.maxStack > 1 && temp.value == 0)
                itemvalue = 5;

            switch (temp.type)
            {
                case 1569://地牢六环境武器
                case 1156:
                case 1571:
                case 1260:
                case 4607:
                case 1572:
                case 757://泰拉刃
                case 4952://夜光
                case 4953://日暮
                case 4923://星光
                case 4914://万花筒
                case 4715://星星吉他
                case 3870://双足翼龙怒气
                case 3859://空中祸害
                case 3858://天龙之怒
                case 3827://飞龙
                case 2611://猪鲨链球
                case 2624://海啸
                case 2623://泡泡枪
                case 2622://利刃台风
                case 2621://暴风雨法杖
                    itemvalue = 500000;
                    break;
                case 1324://香蕉
                case 756://蘑菇长毛
                case 496://冰雪魔杖
                    itemvalue = 270000;
                    break;
                case 561://光辉飞盘
                    itemvalue = 230000;
                    break;
                case 4281://雀杖
                    itemvalue = 25000;
                    break;
                case 98://迷你鲨
                    itemvalue = 30000;
                    break;
                case 4273://吸血青蛙
                case 4381://血雨弓
                    itemvalue = 50000;
                    break;
                case 4703://四管霰弹枪
                case 4760://中士联盾
                case 1265://伍兹冲锋枪
                    itemvalue = 125000;
                    break;
                case 2270://鳄鱼机关枪
                case 1327://死神镰刀
                case 3007://飞镖步枪
                case 3008://飞镖
                case 3012://血滴子
                case 3013://臭虎爪
                case 3014://爬藤怪法杖
                case 3029://代达罗斯
                case 3030://飞刀
                case 3051://魔晶碎块
                case 3006://夺命杖
                    itemvalue /= 2;
                    break;
                case 5065://共鸣权杖
                case 674://原版断钢剑
                case 675://原版永夜
                case 4348://橙冲击波
                    itemvalue = 350000;
                    break;
                case 3531://四柱武器
                case 3473:
                case 3540:
                case 3474:
                case 3475:
                case 3476:
                case 3541:
                case 3389:
                case 3571:
                case 3930:
                case 3569:
                case 3570:
                case 3542:
                case 3543:
                    itemvalue = 650000;
                    break;
                default:
                    break;
            }

            float k = 2f;
            cost = itemvalue * k; // 5 %

            switch (plus)
            {
                case PlusType.damage:
                    {
                        int damage = (int)(orig_damage * 0.05f * (damage_level + 1));
                        if (damage < damage_level + 1)
                        {
                            cost = (1f / orig_damage) * (itemvalue * 20 * k);
                            damage = damage_level + 1;
                        }
                        if (damage + orig_damage > WeaponPlus.config.武器升级伤害上限倍数_MaximumDamageMultipleOfWeaponUpgrade * orig_damage)
                        {
                            return false;
                        }
                        if (temp.magic)
                            cost *= 0.9f;
                    }
                    break;
                case PlusType.scale:
                    {
                        if (orig_scale + orig_scale * 0.05f * (scale_level + 1) > orig_scale * WeaponPlus.config.武器升级尺寸上限倍数_MaximumScaleMultipleOfWeaponUpgrade)
                        {
                            return false;
                        }
                        if (!temp.melee)
                            cost *= 0.7f;
                    }
                    break;
                case PlusType.knockBack:
                    {
                        if (orig_knockBack + orig_knockBack * 0.05f * (knockBack_level + 1) > orig_knockBack * WeaponPlus.config.武器升级击退上限倍数_MaximumKnockBackMultipleOfWeaponUpgrade)
                        {
                            return false;
                        }
                        if (temp.summon)
                            cost *= 0.7f;
                        else
                            cost *= 0.85f;
                    }
                    break;
                case PlusType.useSpeed:
                    {
                        int shangxian;
                        if (temp.melee)
                            shangxian = WeaponPlus.config.近战武器升级攻速上限_MaximumAttackSpeedOfMeleeWeaponUpgrade;
                        else if (temp.magic)
                            shangxian = WeaponPlus.config.魔法武器升级攻速上限_MaximumAttackSpeedOfMagicWeaponUpgrade;
                        else if (temp.ranged)
                            shangxian = WeaponPlus.config.远程武器升级攻速上限_MaximumAttackSpeedOfRangeWeaponUpgrade;
                        else if (temp.summon)
                            shangxian = WeaponPlus.config.召唤武器升级攻速上限_MaximumAttackSpeedOfSummonWeaponUpgrade;
                        else
                            shangxian = WeaponPlus.config.其他武器升级攻速上限_MaximumAttackSpeedOfOtherWeaponUpgrade;

                        if (orig_useAnimation - (useSpeed_level + 1) < shangxian || orig_useAnimation <= shangxian)
                        {
                            return false;
                        }
                        else if(orig_useTime - (useSpeed_level + 1) < shangxian || orig_useTime <= shangxian)
                        {
                            return false;
                        }
                        else if (orig_useAnimation * 1.0f / (orig_useAnimation - (useSpeed_level + 1)) > WeaponPlus.config.武器升级攻速上限倍数_MaximumUseTimeMultipleOfWeaponUpgrade)
                        {
                            return false;
                        }
                        else
                        {
                            cost = (orig_useAnimation * 1.0f / (orig_useAnimation - (useSpeed_level + 1.0f)) - orig_useAnimation * 1.0f / (orig_useAnimation - useSpeed_level)) * (itemvalue * 20 * k);
                            cost *= 1.05f;
                        }
                        if(temp.summon && temp.mana > 0)
                        {
                            cost *= 0.3f;
                        }
                    }
                    break;
                case PlusType.shootSpeed:
                    {
                        if (orig_shootSpeed + orig_shootSpeed * 0.05f * (shootSpeed_level + 1) > orig_shootSpeed * WeaponPlus.config.武器升级射弹飞速上限倍数_MaximumProjectileSpeedMultipleOfWeaponUpgrade)
                        {
                            return false;
                        }
                    }
                    break;
                default: break;
            }
            cost *= (1 + WeaponPlus.config.升级花费增加_UpgradeCostsIncrease * Level) * 0.7f;
            if (Level < 3)
                cost *= 0.2f;
            price = (int)(cost * temp.maxStack * WeaponPlus.config.花费参数_CostParameters);
            return true;
        }
    }
}
