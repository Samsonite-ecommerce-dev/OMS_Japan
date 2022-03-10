using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Samsonite.OMS.DTO
{

    #region 菜单栏
    public class DefineMenu
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public List<MenuChild> Children { get; set; }

        public class MenuChild
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public string Url { get; set; }

            public string Target { get; set; }
        }
    }
    #endregion

    #region 权限
    public class DefineUserPower
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
    #endregion

    #region 结果集
    public class DefineResult
    {
        public bool Success { get; set; }

        public string ErrMessage { get; set; }
    }
    #endregion 

    #region Enum
    public class DefineEnum
    {
        /// <summary>
        /// id
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 显示值
        /// </summary>
        public string Display { get; set; }
        /// <summary>
        /// 样式
        /// </summary>
        public string Css { get; set; }
    }
    #endregion

    #region 通用框
    public class DefineComboBox
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "selected")]
        public bool Selected { get; set; }
    }

    public class DefineGroupComboBox
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        [JsonProperty(PropertyName = "selected")]
        public bool Selected { get; set; }
    }

    public class DefineComboTree
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "iconCls")]
        public string IconCls { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<Children> Childrens { get; set; }


        public class Children
        {
            [JsonProperty(PropertyName = "id")]
            public string ID { get; set; }

            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "iconCls")]
            public string IconCls { get; set; }
        }
    }

    public class DefineChooseBox
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class DefineDataList
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }
    }

    public class DefineTree
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<TreeChildren> Children { get; set; }

        public class TreeChildren
        {
            [JsonProperty(PropertyName = "id")]
            public string ID { get; set; }

            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }
        }
    }
    #endregion

}
