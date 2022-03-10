/**easyui-extend**/
if ($.fn.easyUIExtend) {
    $.fn.easyUIExtend = {
        Grid: {
            List: {
                onLoadErrorText: 'The Data cannot be retrieved!'
            },
            SaveDialog: {
                defaultbuttonText: 'Save',
                confirmText: 'Do you wish to proceed?'
            },
            SaveOper: {
                messageAltText: 'Please select at least one record.'
            },
            ComplexSaveOper: {
                messageAltText: 'Please select at least one record.'
            },
            SingleOper: {
                messageAltText: 'Please select at least one record.'
            },
            ComplexOper: {
                messageAltText: 'Please select at least one record.',
                confirmText: 'Do you wish to proceed?',
                errorText: 'System error, please try again!'
            },
            SingleRowOrComplexOper: {
                messageAltText: 'Please select at least one record.'
            },
            CommonOper: {
                errorText: 'System error, please try again!'
            }
        }, GridExtension: {
            DetailView: {
                onLoadErrorText: 'The Data cannot be retrieved!'
            }
        },
        SubForm: {
            defaultbuttonText: 'Save',
            errorText: 'System error, please try again!'
        }
    }
}

/**easyui-extend**/
if ($.fn.artDialogExtend) {
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
}

/**common**/
if ($.fn.commonJs) {
    $.fn.commonJs = {
        sysAjaxFunction: {
            errorText: 'system error, please try again!'
        },
        openFileView: {
            title: 'Upload File'
        }
    }
}

/**echart**/
if ($.fn.echartExtend) {
    $.fn.echartExtend = {
        AjaxSet: {
            loadingText: 'Loading...'
        }
    }
}