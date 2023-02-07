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
                    8, 6, 4, 8, 8, 2.0f, 3.0f, 100.0f, 2.5f, 3.0f, 1.0f, 50
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

        public Config(int 近战武器升级攻速上限, int 魔法武器升级攻速上限, int 远程武器升级攻速上限, int 召唤武器升级攻速上限, int 其他武器升级攻速上限, float 武器升级伤害上限倍数, float 武器升级击退上限倍数, float 武器升级速度上限倍数, float 武器升级尺寸上限倍数, float 武器升级射弹飞速上限倍数, float 花费参数, int 最多升级次数)
        {
            this.近战武器升级攻速上限 = 近战武器升级攻速上限;
            this.魔法武器升级攻速上限 = 魔法武器升级攻速上限;
            this.远程武器升级攻速上限 = 远程武器升级攻速上限;
            this.召唤武器升级攻速上限 = 召唤武器升级攻速上限;
            this.其他武器升级攻速上限 = 其他武器升级攻速上限;
            this.武器升级伤害上限倍数 = 武器升级伤害上限倍数;
            this.武器升级击退上限倍数 = 武器升级击退上限倍数;
            this.武器升级速度上限倍数 = 武器升级速度上限倍数;
            this.武器升级尺寸上限倍数 = 武器升级尺寸上限倍数;
            this.武器升级射弹飞速上限倍数 = 武器升级射弹飞速上限倍数;
            this.花费参数 = 花费参数;
            this.最多升级次数 = 最多升级次数;
        }

        public int 近战武器升级攻速上限;
        public int 魔法武器升级攻速上限;
        public int 远程武器升级攻速上限;
        public int 召唤武器升级攻速上限;
        public int 其他武器升级攻速上限;
        public float 武器升级伤害上限倍数;
        public float 武器升级击退上限倍数;
        public float 武器升级速度上限倍数;
        public float 武器升级尺寸上限倍数;
        public float 武器升级射弹飞速上限倍数;
        public float 花费参数;
        public int 最多升级次数;
    }
}
