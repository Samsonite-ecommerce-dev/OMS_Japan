/**easyui-extend**/
if ($.fn.easyUIExtend) {
    $.fn.easyUIExtend = {
        Grid: {
            List: {
                onLoadErrorText: '数据读取错误!'
            },
            SaveDialog: {
                defaultbuttonText: '保存',
                confirmText: '确实要进行该操作吗?'
            },
            SaveOper: {
                messageAltText: '请至少选择一条要操作的信息!'
            },
            ComplexSaveOper: {
                messageAltText: '请至少选择一条要操作的信息!'
            },
            SingleOper: {
                messageAltText: '请至少选择一条要操作的信息!'
            },
            ComplexOper: {
                messageAltText: '请至少选择一条要操作的信息!',
                confirmText: '确实要进行该操作吗?',
                errorText: '系统错误,请重新尝试!'
            },
            SingleRowOrComplexOper: {
                messageAltText: '请至少选择一条要操作的信息!'
            },
            CommonOper: {
                errorText: '系统错误,请重新尝试!'
            }
        }, GridExtension: {
            DetailView: {
                onLoadErrorText: '数据读取错误!'
            }
        },
        SubForm: {
            defaultbuttonText: '保存',
            errorText: '系统错误,请重新尝试!'
        }
    }
}

/**easyui-extend**/
if ($.fn.artDialogExtend) {
    $.fn.artDialogExtend = {
        sysTipTitle: '系统提示',
        Loading: {
            Load: {
                loadText: '数据加载中...'
            }
        },
        Confirm: {
            okText: '确定',
            cancelText: '取消'
        }
    }
}

if ($.fn.commonJs) {
    $.fn.commonJs = {
        sysAjaxFunction: {
            errorText: '系统错误,请重新尝试!'
        },
        openFileView: {
            title: '上传文件'
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