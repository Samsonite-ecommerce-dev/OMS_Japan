//ajax传递操作方法
/*
* url 地址
* para 参数
* isload 是否显示加载条
* func 执行的方法
*/
var commonJs = {};
// 语言
$.fn.commonJs = {
    sysAjaxFunction: {
        errorText: 'system error, please try again!'
    },
    openFileView: {
        title: 'Upload File'
    }
}
//ajax操作
commonJs.sysAjaxFunction = function (objconfig) {
    var _url = (objconfig.url === undefined) ? "" : objconfig.url;
	var _para = (objconfig.para === undefined) ? {} : objconfig.para;
    var _dataType = (objconfig.dataType === undefined) ? 'html' : objconfig.dataType;
    var _isload = (objconfig.isload === undefined) ? true : objconfig.isload;
	var _func = (typeof (objconfig.func) === 'function') ? objconfig.func : null;
	//CSRF验证
	if (verificationTokenJs.Enabled) {
		var _verificationToken = verificationTokenJs.getVerificationToken();
		_para.__RequestVerificationToken = _verificationToken;
	}
    $.ajax(
            {
                url: _url,
                type: 'post',
                data: _para,
                dataType: _dataType,
                timeout: 20000,
                beforeSend: function (XMLHttpRequest) {
                    if (_isload) artDialogExtend.Loading.Load();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    if (_isload) artDialogExtend.Loading.Close();
                    artDialogExtend.Tips.Error($.fn.commonJs.sysAjaxFunction.errorText, 2);
                },
                success: function (data, textStatus) {
                    if (_isload) artDialogExtend.Loading.Close();
                    if (_func) _func(data);
                }
            });
}

//获取单选框的值
commonJs.getRadioValue = function (name) {
    var _check = $('input[type=radio][name=' + name + ']:checked');
    return _check.eq(0).val();
}

//获取多选框的值
commonJs.getCheckBoxValue = function (name) {
    var _checks = [];
    $('input[type=checkbox][name=' + name + ']:checked').each(function () {
        _checks.push($(this).val());
    });
    return _checks;
}

//全部选中
commonJs.sysCheckBox = function (object, objName) {
    $("input[name=" + objName + "]").each(function () {
        if (!$(this).prop("disabled")) {
            $(this).prop("checked", (object.prop("checked") ? true : false));
        }
    });
}

//格式化货币
commonJs.formatCurrency = function (num) {
    var _result = '';
    num = num.toString().replace(/\$|\,/g, '');
    if (isNaN(num))
        num = '0';
    var sign = (num == (num = Math.abs(num)));
    num = Math.floor(num * 100 + 0.50000000001);
    var cents = num % 100;
    num = Math.floor(num / 100).toString();
    if (cents > 0 && cents < 10)
        cents = '0' + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++) {
        num = num.substring(0, num.length - (4 * i + 3)) + ',' + num.substring(num.length - (4 * i + 3));
    }
    if (cents == 0) {
        _result = (((sign) ? '' : '-') + num);
    }
    else {
        _result = (((sign) ? '' : '-') + num + '.' + cents);
    }
    return _result;
}

//生成随机密钥
commonJs.CreateKey = function (num) {
    var _result = '';
    var _keyArrays = new Array('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
    for (var t = 0; t < num; t++) {
        var _l = Math.floor(Math.random() * 2);
        var _k = (_l == 1) ? _keyArrays[Math.floor(Math.random() * 36)].toUpperCase() : _keyArrays[Math.floor(Math.random() * 36)];
        _result += _k;
    }
    return _result;
}

//上传文件
//objModel, obj, objWidth, objHeight
commonJs.openFileView = function (objconfig) {
    if (objconfig.title === undefined) objconfig.title = $.fn.commonJs.openFileView.title;
    if (objconfig.catalog === undefined) objconfig.catalog = '';
    if (objconfig.width === undefined) objconfig.width = '100%';
    if (objconfig.height === undefined) objconfig.height = '100%';
    //对当前input设置一个标识
    objconfig.object.parent().find('input[type="hidden"]').attr("title", "value");
    objconfig.object.parent().find('input[type="text"]').attr("title", "url");
    artDialogExtend.Dialog.Open('/Upload/Index?model=' + objconfig.model + '&catalog=' + objconfig.catalog, {
        title: objconfig.title,
        width: objconfig.width,
        height: objconfig.height
    });
}