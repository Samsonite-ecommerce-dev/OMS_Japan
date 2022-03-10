//扩展方法
$.extend({
    requestQueryString: function (key) {
        var _data = {};
        var aPairs, aTmp;
        var queryString = new String(window.location.search);
        queryString = queryString.substr(1, queryString.length);
        aPairs = queryString.split("&");
        for (var i = 0; i < aPairs.length; i++) {
            aTmp = aPairs[i].split("=");
            _data[aTmp[0]] = aTmp[1];
        }
        return _data[key];
    }
});

//扩展对象
$.fn.extend({
    //在字符串中插入字符
    insertAtCaret: function (myValue) {
        var $t = $(this)[0];
        if (document.selection) {
            this.focus();
            sel = document.selection.createRange();
            sel.text = myValue;
            this.focus();
        }
        else
            if ($t.selectionStart || $t.selectionStart == '0') {
                var startPos = $t.selectionStart;
                var endPos = $t.selectionEnd;
                var scrollTop = $t.scrollTop;
                $t.value = $t.value.substring(0, startPos) + myValue + $t.value.substring(endPos, $t.value.length);
                this.focus();
                $t.selectionStart = startPos + myValue.length;
                $t.selectionEnd = startPos + myValue.length;
                $t.scrollTop = scrollTop;
            }
            else {
                this.value += myValue;
                this.focus();
            }
    }
});

/*****************************  alan.wang  Jquery 插件   ***************************************/

/**
 * @author alan wang
 * @description jquery 表单把form 对象系列化成json 对象
 * @returns { Json } 
 */

//$.fn.serializeJson = function () {
//    var serializeObj = {};
//    var array = this.serializeArray();
//    var str = this.serialize();
//    $(array).each(function () {
//        if (serializeObj[this.name]) {
//            if ($.isArray(serializeObj[this.name])) {
//                serializeObj[this.name].push(this.value);
//            } else {
//                serializeObj[this.name] = [serializeObj[this.name], this.value];
//            }
//        } else {
//            serializeObj[this.name] = this.value;
//        }
//    });
//    return serializeObj;
//};





/*  
    序列化表单数据到JSON对象  
*/
(function ($) {
    $.fn.serializeJson = function () {
        var serializeObj = {};
        var array = this.serializeArray();
        var str = this.serialize();
        $(array).each(function () {
            if (serializeObj[this.name]) {
                if ($.isArray(serializeObj[this.name])) {
                    serializeObj[this.name].push(this.value);
                } else {
                    serializeObj[this.name] = [serializeObj[this.name], this.value];
                }
            } else {
                serializeObj[this.name] = this.value;
            }
        });
        return serializeObj;
    };
})(jQuery);

/*  
   JSON对象根据名称自动赋值到表单
*/
(function ($) {
    $.fn.deserializeJson = function (json) {
        for (var key in json) {
            $(":input[name='" + key + "']:eq(0)", this).val(json[key]);
            $(":input[name='" + key + "']:eq(0)", this).attr('Value', json[key]);
        }
    };
})(jQuery);

/*  
   清除表单数据
*/
(function ($) {
    $.fn.clearValue = function () {
        $(':input', this).each(function () {
            var type = this.type;
            var tag = this.tagName.toLowerCase(); // normalize case
            // it's ok to reset the value attr of text inputs,
            // password inputs, and textareas
            if (type == 'text' || type == 'password' || tag == 'textarea')
                this.value = "";
                // checkboxes and radios need to have their checked state cleared
                // but should *not* have their 'value' changed
            else if (type == 'checkbox' || type == 'radio')
                this.checked = false;
                // select elements need to have their 'selectedIndex' property set to -1
                // (this works for both single and multiple select elements)
            else if (tag == 'select')
                this.selectedIndex = -1;
        });
    };
})(jQuery);