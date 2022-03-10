/**easyui-extend**/
if ($.fn.easyUIExtend) {
    $.fn.easyUIExtend = {
        Grid: {
            List: {
                onLoadErrorText: '數據讀取錯誤!'
            },
            SaveDialog: {
                defaultbuttonText: '保存',
                confirmText: '確實要進行該操作嗎?'
            },
            SaveOper: {
                messageAltText: '請至少選擇一條要操作的信息!'
            },
            ComplexSaveOper: {
                messageAltText: '請至少選擇一條要操作的信息!'
            },
            SingleOper: {
                messageAltText: '請至少選擇一條要操作的信息!'
            },
            ComplexOper: {
                messageAltText: '請至少選擇一條要操作的信息!',
                confirmText: '確實要進行該操作嗎?',
                errorText: '系統錯誤,請重新嘗試!'
            },
            SingleRowOrComplexOper: {
                messageAltText: '請至少選擇一條要操作的信息!'
            },
            CommonOper: {
                errorText: '系統錯誤,請重新嘗試!'
            }
        }, GridExtension: {
            DetailView: {
                onLoadErrorText: '數據讀取錯誤!'
            }
        },
        SubForm: {
            defaultbuttonText: '保存',
            errorText: '系統錯誤,請重新嘗試!'
        }
    }
}

/**easyui-extend**/
if ($.fn.artDialogExtend) {
    $.fn.artDialogExtend = {
        sysTipTitle: '系統提示',
        Loading: {
            Load: {
                loadText: '數據加載中...'
            }
        },
        Confirm: {
            okText: '確定',
            cancelText: '取消'
        }
    }
}

/**common**/
if ($.fn.commonJs) {
    $.fn.commonJs = {
        sysAjaxFunction: {
            errorText: '系统错误,请重新尝试!'
        },
        openFileView: {
            title: '上傳文件'
        }
    }
}

/**echart**/
if ($.fn.echartExtend) {
    $.fn.echartExtend = {
        AjaxSet: {
            loadingText: '数据加载中...'
        }
    }
}