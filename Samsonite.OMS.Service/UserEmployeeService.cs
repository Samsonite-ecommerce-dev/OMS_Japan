using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class UserEmployeeService
    {
        #region 等级组
        /// <summary>
        /// 获取等级组列表
        /// </summary>
        /// <returns></returns>
        public static List<UserEmployeeLevel> GetLevelOption()
        {
            using (var db = new ebEntities())
            {
                return db.UserEmployeeLevel.ToList();
            }
        }

        /// <summary>
        /// 根据关键字获取ID
        /// </summary>
        /// <param name="objKey"></param>
        /// <returns></returns>
        public static int GetLevelID(string objKey)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                List<UserEmployeeLevel> objUserEmployeeLevelList = db.UserEmployeeLevel.ToList();
                UserEmployeeLevel objUserEmployeeLevel = new UserEmployeeLevel();
                //是否存在,如果不存在则取默认ID
                objUserEmployeeLevel = objUserEmployeeLevelList.Where(p => p.LevelKey == objKey).FirstOrDefault();
                if (objUserEmployeeLevel != null)
                {
                    _result = objUserEmployeeLevel.LevelID;
                }
                else
                {
                    objUserEmployeeLevel = objUserEmployeeLevelList.Where(p => p.IsDefault).SingleOrDefault();
                    if (objUserEmployeeLevel != null)
                    {
                        _result = objUserEmployeeLevel.LevelID;
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 时间组
        /// <summary>
        /// 获取时间组列表
        /// </summary>
        /// <returns></returns>
        public static List<UserEmployeeGroup> GetGroupOption()
        {
            using (var db = new ebEntities())
            {
                return db.UserEmployeeGroup.ToList();
            }
        }

        /// <summary>
        /// 获取时间组ID
        /// </summary>
        /// <param name="objTime"></param>
        /// <param name="objDb"></param>
        /// <returns></returns>
        public static int GetDataGroupID(DateTime objTime, ebEntities objDb = null)
        {
            int _result = 0;
            if (objDb == null) objDb = new ebEntities();

            DateTime _BeginDate = Convert.ToDateTime(DateTime.Today.ToString("yyyy-01-01"));
            DateTime _EndDate = Convert.ToDateTime(DateTime.Today.ToString("yyyy-12-31"));
            UserEmployeeGroup objUserEmployeeGroup = objDb.UserEmployeeGroup.Where(p => p.BeginDate == _BeginDate && p.EndDate == _EndDate).SingleOrDefault();
            if (objUserEmployeeGroup != null)
            {
                _result = objUserEmployeeGroup.ID;
            }
            else
            {
                objUserEmployeeGroup = new UserEmployeeGroup()
                {
                    EmployeeGroup = $"{_BeginDate.ToString("yyyy/MM/dd")}-{_EndDate.ToString("yyyy/MM/dd")}",
                    BeginDate = _BeginDate,
                    EndDate = _EndDate,
                    Remark = string.Empty
                };
                objDb.UserEmployeeGroup.Add(objUserEmployeeGroup);
                objDb.SaveChanges();
                _result = objUserEmployeeGroup.ID;
            }
            return _result;
        }
        #endregion

        #region 保存员工信息
        /// <summary>
        /// 转换成员工信息列表
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static List<EmployeeImportDto> ConvertToUserEmployees(string excelPath)
        {
            List<EmployeeImportDto> objs = new List<EmployeeImportDto>();
            ExcelHelper helper = new ExcelHelper(excelPath);
            var table = helper.ExcelToDataTable("Sheet1");
            foreach (DataRow row in table.Rows)
            {
                string _email = VariableHelper.SaferequestStr(row[1].ToString());
                if (!string.IsNullOrEmpty(_email))
                {
                    var obj = new EmployeeImportDto()
                    {
                        Name = VariableHelper.SaferequestStr(row[0].ToString()),
                        Email = VariableHelper.SaferequestStr(row[1].ToString()),
                        LevelKey = VariableHelper.SaferequestStr(row[2].ToString()),
                        Effect = VariableHelper.SaferequestIntToBool(row[3].ToString()),
                        Result = true,
                        ResultMsg = string.Empty
                    };
                    objs.Add(obj);
                }
            }
            return objs;
        }


        /// <summary>
        /// 保存员工信息
        /// </summary>
        /// <param name="objUserEmployeeLevelList"></param>
        /// <param name="objDataGroupID"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ItemResult SaveUserEmployees(List<UserEmployeeLevel> objUserEmployeeLevelList, int objDataGroupID, EmployeeImportDto item)
        {
            ItemResult _result = new ItemResult();
            using (var db = new ebEntities())
            {
                StringBuilder sb = new StringBuilder();
                try
                {
                    if (string.IsNullOrEmpty(item.Name)) throw new Exception($"The Name can not be empty!");
                    if (string.IsNullOrEmpty(item.Email)) throw new Exception($"The Email can not be empty!");

                    //限制组
                    UserEmployeeLevel objUserEmployeeLevel = objUserEmployeeLevelList.Where(p => p.LevelKey == item.LevelKey).SingleOrDefault();
                    //如果不存在,则取默认值
                    if (objUserEmployeeLevel == null)
                    {
                        objUserEmployeeLevel = objUserEmployeeLevelList.Where(p => p.IsDefault).SingleOrDefault();
                    }

                    item.Name = EncryptionBase.EncryptString(item.Name);
                    item.Email = EncryptionBase.EncryptString(item.Email);
                    UserEmployee objData = db.UserEmployee.Where(p => p.EmployeeEmail == item.Email).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.EmployeeName = item.Name;
                        objData.EmployeeEmail = item.Email;
                        objData.LevelID = objUserEmployeeLevel.LevelID;
                        objData.Remark = string.Empty;
                        objData.IsLock = !item.Effect;
                        objData.EditTime = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        UserEmployee objUserEmployee = new UserEmployee()
                        {
                            EmployeeName = item.Name,
                            EmployeeEmail = item.Email,
                            LevelID = objUserEmployeeLevel.LevelID,
                            CurrentAmount = 0,
                            CurrentQuantity = 0,
                            DataGroupID = objDataGroupID,
                            Remark = string.Empty,
                            IsLock = !item.Effect,
                            AddTime = DateTime.Now,
                            EditTime = null
                        };
                        //数据加密
                        EncryptionFactory.Create(objUserEmployee).Encrypt();
                        //如果是添加,则需要检查时间组
                        //添加员工信息
                        db.UserEmployee.Add(objUserEmployee);
                        db.SaveChanges();
                    }
                    //返回信息
                    _result.Result = true;
                    _result.Message = string.Empty;
                }
                catch (Exception ex)
                {
                    _result.Result = false;
                    _result.Message = ex.Message;
                }
            }
            return _result;
        }
        #endregion
    }
}
