using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public class CommonResult
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalRecord { get; set; }

        /// <summary>
        /// 成功数
        /// </summary>
        public int SuccessRecord { get; set; }

        /// <summary>
        /// 失败数
        /// </summary>
        public int FailRecord { get; set; }

        /// <summary>
        /// 生成的文件
        /// </summary>
        public string FileName { get; set; }
    }

    /// <summary>
    /// 返回结果
    /// </summary>
    public class CommonResult<T>
    {
        private List<CommonResultData<T>> _resultData = new List<CommonResultData<T>>();
        /// <summary>
        /// 总数
        /// </summary>
        public List<CommonResultData<T>> ResultData
        {
            get { return _resultData; }
            set { _resultData = value; }
        }

        private string _fileName = string.Empty;
        /// <summary>
        /// 生成的文件
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
    }

    public class CommonResultData<T>
    {
        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 信息描述
        /// </summary>
        public string ResultMessage { get; set; }
    }

    /// <summary>
    /// 生成DW
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExportResult<T>
    {
        /// <summary>
        /// 生成的XMl文件
        /// </summary>
        public string XML { get; set; }

        /// <summary>
        /// 成功数据
        /// </summary>
        public List<T> SuccessData { get; set; }

        /// <summary>
        /// 失败数据
        /// </summary>
        public List<T> FailData { get; set; }
    }

    /// <summary>
    /// ftp返回文件
    /// </summary>
    public class FTPResult
    {
        /// <summary>
        /// 成功文件
        /// </summary>
        public List<string> SuccessFile { get; set; }

        /// <summary>
        /// 失败文件
        /// </summary>
        public List<string> FailFile { get; set; }
    }
}
