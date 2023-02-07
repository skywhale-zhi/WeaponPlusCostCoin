using Newtonsoft.Json;
using TShockAPI;

namespace WeaponPlus
{
    public class Config
    {
        static string configPath = Path.Combine(TShock.SavePath, "WeaponPlus.json");

        public static Config LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                Config config = new Config(
                    8, 6, 4, 8, 8, 2.0f, 3.0f, 60.0f, 2.5f, 3.0f, 1.0f, 0.2f, 50, true
                );
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            else
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
                return config;
            }
        }

        public Config() { }

        public Config(int 近战武器升级攻速上限_MaximumAttackSpeedOfMeleeWeaponUpgrade, int 魔法武器升级攻速上限_MaximumAttackSpeedOfMagicWeaponUpgrade, int 远程武器升级攻速上限_MaximumAttackSpeedOfRangeWeaponUpgrade, int 召唤武器升级攻速上限_MaximumAttackSpeedOfSummonWeaponUpgrade, int 其他武器升级攻速上限_MaximumAttackSpeedOfOtherWeaponUpgrade, float 武器升级伤害上限倍数_MaximumDamageMultipleOfWeaponUpgrade, float 武器升级击退上限倍数_MaximumKnockBackMultipleOfWeaponUpgrade, float 武器升级攻速上限倍数_MaximumUseTimeMultipleOfWeaponUpgrade, float 武器升级尺寸上限倍数_MaximumScaleMultipleOfWeaponUpgrade, float 武器升级射弹飞速上限倍数_MaximumProjectileSpeedMultipleOfWeaponUpgrade, float 花费参数_CostParameters, float 升级花费增加_UpgradeCostsIncrease, int 最多升级次数_MaximunofLevel, bool 进服时是否开启自动重读武器_WhetherToTurnOnAutomaticLoadWeaponsWhenEnteringTheGame)
        {
            this.近战武器升级攻速上限_MaximumAttackSpeedOfMeleeWeaponUpgrade = 近战武器升级攻速上限_MaximumAttackSpeedOfMeleeWeaponUpgrade;
            this.魔法武器升级攻速上限_MaximumAttackSpeedOfMagicWeaponUpgrade = 魔法武器升级攻速上限_MaximumAttackSpeedOfMagicWeaponUpgrade;
            this.远程武器升级攻速上限_MaximumAttackSpeedOfRangeWeaponUpgrade = 远程武器升级攻速上限_MaximumAttackSpeedOfRangeWeaponUpgrade;
            this.召唤武器升级攻速上限_MaximumAttackSpeedOfSummonWeaponUpgrade = 召唤武器升级攻速上限_MaximumAttackSpeedOfSummonWeaponUpgrade;
            this.其他武器升级攻速上限_MaximumAttackSpeedOfOtherWeaponUpgrade = 其他武器升级攻速上限_MaximumAttackSpeedOfOtherWeaponUpgrade;
            this.武器升级伤害上限倍数_MaximumDamageMultipleOfWeaponUpgrade = 武器升级伤害上限倍数_MaximumDamageMultipleOfWeaponUpgrade;
            this.武器升级击退上限倍数_MaximumKnockBackMultipleOfWeaponUpgrade = 武器升级击退上限倍数_MaximumKnockBackMultipleOfWeaponUpgrade;
            this.武器升级攻速上限倍数_MaximumUseTimeMultipleOfWeaponUpgrade = 武器升级攻速上限倍数_MaximumUseTimeMultipleOfWeaponUpgrade;
            this.武器升级尺寸上限倍数_MaximumScaleMultipleOfWeaponUpgrade = 武器升级尺寸上限倍数_MaximumScaleMultipleOfWeaponUpgrade;
            this.武器升级射弹飞速上限倍数_MaximumProjectileSpeedMultipleOfWeaponUpgrade = 武器升级射弹飞速上限倍数_MaximumProjectileSpeedMultipleOfWeaponUpgrade;
            this.花费参数_CostParameters = 花费参数_CostParameters;
            this.升级花费增加_UpgradeCostsIncrease = 升级花费增加_UpgradeCostsIncrease;
            this.最多升级次数_MaximunofLevel = 最多升级次数_MaximunofLevel;
            this.进服时是否开启自动重读武器_WhetherToTurnOnAutomaticLoadWeaponsWhenEnteringTheGame = 进服时是否开启自动重读武器_WhetherToTurnOnAutomaticLoadWeaponsWhenEnteringTheGame;
        }

        public int 近战武器升级攻速上限_MaximumAttackSpeedOfMeleeWeaponUpgrade;
        public int 魔法武器升级攻速上限_MaximumAttackSpeedOfMagicWeaponUpgrade;
        public int 远程武器升级攻速上限_MaximumAttackSpeedOfRangeWeaponUpgrade;
        public int 召唤武器升级攻速上限_MaximumAttackSpeedOfSummonWeaponUpgrade;
        public int 其他武器升级攻速上限_MaximumAttackSpeedOfOtherWeaponUpgrade;
        public float 武器升级伤害上限倍数_MaximumDamageMultipleOfWeaponUpgrade;
        public float 武器升级击退上限倍数_MaximumKnockBackMultipleOfWeaponUpgrade;
        public float 武器升级攻速上限倍数_MaximumUseTimeMultipleOfWeaponUpgrade;
        public float 武器升级尺寸上限倍数_MaximumScaleMultipleOfWeaponUpgrade;
        public float 武器升级射弹飞速上限倍数_MaximumProjectileSpeedMultipleOfWeaponUpgrade;
        public float 花费参数_CostParameters;
        public float 升级花费增加_UpgradeCostsIncrease;
        public int 最多升级次数_MaximunofLevel;
        public bool 进服时是否开启自动重读武器_WhetherToTurnOnAutomaticLoadWeaponsWhenEnteringTheGame;
    }
}
