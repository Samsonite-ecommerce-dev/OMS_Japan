using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class EmailService
    {
        /// <summary>
        /// 添加邮件到待发送邮件表
        /// </summary>
        /// <param name="objSMMailSend"></param>
        public static void SendEmail(SMMailSend objSMMailSend)
        {
            using (var db = new ebEntities())
            {
                try
                {
                    db.SMMailSend.Add(objSMMailSend);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
