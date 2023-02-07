using MySql.Data.MySqlClient;
using System.Data;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace WeaponPlus
{
    public class WeaponPlusDB
    {
        private IDbConnection database;
        private string tableName;

        public WeaponPlusDB(IDbConnection database)
        {
            this.database = database;
            this.tableName = "WeaponPlusDBcostCoin";
            SqlTable table = new SqlTable(tableName, new SqlColumn[]
            {
                new SqlColumn("owner", MySqlDbType.Text),
                new SqlColumn("itemID", MySqlDbType.Int32),
                new SqlColumn("itemName", MySqlDbType.Text),

                new SqlColumn("lable", MySqlDbType.Int32),
                new SqlColumn("level", MySqlDbType.Int32),

                new SqlColumn("damage_level", MySqlDbType.Int32),
                new SqlColumn("scale_level", MySqlDbType.Int32),
                new SqlColumn("knockBack_level", MySqlDbType.Int32),
                new SqlColumn("useSpeed_level", MySqlDbType.Int32),
                new SqlColumn("shootSpeed_level", MySqlDbType.Int32),
                new SqlColumn("allCost", MySqlDbType.Int64)
            });
            IQueryBuilder queryBuilder;
            if (database.GetSqlType() != SqlType.Sqlite)
            {
                queryBuilder = new MysqlQueryCreator();
            }
            else
            {
                queryBuilder = new SqliteQueryCreator();
            }
            queryBuilder.CreateTable(table);
            SqlTableCreator sqlTableCreator = new SqlTableCreator(database, queryBuilder);
            sqlTableCreator.EnsureTableStructure(table);
        }


        /// <summary>
        /// 获取该玩家所有升级的武器，id = 0 获取所有的，id != 0 获取那一个武器
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public WItem[] ReadDBGetWItemsFromOwner(string owner, int ID = 0)
        {
            List<WItem> WItems = new List<WItem>();
            string mess = ID == 0 ? "'" : "' And itemID = " + ID;
            try
            {
                using (QueryResult queryResult = database.QueryReader("SELECT * FROM " + tableName + " WHERE owner = '" + owner + mess))
                {
                    while (queryResult.Read())
                    {
                        WItem item = new WItem(queryResult.Get<int>("itemID"), owner);
                        item.lable = queryResult.Get<int>("lable");
                        item.damage_level = queryResult.Get<int>("damage_level");
                        item.scale_level = queryResult.Get<int>("scale_level");
                        item.knockBack_level = queryResult.Get<int>("knockBack_level");
                        item.useSpeed_level = queryResult.Get<int>("useSpeed_level");
                        item.shootSpeed_level = queryResult.Get<int>("shootSpeed_level");
                        item.allCost = queryResult.Get<long>("allCost");
                        WItems.Add(item);
                    }
                }
                return WItems.ToArray();
            }
            catch (Exception ex)
            {
                TShock.Log.Error("错误：ReadDBGetWItemsFromOwner " + ex.ToString());
                TSPlayer.All.SendErrorMessage("错误：ReadDBGetWItemsFromOwner " + ex.ToString());
                Console.WriteLine("错误：ReadDBGetWItemsFromOwner " + ex.ToString());
                return WItems.ToArray();
            }
        }


        /// <summary>
        /// 将WItem[]写入数据库，WItem[]包含了武器所有者
        /// </summary>
        /// <param name="WItem"></param>
        public bool WriteDB(WItem[] WItem)
        {
            if (WItem.Length == 0)
                return false;

            bool flag = true;
            foreach (var v in WItem)
            {
                if (v == null || v.Level == 0 || string.IsNullOrWhiteSpace(v.owner))
                    continue;

                try
                {   //如果没有，添加
                    if (ReadDBGetWItemsFromOwner(v.owner, v.id).Length == 0)
                    {
                        database.Query("INSERT INTO " + tableName + " (owner, itemName, itemID, lable, level, damage_level, scale_level, knockBack_level, useSpeed_level, shootSpeed_level, allCost) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10);", new object[]
                        {
                                v.owner,
                                Lang.GetItemNameValue(v.id),
                                v.id,
                                v.lable,
                                v.Level,
                                v.damage_level,
                                v.scale_level,
                                v.knockBack_level,
                                v.useSpeed_level,
                                v.shootSpeed_level,
                                v.allCost
                        });
                    }//如果有，更新
                    else
                    {
                        database.Query("UPDATE " + tableName + " SET lable = @0, level = @1, damage_level = @4, scale_level = @5, knockBack_level = @6, useSpeed_level = @7, shootSpeed_level = @8, allCost = @9 WHERE owner = @2 And itemID = @3;", new object[]
                        {
                                v.lable,
                                v.Level,
                                v.owner,
                                v.id,
                                v.damage_level,
                                v.scale_level,
                                v.knockBack_level,
                                v.useSpeed_level,
                                v.shootSpeed_level,
                                v.allCost
                        });
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：WriteDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：WriteDB " + ex.ToString());
                    Console.WriteLine("错误：WriteDB " + ex.ToString());
                    flag = false;
                }
            }
            return flag;
        }


        /// <summary>
        /// 将WItem写入数据库，WItem包含了武器所有者
        /// </summary>
        /// <param name="WItem"></param>
        public bool WriteDB(WItem? WItem)
        {
            if (WItem == null || WItem.Level <= 0 || string.IsNullOrWhiteSpace(WItem.owner))
                return false;

            try
            {   //如果没有，添加
                if (ReadDBGetWItemsFromOwner(WItem.owner, WItem.id).Length == 0)
                {
                    database.Query("INSERT INTO " + tableName + " (owner, itemName, itemID, lable, level, damage_level, scale_level, knockBack_level, useSpeed_level, shootSpeed_level, allCost) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10);", new object[]
                    {
                                WItem.owner,
                                Lang.GetItemNameValue(WItem.id),
                                WItem.id,
                                WItem.lable,
                                WItem.Level,
                                WItem.damage_level,
                                WItem.scale_level,
                                WItem.knockBack_level,
                                WItem.useSpeed_level,
                                WItem.shootSpeed_level,
                                WItem.allCost
                    });
                    return true;
                }//如果有，更新
                else
                {
                    database.Query("UPDATE " + tableName + " SET lable = @0, level = @1, damage_level = @4, scale_level = @5, knockBack_level = @6, useSpeed_level = @7, shootSpeed_level = @8, allCost = @9 WHERE owner = @2 And itemID = @3;", new object[]
                    {
                                WItem.lable,
                                WItem.Level,
                                WItem.owner,
                                WItem.id,
                                WItem.damage_level,
                                WItem.scale_level,
                                WItem.knockBack_level,
                                WItem.useSpeed_level,
                                WItem.shootSpeed_level,
                                WItem.allCost
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error("错误：WriteDB2 " + ex.ToString());
                TSPlayer.All.SendErrorMessage("错误：WriteDB2 " + ex.ToString());
                Console.WriteLine("错误：WriteDB2 " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 删除该玩家的升级的武器，id = 0 删除所有的，id != 0 删除那一个武器
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="ID"></param>
        public bool DeleteDB(string owner, int ID = 0)
        {
            try
            {
                string mess = ID == 0 ? "'" : "' And itemID = " + ID;
                database.Query("DELETE FROM " + tableName + " WHERE owner = '" + owner + mess);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error("错误：DeleteDB " + ex.ToString());
                TSPlayer.All.SendErrorMessage("错误：DeleteDB " + ex.ToString());
                Console.WriteLine("错误：DeleteDB " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// 删除所有数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteDBAll()
        {
            try
            {
                database.Query("DROP TABLE " + tableName);
                WeaponPlus.DB = new WeaponPlusDB(TShock.DB);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error("错误：DeleteDBAll " + ex.ToString());
                TSPlayer.All.SendErrorMessage("错误：DeleteDBAll " + ex.ToString());
                Console.WriteLine("错误：DeleteDBAll " + ex.ToString());
                return false;
            }
        }
    }
}
