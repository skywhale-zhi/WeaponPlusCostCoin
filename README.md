# WeaponPlusCostCoin
强化你的武器
# 介绍
- 该插件能强化你的几乎所有武器和弹药的 伤害，攻速，击退，大小，射弹飞行速度 属性，采用升级的方式
- 每次升级选择一个属性方向进行升级，可以自由配置武器的属性分配，等级越高，升级需要的代价越大
- 强化栏为玩家第一个物品栏，将武器放入再使用指令即可有效，升级是升级一类武器（同ID类）而不是一个武器
- 该强化会使物品原本的词缀无效一部分，所以前三次升级价格降低 80%
- 由于攻速属性比其他的特殊，对攻速的升级需要的代价变化更大
- 可以由配置文件设置各种属性的最大升级加成
- 所有强化数据写入 tshock.sqlite 数据库中的 WeaponPlusDBcostCoin 表中
- 该插件的属性修改是根据武器的原始数据进行编辑，发生在套装buff等加成之前，当武器的某个属性为 0 时，增幅无效，比如刃杖的击退
- 该插件消耗钱币来升级
# 指令和权限
- 权限1：`weaponplus.plus`
- 指令1-1：`/plus help`
- 功能1-1：查看该插件的所有指令帮助
- 指令1-2：`/plus`
- 功能1-2：查看当前（第一个物品栏）内武器的等级状态，并查看升至下一级需要多少代价
- 指令1-3：`/plus load`
- 功能1-3：当武器扔出物品栏或重铸导致加成失效时，使用此指令，能将背包里所有加强过的那类武器重新读取
- 指令1-4：`/plus [damage/da/伤害] [up/down]`
- 功能1-4：对伤害等级进行升级或降级，变化5%，若武器伤害过低，5%的增幅 < 1，那么强制攻击至少 +1 点，并根据实际增幅量扣费
- 指令1-5：`/plus [scale/sc/大小] [up/down]`
- 功能1-5：对大小等级进行升级或降级，变化5%。该功能会增加由该武器直接生成的射弹的尺寸大小，如泰拉刃，对近战武器效果更明显，对召唤仆从几乎无效
- 指令1-6：`/plus [knockback/kn/击退] [up/down]`
- 功能1-6：对击退等级进行升级或降级，变化5%
- 指令1-7：`/plus [usespeed/us/用速] [up/down]`
- 功能1-7：对攻速等级进行升级或降级，变化不定。因为攻速的奇怪设定，每次升级必定增加但是代价计算较为特殊
- 指令1-8：`/plus [shootspeed/sh/飞速] [up/down]`
- 功能1-8：对射弹飞行速度进行升级或降级，变化5%。这个会影响鞭子武器的范围
- 指令1-9：`/plus clear`
- 功能1-9：重置当前选中的武器的所有等级，返还一半消耗材料
-
- 权限2：`weaponplus.admin`
- 指令2：`/clearallplayersplus`
- 功能2：从数据库中清理所有强化过的武器并将玩家身上所有强化过的武器重置，清理所有数据
# 部分问题
- 原版攻速是整数，且加快的方法是减法。攻速 60 意味着 1 秒攻击 1 次，60 代表一个攻击动作需要 60 帧来完成，60 帧 = 1 秒，每次升级从这个数值 - 1，攻速最快是 1 ，即每秒攻击 60 次，这意味着攻速慢的武器升级代价越低，越接近 1 的武器升级攻速需要的代价越大
- 配置文件允许限制每个属性的上限，除了攻速以外的属性都是上限倍数限制的，如伤害上限倍数为 2，那么一个攻击 150 的武器伤害等级升到最高后伤害为 300
- 攻速上限有两个限制，一个是实际攻速限制，如`近战武器升级攻速上限: 8`，近战武器的攻速最大就为 8，已达到或超过 8 的武器不能升级。另一个是上限倍数限制：一个攻速 60 的武器升级到 30，速度是原来的2倍，+ 100 %，达到了 2 倍数限制；一个攻速 60 的武器升级到 1，速度是原来的60倍，+ 5900 %，达到了 60 倍数限制，两个限制同时起效
- 召唤法杖的射弹有自己的AI，除了伤害和击退外不会受到其他任何加成，提高攻速也只会增加法杖的挥舞速度而已，但是对于召唤武器鞭子，鞭子的范围却受到射弹飞速的加成
# 配置文件 WeaponPlus.json
```
{
  "近战武器升级攻速上限": 8,   //在【部分问题】里解释的很清楚了
  "魔法武器升级攻速上限": 6,
  "远程武器升级攻速上限": 4,
  "召唤武器升级攻速上限": 8,
  "其他武器升级攻速上限": 8,
  "武器升级伤害上限倍数": 2.0,
  "武器升级击退上限倍数": 3.0,
  "武器升级攻速上限倍数": 100.0,
  "武器升级尺寸上限倍数": 2.5,
  "武器升级射弹飞速上限倍数": 3.0,
  "花费参数": 1.0,	   //越大每次升级花费越多，越小花费越少
  "最多升级次数": 50    //总等级最大50级，让玩家合理分配升级方向
}
```