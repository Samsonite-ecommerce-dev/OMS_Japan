
//初始化菜单栏
function InitMenu() {
    var $dl = $('.menu_list').find('dl');
    var $dd = $('.menu_list').find('dd');
    //初始化菜单显示
    $dl.eq(0).show();
    //$dd.eq(0).addClass('active');
    //父级点击事件
    $('.menu_list').find('div.title').bind('click', function () {
        var _dl = $(this).parent().find('dl');
        if (_dl.css('display') == 'none') {
            //关闭已打开的菜单栏
            for (var t = 0; t < $dl.length; t++) {
                $dl.eq(t).hide();
            }
            _dl.slideDown(500);
        }
        else {
            _dl.slideUp(500);
        }
    });
    //绑定链接
    $('.menu_list').find('dd').bind('click', function () {
        var $a = $(this).find('a');
        //清空其它ID
        for (var t = 0; t < $dd.length; t++) {
            $dd.eq(t).removeClass('active');
        }
        $(this).addClass('active');
        easyUIExtend.AddMenuTab($a.html(), $a.attr('rel'), false);
    });
}

//初始化菜单切换
function InitTab() {
    $('#tabs').tabs({
        border: false,
        onSelect: function (title, index) {
            //解决tab的iframe内容会被最新的tab覆盖问题,所以每次切换后,重新载入
            var _url = $('#tabs').find('iframe[name=mainFrame]').eq(index).attr("src");
            $('#tabs').tabs('update', {
                tab: $('#tabs').tabs('getSelected'),
                options: {
                    content: createFrame(_url)
                }
            });
        }
    });
}
