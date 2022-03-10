//print
var printExtend = {};

/*
 *printArea 打印区域
 *dataArray 数据数组
*/
printExtend.printPages = function (printArea, dataArray) {
    //loadding
    artDialogExtend.Loading.Load();
    //计数
    var _currentIndex = 0;

    //创建打印区域分块
    var obj_Array = new Array(dataArray.length);
    for (var i = 0; i < dataArray.length; i++) {
        obj_Array[i] = $("<div>", {
            id: "A" + i,
        });
        printArea.append(obj_Array[i]);
    }
    printArea.hide();
    for (var i = 0; i < dataArray.length; i++) {
        var obj = dataArray[i];
        var iframe = $('<iframe index=' + i + ' style="display: none" src="' + obj.url + '" sort=' + obj.sort + '></iframe>').load(function () {
            var index = parseInt($(this).attr('index'));
            var _body = $(this).contents().find('body');
            //为div添加自动分页属性page-break-after,强制在页尾进行分页
            var newBody = $("<div style='page-break-after:always'></div>");
            var sort = $(this).attr('sort');
            newBody.append("<h2>" + sort + "</h2>");
            var $link = $(this).contents().find('head').find('link');
            if ($link.length > 0) {
                _href = $link.attr("href");
                newBody.append('<link rel="stylesheet" href="' + _href + '">');
            }
            newBody.append(_body);
            //分块插入,解决无序排列问题
            obj_Array[index].html(newBody);
            //printArea.append(newBody);
            _currentIndex++;
            //判断是否全部完成
            if (_currentIndex == dataArray.length) {
                //关闭加载提示
                artDialogExtend.Loading.Close();
                //打印
                printArea.show();
                printArea.print({
                    globalStyles: true,
                    mediaPrint: false
                });
                printArea.hide();
            }
        });
        $(document.body).before(iframe);
    }
}

/*
 *printArea 打印区域
 *dataArray 数据数组
 *manifestHrml manifest页面代码
*/
printExtend.printAllPages = function (printArea, dataArray, manifestHrml) {
    //loadding
    artDialogExtend.Loading.Load();
    //计数
    var _currentIndex = 0;

    //创建打印区域分块
    var obj_Array = new Array(dataArray.length);
    for (var i = 0; i < dataArray.length; i++) {
        obj_Array[i] = $("<div>", {
            id: "A" + i,
        });
        printArea.append(obj_Array[i]);
    }
    //添加manifest打印块
    if (manifestHrml != '') {
        var mH = $("<div>", {
            id: "M0",
            html: manifestHrml
        });
        printArea.append(mH);
    }
    printArea.hide();
    for (var i = 0; i < dataArray.length; i++) {
        var obj = dataArray[i];
        var iframe = $('<iframe index=' + i + ' style="display: none" src="' + obj.url + '" sort=' + obj.sort + '></iframe>').load(function () {
            var index = parseInt($(this).attr('index'));
            var _body = $(this).contents().find('body');
            //为div添加自动分页属性page-break-after,强制在页尾进行分页
            var newBody = $("<div style='page-break-after:always'></div>");
            var sort = $(this).attr('sort');
            newBody.append("<h2>" + sort + "</h2>");
            var $link = $(this).contents().find('head').find('link');
            if ($link.length > 0) {
                _href = $link.attr("href");
                newBody.append('<link rel="stylesheet" href="' + _href + '">');
            }
            newBody.append(_body);
            //分块插入,解决无序排列问题
            obj_Array[index].html(newBody);
            //printArea.append(newBody);
            _currentIndex++;
            //判断是否全部完成
            if (_currentIndex == dataArray.length) {
                //关闭加载提示
                artDialogExtend.Loading.Close();
                //打印
                printArea.show();
                printArea.print({
                    globalStyles: true,
                    mediaPrint: false
                });
                printArea.hide();
            }
        });
        $(document.body).before(iframe);
    }
}