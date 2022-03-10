using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class UploadController : BaseController
    {
        //
        // GET: /Upload/

        #region 查询
        [UserLoginAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            string _Model = VariableHelper.SaferequestStr(Request.QueryString["model"]);
            string _Catalog = VariableHelper.SaferequestStr(Request.QueryString["catalog"]);

            using (var db = new ebEntities())
            {
                SysUploadModel objSysUploadModel = db.SysUploadModel.Where(p => p.ModelMark == _Model).SingleOrDefault();
                if (objSysUploadModel != null)
                {
                    if (!string.IsNullOrEmpty(_Catalog))
                    {
                        objSysUploadModel.SaveCatalog = _Catalog;
                    }
                    return View(objSysUploadModel);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();

            List<FileCollection> objFiles = new List<FileCollection>();
            string _directoryPath = AppGlobalService.UPLOAD_FILE_PATH;
            string _filepath = VariableHelper.SaferequestNull(Request.Form["filepath"]);
            string _path = string.Empty;

            //默认根目录
            if (string.IsNullOrEmpty(_filepath))
            {
                _path = $"{_directoryPath}/";
            }
            else
            {
                //过滤../参数
                if (_filepath.IndexOf("..") > -1)
                    _filepath = _filepath.Replace(".", "");
                if (_filepath.IndexOf("/") == 0)
                    _filepath = _filepath.Substring(1);

                _path = $"{_directoryPath}/{_filepath}/";
            }
            //如果不存在默认为根目录
            if (!Directory.Exists(Server.MapPath(_path)))
            {
                _path = $"{_directoryPath}/";
            }
            //读取文件
            DirectoryInfo _dir = new DirectoryInfo(Server.MapPath(_path));
            //目录列表
            foreach (var _d in _dir.GetDirectories().OrderByDescending(p => p.LastWriteTime))
            {
                objFiles.Add(new FileCollection()
                {
                    FileName = $"<i class=\"fa fa-folder-open color_warning\"></i><a href=\"javascript:void(0);\" onclick=\"SearchNextPath('{(!string.IsNullOrEmpty(_filepath) ? _filepath + "/" + _d.Name : _d.Name)}')\">{_d.Name}</a>",
                    FileExt = "folder",
                    FileSize = "",
                    EditTime = _d.LastWriteTime
                });
            }
            //文件列表
            foreach (var _f in _dir.GetFiles().OrderByDescending(p => p.LastWriteTime))
            {
                objFiles.Add(new FileCollection()
                {
                    FileName = $"<a href=\"javascript:void(0);\" onclick=\"SelectFile('{_path}{_f.Name}')\">{GetFileName(_path, _f)}</a>",
                    FileExt = _f.Extension.Replace(".", ""),
                    FileSize = VariableHelper.FormatSize(_f.Length),
                    EditTime = _f.LastWriteTime
                });
            }

            _result.Data = from dy in objFiles.OrderByDescending(p => p.EditTime)
                           select new
                           {
                               s1 = dy.FileName,
                               s2 = dy.FileExt,
                               s3 = dy.FileSize,
                               s4 = dy.EditTime.ToString("yyyy-MM-dd HH:mm")
                           };

            return _result;
        }
        #endregion

        #region 保存文件
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult Save_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<string> _Files = new List<string>();
            //上传目录
            string _directoryPath = AppGlobalService.UPLOAD_FILE_PATH;
            string _filePath = string.Empty;

            string _Model = VariableHelper.SaferequestStr(Request.Form["model"]);
            string _Catalog = VariableHelper.SaferequestStr(Request.Form["catalog"]);

            using (var db = new ebEntities())
            {
                try
                {
                    SysUploadModel objSysUploadModel = db.SysUploadModel.Where(p => p.ModelMark == _Model).SingleOrDefault();
                    if (objSysUploadModel != null)
                    {
                        if (!string.IsNullOrEmpty(_Catalog))
                        {
                            objSysUploadModel.SaveCatalog = _Catalog;
                        }

                        if (Request.Files.Count == 0)
                        {
                            throw new Exception("Please select a file to upload!");
                        }
                        int _FileSize = 0;
                        string _O_FileName = string.Empty;
                        string _O_FileExt = string.Empty;
                        string _N_FileName = string.Empty;
                        int i = 0;
                        if (Request.Files.Count <= objSysUploadModel.MaxFileCount)
                        {
                            if (objSysUploadModel.SaveStyle == "fileorder")
                            {
                                _filePath = $"/{ objSysUploadModel.SaveCatalog}";
                            }
                            else
                            {
                                _filePath = $"/{ objSysUploadModel.SaveCatalog}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                            }
                            _directoryPath = $"{_directoryPath}{_filePath}/";
                            //创建目录
                            if (!Directory.Exists(Server.MapPath(_directoryPath))) Directory.CreateDirectory(Server.MapPath(_directoryPath));
                            //循环上传文件
                            for (int t = 0; t < Request.Files.Count; t++)
                            {
                                _FileSize = Request.Files[t].ContentLength;
                                if (_FileSize > 0)
                                {
                                    _O_FileName = Request.Files[t].FileName;
                                    if (_FileSize <= objSysUploadModel.MaxFileSize)
                                    {
                                        i = _O_FileName.LastIndexOf(".");
                                        _O_FileExt = _O_FileName.Substring(i + 1).ToLower();
                                        if (("|" + objSysUploadModel.AllowFile + "|").ToUpper().IndexOf("|" + _O_FileExt.ToUpper() + "|") > -1)
                                        {
                                            //是否重命名
                                            if (objSysUploadModel.IsRename)
                                            {
                                                _N_FileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{Request.Files[t].ContentLength.ToString()}.{_O_FileExt}";
                                            }
                                            else
                                            {
                                                _N_FileName = _O_FileName.Substring(_O_FileName.LastIndexOf("\\") + 1);
                                            }
                                            //添加到文件集合
                                            _Files.Add(_directoryPath + _N_FileName);
                                            //保存文件
                                            Request.Files[t].SaveAs(Server.MapPath(_directoryPath + _N_FileName));
                                        }
                                        else
                                        {
                                            throw new Exception($"The format of ({_O_FileExt}) is not allowed to upload!");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception($"The size of files must be less than {objSysUploadModel.MaxFileSize}!");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"The quantity of files must be less than {objSysUploadModel.MaxFileCount}!");
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }

                    //返回信息
                    _result.Data = new
                    {
                        filename = string.Join("|", _Files),
                        filepath = _filePath,
                        result = true,
                        msg = string.Empty
                    };
                }
                catch (Exception ex)
                {
                    //返回信息
                    _result.Data = new
                    {
                        filename = string.Empty,
                        filepath = string.Empty,
                        result = true,
                        msg = ex.Message
                    };
                }
            }

            return _result;
        }
        #endregion

        /// <summary>
        /// 格式化文件名称
        /// </summary>
        /// <param name="objFilePath"></param>
        /// <param name="objFileInfo"></param>
        /// <returns></returns>
        private string GetFileName(string objFilePath, FileInfo objFileInfo)
        {
            string _result = string.Empty;
            string _ext = objFileInfo.Extension.ToUpper();
            if (_ext == ".JPG" || _ext == ".JPEG" || _ext == ".GIF" || _ext == ".PNG" || _ext == ".BMP")
            {
                _result = $"<img src=\"{objFilePath}{objFileInfo.Name}\" style=\"width:50px;height:50px;border:1px silver solid;border-radius:3px;margin:1px;padding:1px;\" />{ objFileInfo.Name}";
            }
            else
            {
                _result = objFileInfo.Name;
            }
            return _result;
        }

        private class FileCollection
        {
            public string FileName { get; set; }

            public string FileExt { get; set; }

            public string FileSize { get; set; }

            public DateTime EditTime { get; set; }
        }
    }
}
