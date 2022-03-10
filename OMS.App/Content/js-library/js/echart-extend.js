/*
 * echartExtend.Charts相关操作
 * >>Init :初始化对象
 * >>Set: 创建图表
 * >>AjaxSet: Ajax方式创建图表
*/
var echartExtend = {};
var defaultStyle = 'shine';
// 语言
$.fn.echartExtend = {
    AjaxSet: {
        loadingText: 'Loading...'
    }
}


echartExtend.Charts = {
    Init: function (id) {
        return echarts.init(document.getElementById(id), defaultStyle)
    },
    Set: function (object, option) {
        object.setOption(option);
    },
    AjaxSet: function (object, objconfig) {
        var _url = (objconfig.url === undefined) ? "" : objconfig.url;
		var _para = (objconfig.para === undefined) ? {} : objconfig.para;
		var _func = (typeof (objconfig.func) === 'function') ? objconfig.func : null;
        $.ajax(
                {
                    url: _url,
                    type: 'post',
                    data: _para,
                    dataType: 'json',
                    timeout: 20000,
                    beforeSend: function (XMLHttpRequest) {
                        object.showLoading({ text: $.fn.echartExtend.AjaxSet.loadingText });
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        object.hideLoading();
                        artDialogExtend.Tips.Error($.fn.commonJs.sysAjaxFunction.errorText, 2);
                    },
                    success: function (data, textStatus) {
                        object.hideLoading();
                        if (_func) _func(data);
                    }
                });
    }
};