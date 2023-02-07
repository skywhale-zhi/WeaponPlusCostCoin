using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace WeaponPlus
{
    [ApiVersion(2, 1)]
    public partial class WeaponPlus : TerrariaPlugin
    {
        public override string Author => "z枳";
        public override string Description => "允许在基础属性上强化任何武器";
        public override string Name => "WeaponPlusCostCoin";
        public override Version Version => new Version(1, 0, 0, 0);

        public string configPath = Path.Combine(TShock.SavePath, "WeaponPlus.json");

        public static Config config = new Config();

        public static WPlayer[] wPlayers = new WPlayer[Main.maxPlayers + 1];

        /// <summary>
        /// 强化物品数据库
        /// </summary>
        public static WeaponPlusDB DB { get; set; }

        public WeaponPlus(Main game ) : base(game) { }

        public override void Initialize()
        {
            config = Config.LoadConfig();
            DB = new WeaponPlusDB(TShock.DB);

            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);

            Commands.ChatCommands.Add(new Command("weaponplus.plus", PlusItem, "plus")
            {
                HelpText = "输入 /plus help   查看 plus 系列指令帮助"
            });
            Commands.ChatCommands.Add(new Command("weaponplus.admin", ClearPlusItem, "clearallplayersplus")
            {
                HelpText = "输入 /clearallplayersplus   将数据库中所有玩家的所有强化物品全部清理"
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }
        }
    }
}