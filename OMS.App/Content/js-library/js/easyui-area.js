function AreaInit(obj) {
    var $AreaObj = obj;
    var $selOption = $AreaObj.find('input');
    //初始化省列表
    InitAreaOption(0, function (value) {
        ReloadCity(value);
        ReloadDistrict('');
    });
    //初始化市列表
    InitAreaOption(1, function (value) {
        ReloadDistrict(value);
    });
    //初始化地区列表
    InitAreaOption(2);

    function InitAreaOption(objId, objFunc) {
        var _value = '';
        switch (objId) {
            case 0:
                _value = '1';
                break;
            case 1:
                if ($selOption.eq(0).val) {
                    _value = $selOption.eq(0).val();
                }
                break;
            case 2:
                if ($selOption.eq(1).val) {
                    _value = $selOption.eq(1).val();
                }
                break;
            default:
                _value = '';
                break;
        }
        //初始化信息
        $selOption.eq(objId).combobox({
            url: '/Common/Area_Message',
            valueField: 'value',
            textField: 'text',
            queryParams: {
                parentid: _value
            },
            onChange: (objFunc != undefined) ? objFunc : function () { }
        });
    }

    function ReloadCity(value) {
        $selOption.eq(1).combobox({
            url: '/Common/Area_Message',
            valueField: 'value',
            textField: 'text',
            queryParams: {
                parentid: value,
            },
            onChange: function (value) {
                ReloadDistrict(value);
            }
        }).combobox('clear');
    }

    function ReloadDistrict(value) {
        $selOption.eq(2).combobox({
            url: '/Common/Area_Message',
            valueField: 'value',
            textField: 'text',
            queryParams: {
                parentid: value,
            }
        }).combobox('clear');
    }
}
