/*
 * artDialogExtend.Dialog
 * >>Open 打开iframe窗口
 * >>Close 关闭窗口
 * artDialogExtend.Content
 * >>Open 打开窗口
 * >>Close 关闭窗口
 * artDialogExtend.Comfirm 确认窗口
 * artDialogExtend.Message 提示窗口
 * >>Success
 * >>Alert
 * >>Error
 * artDialogExtend.Tips 提示标签
 * >>Success
 * >>Alert
 * >>Error
 * artDialogExtend.Loading loading窗口
 * >>Load
 * >>Close
 * artDialogExtend.Notice 右下角滑动通知
 */

var artDialogExtend = {};
// 语言
$.fn.artDialogExtend = {
    sysTipTitle: 'System alert',
    Loading: {
        Load: {
            loadText: 'Loading...'
        }
    },
    Confirm: {
        okText: 'OK',
        cancelText: 'Cancel'
    }
}
// iframe弹窗
artDialogExtend.Dialog = {
    // 打开窗口
    Open: function (url, config) {
        config = config || {};
        // 弹出框iframe中的datagrid不能取到正确宽度
        if (config.postWidth) {
            var _postWidth = (config.width === undefined) ? $(document).width() : config.width;
            var _postHeight = (config.height === undefined) ? $(document).height() : config.height;
            if (config.width == '100%') _postWidth = $(document).width();
            if (config.height == '100%') _postHeight = $(document).height();
            if (url.indexOf('?') > -1) {
                url += "&iwidth=" + (_postWidth - 6) + "&iheight=" + (_postHeight - 10);
            } else {
                url += "?iwidth=" + (_postWidth - 6) + "&iheight=" + (_postHeight - 10);
            }
        }
        artDialog.open(url, {
            title: config.title,
            width: config.width,
            height: config.height,
            okVal: (config.okVal === undefined) ? '' : config.okVal,
            ok: (typeof (config.ok) === 'function') ? config.ok : null,
            cancelVal: (config.cancelVal === undefined) ? '' : config.cancelVal,
            cancel: (typeof (config.cancel) === 'function') ? config.cancel : null,
            fixed: (config.fixed === undefined) ? true : config.fixed,
            lock: (config.lock === undefined) ? true : config.lock,
            opacity: 0.5,
            resize: (config.resize === undefined) ? false : config.resize,
            drag: (config.drag === undefined) ? true : config.drag,
            esc: (config.esc === undefined) ? true : config.esc,
            close: (typeof (config.close) === 'function') ? config.close : null
        }, false);
    },
    // 关闭窗口
    Close: function () {
        $.each(art.dialog.list, function () {
            this.close();
        });
    }
}

// 普通弹窗
artDialogExtend.Content = {
    // 打开窗口
    Open: function (config) {
        config = config || {};
        artDialog({
            title: config.title,
            content: config.content,
            width: config.width,
            height: config.height,
            okVal: (config.okVal === undefined) ? '' : config.okVal,
            ok: (typeof (config.ok) === 'function') ? config.ok : null,
            cancelVal: (config.cancelVal === undefined) ? '' : config.cancelVal,
            cancel: (typeof (config.cancel) === 'function') ? config.cancel : null,
            fixed: (config.fixed === undefined) ? true : config.fixed,
            lock: (config.lock === undefined) ? true : config.lock,
            opacity: 0.5,
            resize: (config.resize === undefined) ? false : config.resize,
            drag: (config.drag === undefined) ? true : config.drag,
            esc: (config.esc === undefined) ? true : config.esc,
            close: (typeof (config.close) === 'function') ? config.close : null
        }, false);
    },
    // 关闭窗口,true父窗体
    Close: function () {
        $.each(art.dialog.list, function () {
            this.close();
        });
    }
}

//Confirm
artDialogExtend.Confirm = function (content, yes, no) {
    return artDialog({
        id: 'Confirm',
        title: $.fn.artDialogExtend.sysTipTitle,
        icon: 'question',
        fixed: true,
        lock: true,
        opacity: .1,
        content: content,
        okVal: $.fn.artDialogExtend.Confirm.okText,
        cancelVal: $.fn.artDialogExtend.Confirm.cancelText,
        ok: function (here) {
            return yes.call(this, here);
        },
        cancel: function (here) {
            return no && no.call(this, here);
        }
    });
}

