using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;

namespace OMS.ToolConsole.Encryption
{
    class TestEncryption
    {
        /// <summary>
        /// 加密OMS历史数据
        /// </summary>
        public static void EncryptionHistoricalData(int index)
        {
            if (index == 1)
            {
                EncryptionTable<Customer>();
            }
            else if (index == 2)
            {
                EncryptionTable<OrderReceive>();
            }
            else if (index == 3)
            {
                EncryptionTable<OrderModify>();
            }
            else if (index == 4)
            {
                EncryptionTable<OrderReturn>();
            }
            else if (index == 5)
            {
                EncryptionTable<OrderBilling>();
            }
            else
            {
                Console.WriteLine("Missing Table!");
            }
        }


        private static void EncryptionTable<T>() where T : class
        {
            string direPath = AppDomain.CurrentDomain.BaseDirectory + "/Log/Encryption";
            var type = typeof(T);
            string logPath = $"{direPath}/{type.Name}.log";
            if (!Directory.Exists(direPath))
            {
                Directory.CreateDirectory(direPath);
            }
            Console.WriteLine($"Encryption  {type.Name} -----------------------------------------");

            //找出id属性
            var idProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower() == "id");

            //已经处理过的日志记录
            var existsIds = new string[0];
            if (File.Exists(logPath))
            {
                existsIds = File.ReadAllLines(logPath);
            }

            int _total = 0;
            //对表进行加密
            using (ebEntities db = new ebEntities())
            {
                int count = db.Set<T>().Count();
                int pageSize = 200;

                int pageTotal = (int)Math.Ceiling((decimal)count / pageSize);
                for (int pageIndex = 0; pageIndex <= pageTotal; pageIndex++)
                {
                    //分页查询
                    string sql = $"select * from [dbo].[{type.Name}]  order by id offset {pageSize * pageIndex} rows fetch next {pageSize} rows only";
                    var pageData = db.Database.SqlQuery<T>(sql);

                    var ids = pageData.Select(t => idProp.GetValue(t).ToString()).ToArray();
                    var exceptsIds = ids.Except(existsIds);  //计算差集,只对没有加密过的进行加密

                    //过滤已经上传过的
                    var filterData = pageData.Where(t => exceptsIds.Contains(idProp.GetValue(t).ToString())).ToList();

                    Console.WriteLine($"PageTotal:{pageTotal}  PageIndex:{pageIndex} PageSize:{pageSize}");
                    List<string> saveId = new List<string>();
                    foreach (var item in filterData)
                    {
                        var id = idProp.GetValue(item).ToString();

                        try
                        {
                            EncryptionFactory.Create(item).Encrypt(); //对数据进行加密
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            //fieldEncryption.EncryptionField(item); //对数据进行加密
                        }
                        saveId.Add(id);
                        _total++;
                    }
                    db.Set<T>().AddOrUpdate(filterData.ToArray());

                    try
                    {
                        db.SaveChanges();
                        //数据库保存成功之后,写入文件日志
                        File.AppendAllLines(logPath, saveId);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            };
            Console.WriteLine($"Encryption  {type.Name} End. Total:{_total}.");
        }
    }
}
