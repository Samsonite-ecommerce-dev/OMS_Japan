using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class LanguageController : BaseController
    {
        //
        // GET: /Language/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _classid = VariableHelper.SaferequestInt(Request.Form["classid"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((PackKey like {0}) or (PackChinese like {0}) or (PackEnglish like {0}) or (PackKorean like {0}) or (PackThai like {0}))", Param = "%" + _keyword + "%" });
                }
                if (_classid != 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "FunctionID={0}", Param = _classid });
                }
                if (_isdelete == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=1", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=0", Param = null });
                }
                //查询
                var _list = db.GetPage<LanguagePack>("select * from LanguagePack order by FunctionID asc,SeqNumber asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = dy.PackKey,
                               s2 = dy.PackChinese,
                               s3 = dy.PackTraditionalChinese,
                               s4 = dy.PackEnglish,
                               s5 = dy.PackKorean,
                               s6 = dy.PackThai,
                               s7=dy.PackJapan,
                               s8 = string.Format("<a href=\"javascript: void(0)\" onclick=\"easyUIExtend.Grid.CommonOper($('#dg'),'" + Url.Action("Sort_Message", "Language") + "',{{id:{0},type:'U'}})\"><i class=\"fa fa-arrow-up color_success\"></i></a><a href=\"javascript: void(0)\" onclick=\"easyUIExtend.Grid.CommonOper($('#dg'),'" + Url.Action("Sort_Message", "Language") + "',{{id:{0},type:'D'}})\"><i class=\"fa fa-arrow-down color_success\"></i></a>", dy.ID),
                               s9 = (!dy.IsDelete) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                           }
                };
                return _result;
            }
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _FunctionID = VariableHelper.SaferequestInt(Request.Form["FunctionID"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["SortID"]);
            int _SeqNumberID = 0;

            //Key
            string _PackKeys = Request.Form["PackKey"];
            string _PackChineses = Request.Form["PackChinese"];
            string _PackTraditionalChineses = Request.Form["PackTraditionalChinese"];
            string _PackEnglishs = Request.Form["PackEnglish"];
            string _PackKoreans = Request.Form["PackKorean"];
            string _PackThais = Request.Form["PackThai"];
            string _PackJapans = Request.Form["PackJapan"];
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (_FunctionID == 0)
                        {
                            throw new Exception(_LanguagePack["language_edit_message_no_category"]);
                        }

                        string[] _PackKey_Array = _PackKeys.Split(',');
                        string[] _PackChinese_Array = (!string.IsNullOrEmpty(_PackChineses)) ? _PackChineses.Split(',') : null;
                        string[] _PackTraditionalChinese_Array = (!string.IsNullOrEmpty(_PackTraditionalChineses)) ? _PackTraditionalChineses.Split(',') : null;
                        string[] _PackEnglish_Array = (!string.IsNullOrEmpty(_PackEnglishs)) ? _PackEnglishs.Split(',') : null;
                        string[] _PackKorean_Array = (!string.IsNullOrEmpty(_PackKoreans)) ? _PackKoreans.Split(',') : null;
                        string[] _PackThai_Array = (!string.IsNullOrEmpty(_PackThais)) ? _PackThais.Split(',') : null;
                        string[] _PackJapan_Array = (!string.IsNullOrEmpty(_PackJapans)) ? _PackJapans.Split(',') : null;
                        string _key = string.Empty;
                        //要插队的排序号
                        int _N_SeqNumberID = 0;
                        for (int t = 0; t < _PackKey_Array.Length; t++)
                        {
                            _key = _PackKey_Array[t];
                            if (string.IsNullOrEmpty(_key))
                            {
                                throw new Exception(_LanguagePack["language_edit_message_no_key"]);
                            }
                            else
                            {
                                LanguagePack objLanguagePack = db.LanguagePack.Where(p => p.PackKey == _key).SingleOrDefault();
                                if (objLanguagePack != null)
                                {
                                    throw new Exception(_LanguagePack["language_edit_message_repeat_category"]);
                                }
                            }

                            if (_SortID == 0)
                            {
                                if (_SeqNumberID == 0)
                                {
                                    _SeqNumberID = db.Database.SqlQuery<int>("select isnull(Max(SeqNumber),0) as MaxID from LanguagePack where FunctionID={0}", _FunctionID).SingleOrDefault() + 1;
                                }
                                else
                                {
                                    _SeqNumberID++;
                                }
                            }
                            else
                            {
                                if (_SeqNumberID == 0)
                                {
                                    LanguagePack objLanguagePack1 = db.LanguagePack.Where(p => p.ID == _SortID && p.FunctionID == _FunctionID).SingleOrDefault();
                                    if (objLanguagePack1 != null)
                                    {
                                        _N_SeqNumberID = objLanguagePack1.SeqNumber;
                                        _SeqNumberID = _N_SeqNumberID + 1;
                                    }
                                }
                                else
                                {
                                    _SeqNumberID++;
                                }
                            }

                            LanguagePack objData = new LanguagePack()
                            {
                                FunctionID = _FunctionID,
                                PackKey = _PackKey_Array[t],
                                PackChinese = (_PackChinese_Array != null) ? _PackChinese_Array[t] : "",
                                PackTraditionalChinese = (_PackTraditionalChinese_Array != null) ? _PackTraditionalChinese_Array[t] : "",
                                PackEnglish = (_PackEnglish_Array != null) ? _PackEnglish_Array[t] : "",
                                PackKorean = (_PackKorean_Array != null) ? _PackKorean_Array[t] : "",
                                PackThai = (_PackThai_Array != null) ? _PackThai_Array[t] : "",
                                PackJapan = (_PackJapan_Array != null) ? _PackJapan_Array[t] : "",
                                SeqNumber = _SeqNumberID,
                                IsDelete = false
                            };
                            db.LanguagePack.Add(objData);
                        }
                        //如果是插队添加
                        if (_SortID > 0)
                        {
                            db.Database.ExecuteSqlCommand("update LanguagePack set SeqNumber=SeqNumber+{2} where FunctionID={0} and SeqNumber>{1}", _FunctionID, _N_SeqNumberID, _PackKey_Array.Length);
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                    return _result;
                }
            }
        }
        #endregion

        #region 编辑

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _data = Request.Form["data"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(_data))
                    {
                        List<LanguagePackData> objData = JsonHelper.JsonDeserialize<List<LanguagePackData>>(_data);
                        List<Int64> _Ids = objData.Select(p => p.id).ToList();
                        List<LanguagePack> objLanguagePack_List = db.LanguagePack.Where(p => _Ids.Contains(p.ID)).ToList();
                        LanguagePackData _d = new LanguagePackData();
                        foreach (var objLanguagePack in objLanguagePack_List)
                        {
                            _d = objData.Where(p => p.id == objLanguagePack.ID).SingleOrDefault();
                            objLanguagePack.PackChinese = _d.cn;
                            objLanguagePack.PackTraditionalChinese = _d.cn_tw;
                            objLanguagePack.PackEnglish = _d.en;
                            objLanguagePack.PackKorean = _d.ko;
                            objLanguagePack.PackThai = _d.th;
                            objLanguagePack.PackJapan = _d.jpn;
                        }
                        db.SaveChanges();
                        //编辑日志
                        AppLogService.UpdateLog<LanguagePack>(objLanguagePack_List, string.Join(",", objLanguagePack_List.Select(p => p.ID).ToList()));
                    }
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_save_success"]
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }

        private class LanguagePackData
        {
            public Int64 id { get; set; }
            public string cn { get; set; }
            public string cn_tw { get; set; }
            public string en { get; set; }
            public string ko { get; set; }
            public string th { get; set; }
            public string jpn { get; set; }
        }
        #endregion

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        LanguagePack objLanguagePack = new LanguagePack();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objLanguagePack = db.LanguagePack.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objLanguagePack != null)
                            {
                                objLanguagePack.IsDelete = true;
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("LanguagePack", _IDs);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_delete_success"]
                        };

                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        #region 恢复
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Restore_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        LanguagePack objLanguagePack = new LanguagePack();
                        foreach (string _str in _IDs.Split(','))
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            objLanguagePack = db.LanguagePack.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objLanguagePack != null)
                            {
                                objLanguagePack.IsDelete = false;
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("LanguagePack", _IDs);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_recover_success"]
                        };

                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        #region 排序
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Sort_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["ID"]);
            string _Type = VariableHelper.SaferequestStr(Request.Form["Type"]);
            using (var db = new ebEntities())
            {

                try
                {
                    int _NewSeqNumber = 0;
                    LanguagePack objLanguagePack = db.LanguagePack.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objLanguagePack != null)
                    {
                        if (_Type.ToUpper() == "U")
                        {
                            LanguagePack objLanguagePack_U = db.LanguagePack.Where(p => p.FunctionID == objLanguagePack.FunctionID && p.SeqNumber < objLanguagePack.SeqNumber).OrderByDescending(p => p.SeqNumber).FirstOrDefault();
                            if (objLanguagePack_U != null)
                            {
                                _NewSeqNumber = objLanguagePack_U.SeqNumber;
                                //交换排序号
                                objLanguagePack_U.SeqNumber = objLanguagePack.SeqNumber;
                                objLanguagePack.SeqNumber = _NewSeqNumber;
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["language_sort_message_on_top"]);
                            }
                        }
                        else if (_Type.ToUpper() == "D")
                        {
                            LanguagePack objLanguagePack_D = db.LanguagePack.Where(p => p.FunctionID == objLanguagePack.FunctionID && p.SeqNumber > objLanguagePack.SeqNumber).OrderBy(p => p.SeqNumber).FirstOrDefault();
                            if (objLanguagePack_D != null)
                            {
                                _NewSeqNumber = objLanguagePack_D.SeqNumber;
                                //交换排序号
                                objLanguagePack_D.SeqNumber = objLanguagePack.SeqNumber;
                                objLanguagePack.SeqNumber = _NewSeqNumber;
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["language_sort_message_on_bottom"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["common_parameter_error"]);
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("common_data_load_false");
                    }
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_save_success"]
                    };

                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
            }
            return _result;
        }
        #endregion

        #region 重置语言包缓存
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Reset_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            try
            {
                LanguageCache.Instance.Reset();
                //返回信息
                _result.Data = new
                {
                    result = true,
                    msg = _LanguagePack["common_data_save_success"]
                };
            }
            catch (Exception ex)
            {
                _result.Data = new
                {
                    result = false,
                    msg = ex.Message
                };
            }
            return _result;
        }
        #endregion

        #region 分类
        /// <summary>
        /// 功能组合菜单
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize(Type = BaseAuthorize.ResultType.Json)]
        public ContentResult GroupFunction_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            List<SysFunctionGroup> objSysFunctionGroup_List = FunctionService.GetFunctionGroupObject();
            List<SysFunction> objSysFunction_List = FunctionService.GetFunctionObject();
            List<DefineGroupComboBox> objDefineGroupComboBox_List = new List<DefineGroupComboBox>();
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "0",
                Text = "--ALL--",
                Selected = true
            });
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "-999",
                Text = "Home",
                Selected = false
            });
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "-998",
                Text = "Login",
                Selected = false
            });
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "-997",
                Text = "Upload",
                Selected = false
            });
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "-1",
                Text = "Menu",
                Selected = false
            });
            foreach (var _sfg in objSysFunctionGroup_List)
            {
                foreach (var _sf in objSysFunction_List.Where(p => p.Groupid == _sfg.Groupid))
                {
                    objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
                    {
                        Value = _sf.Funcid.ToString(),
                        Text = _LanguagePack[string.Format("menu_function_{0}", _sf.Funcid)],
                        Group = _LanguagePack[string.Format("menu_group_{0}", _sfg.Groupid)],
                        Selected = false
                    });
                }
            }
            objDefineGroupComboBox_List.Add(new DefineGroupComboBox()
            {
                Value = "999",
                Text = "Common",
                Selected = false
            });
            _result.Content = JsonHelper.JsonSerialize(objDefineGroupComboBox_List);
            return _result;
        }

        /// <summary>
        /// 功能组合菜单
        /// </summary>
        /// <returns></returns>
        public JsonResult LanguagePack_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _key = VariableHelper.SaferequestStr(Request.Form["q"]);
            int _fid = VariableHelper.SaferequestInt(Request.Form["fid"]);
            int _perpage = VariableHelper.SaferequestInt(Request.Form["rows"]);
            if (_perpage <= 0) _perpage = 20;
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            if (_page <= 0) _page = 1;
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_key))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "PackKey like {0}", Param = "%" + _key + "%" });
                }

                if (Request.Form["fid"] != null)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "FunctionID = {0}", Param = _fid });
                }

                //查询
                var _list = db.GetPage<dynamic>("select ID,PackKey,SeqNumber from LanguagePack order by FunctionID asc,SeqNumber asc", _SqlWhere, _perpage, _page);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s0 = dy.ID,
                               s1 = dy.PackKey,
                               s2 = dy.SeqNumber
                           }
                };
            }
            return _result;
        }
        #endregion
    }
}
