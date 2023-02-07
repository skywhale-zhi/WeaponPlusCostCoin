using TShockAPI;

namespace WeaponPlus
{
    public class WPlayer
    {
        public TSPlayer me;
        public List<WItem> hasItems;

        /// <summary>
        /// 仅用于验证是否同步的，不要使用他  玩家索引
        /// </summary>
        private int index = 255;
        /// <summary>
        /// 仅用于验证是否同步的，不要使用他  玩家名称
        /// </summary>
        private string name = string.Empty;
        /// <summary>
        /// 从  原Tshock空?, 是否登录, index, whoami, TPlayer空?, _isActive 几个方面验证是否同步成功并合理存在
        /// </summary>
        public bool isActive
        {
            get { return me != null && me.Active && index == me.Index && me.TPlayer != null && index == me.TPlayer.whoAmI && me.TPlayer.active && name == me.Name && name == me.TPlayer.name; }
        }


        public WPlayer(TSPlayer? me)
        {
            if (me == null)
            {
                this.me = new TSPlayer(-1);
                this.index = 255;
                this.name = string.Empty;
            }
            else
            {
                this.me = me;
                this.index = me.Index;
                this.name = me.Name;
            }
            this.hasItems = new List<WItem>();
        }
    }
}