// Message
artDialogExtend.Message = {
    Success: function (message, callback) {
        art.dialog({
            id: 'art_msg_success',
            title: $.fn.artDialogExtend.sysTipTitle,
            icon: 'succeed',
            fixed: true,
            lock: true,
            content: message,
            ok: true,
            okVal: $.fn.artDialogExtend.Confirm.okText,
            close: (typeof (callback) === 'function') ? callback : null
        });
    },
    Alert: function (message, callback) {
        art.dialog({
            id: 'art_msg_alert',
            title: $.fn.artDialogExtend.sysTipTitle,
            icon: 'warning',
            fixed: true,
            lock: true,
            content: message,
            ok: true,
            okVal: $.fn.artDialogExtend.Confirm.okText,
            close: (typeof (callback) === 'function') ? callback : null
        });
    },
    Error: function (message, callback) {
        art.dialog({
            id: 'art_msg_error',
            title: $.fn.artDialogExtend.sysTipTitle,
            icon: 'error',
            fixed: true,
            lock: true,
            content: message,
            ok: true,
            okVal: $.fn.artDialogExtend.Confirm.okText,
            close: (typeof (callback) === 'function') ? callback : null
        });
    }
}

// Tips
artDialogExtend.Tips = {
    Success: function (message, time, callback) {
        art.dialog({
            id: 'art_tip_success',
            title: false,
            icon: 'succeed',
            fixed: true,
            cancel: false,
            content: message,
            time: (time) ? time : 3,
            close: (typeof (callback) === 'function') ? callback : null
        });
    },
    Alert: function (message, time, callback) {
        art.dialog({
            id: 'art_tip_alert',
            title: false,
            icon: 'warning',
            fixed: true,
            cancel: false,
            content: message,
            time: (time) ? time : 2,
            close: (typeof (callback) === 'function') ? callback : null
        });
    },
    Question: function (message, time, callback) {
        art.dialog({
            id: 'art_tip_question',
            title: false,
            icon: 'question',
            fixed: true,
            cancel: false,
            content: message,
            time: (time) ? time : 2,
            close: (typeof (callback) === 'function') ? callback : null
        });
    },
    Error: function (message, time, callback) {
        art.dialog({
            id: 'art_tip_error',
            title: false,
            icon: 'error',
            fixed: true,
            cancel: false,
            content: message,
            time: (time) ? time : 2,
            close: (typeof (callback) === 'function') ? callback : null
        });
    }
}

// loading信息
artDialogExtend.Loading = {
    Load: function () {
        art.dialog({
            id: 'art_loadding',
            title: false,
            fixed: true,
            lock: true,
            cancel: false,
            content: $.fn.artDialogExtend.Loading.Load.loadText
        });
    },
    Close: function () {
        art.dialog.list['art_loadding'].close();
    }
}

// notice信息
artDialogExtend.Notice = {
    Success: function (options) {
        options.icon = "succeed";
        if (options.time === undefined) options.time = 30;
        artDialogExtend.Notice.Method(options);
    },
    Alert: function (options) {
        options.icon = "warning";
        if (options.time === undefined) options.time = 30;
        artDialogExtend.Notice.Method(options);
    },
    Question: function (options) {
        options.icon = "question";
        if (options.time === undefined) options.time = 30;
        artDialogExtend.Notice.Method(options);
    },
    Error: function (options) {
        options.icon = "error";
        if (options.time === undefined) options.time = 30;
        artDialogExtend.Notice.Method(options);
    },
    Close: function () {
        art.dialog.list['art_notice'].close();
    }
}

artDialogExtend.Notice.Method = function (options) {
    var opt = options || {},
                api, aConfig, hide, wrap, top,
                duration = 800;
    var config = {
        id: 'art_notice',
        left: '100%',
        top: '100%',
        fixed: true,
        drag: false,
        resize: false,
        follow: null,
        lock: false,
        init: function (here) {
            api = this;
            aConfig = api.config;
            wrap = api.DOM.wrap;
            top = parseInt(wrap[0].style.top);
            hide = top + wrap[0].offsetHeight;

            wrap.css('top', hide + 'px')
                .animate({ top: top + 'px' }, duration, function () {
                    opt.init && opt.init.call(api, here);
                });
        },
        close: function (here) {
            wrap.animate({ top: hide + 'px' }, duration, function () {
                opt.close && opt.close.call(this, here);
                aConfig.close = $.noop;
                api.close();
            });

            return false;
        }
    };
    for (var i in opt) {
        if (config[i] === undefined) config[i] = opt[i];
    };
    return artDialog(config);
}

