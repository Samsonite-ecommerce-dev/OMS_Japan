var mobile = {};
/* 返回上一页 */
mobile.pageBack = function () {
	var a = window.location.href;
	if (/#top/.test(a)) {
		window.history.go(-2);
	} else {
		window.history.back();
	}
}

/* 逐步加载数据 */
// url 数据读取地址
// params 参数值
// messageBox 显示对象
// nextBox 提示对象
// resultFunc 数据集合操作方法
// afterLoad 加载成功后执行方法
// lazyloadOpen 是否开启图片懒加载，默认不开启
// lazyloadImg 图片懒加载样式
// lazyloadEffect 图片懒加载效果,默认fadeIn
// lazyloadEffectspeed 图片懒加载效果速度,默认速度100
// scrollPage 默认滚动加载页数,0表示不自动加载
mobile.pageList = function (config) {
	var _url = config.url;
	var _page = (config.page === undefined) ? 1 : config.page;
	var _params = config.params;
	var _messageBox = config.messageBox;
	var _nextBox = config.nextBox;
	var _resultFunc = config.resultFunc;
	var _afterLoad = (config.afterLoad === undefined) ? null : config.afterLoad;
	var _lazyloadOpen = (config.lazyloadOpen === undefined) ? false : config.lazyloadOpen;
	var _lazyloadImg = config.lazyloadImg;
	var _lazyloadEffect = (config.lazyloadEffect === undefined) ? 'fadeIn' : config.lazyloadEffect;
	var _lazyloadEffectspeed = (config.lazyloadEffectspeed === undefined) ? 100 : config.lazyloadEffectspeed;
	var _scrollPage = config.scrollPage;
	// 参数
	var _maxpage = 0;
	var _isloading = false;
	// 滚动设定
	if (_scrollPage > 0) {
		InitScroll();
	}

	// 加载数据
	this.load = function() {
		GetData();
	}

	// 重新加载数据
	this.Reload = function(re_config) {
		if (re_config.url!==undefined)
			_url = re_config.url;
		if (re_config.params!==undefined)
			_params = re_config.params;
		// 清空元数据
		_messageBox.empty();
		// 重置读取第一页数据
		_page = 1;
		// 记载数据
		GetData();
	}

	// 读取数据方法
	function GetData() {
		_isloading = true;
		var _t_params = "";
		if (_params != "") {
			_t_params += _params + "&page=" + _page;
		} else {
			_t_params = "page=" + _page;
		}
		$.ajax({
					url : _url,
					type : 'post',
					data : _t_params,
					dataType : 'json',
					beforeSend : function(XMLHttpRequest) {
					    _nextBox.html("<i class=\"glyphicon glyphicon-refresh btn-icon-spin\"></i>数据加载中...");
						_nextBox.show();
					},
					error : function(XMLHttpRequest, textStatus, errorThrown) {

					},
					success : function(result, textStatus) {
						_isloading = false;
						_maxpage = result.totalpage;
						// 返回数据
						var _str_append = _resultFunc(result);
						// 添加信息，如果是第一页，表示是重新搜索
						if (result.nowpage == 1) {
							if (result.data.length == 0) {
								_nextBox.html("暂无数据 ");
								_nextBox.show();
							} else {
								_messageBox.html(_str_append);
								if (parseInt(result.nowpage) + 1 > result.totalpage) {
									_nextBox.html("");
									_nextBox.hide();
								} else {
									_nextBox.html($("<a>",
									{
										href : "javascript:void(0);",
										'class' : "next",
										html : "点击查看更多信息",
										click : function() {
											_page = parseInt(result.nowpage) + 1;
											GetData();
										}
									}));
								}
							}
						} else {
							$(_str_append).appendTo(_messageBox);
							if (parseInt(result.nowpage) + 1 > result.totalpage) {
								_nextBox.html("");
								_nextBox.hide();
							} else {
								_nextBox.html($("<a>", {
									href : "javascript:void(0);",
									'class' : "next",
									html : "点击查看更多信息",
									click : function() {
										_page = parseInt(result.nowpage) + 1;
										GetData();
									}
								}));
							}
						}
						// 图片懒加载
						if (_lazyloadOpen) {
							_messageBox.find('img.' + _lazyloadImg).lazyload({
								effect : _lazyloadEffect,
								effectspeed : _lazyloadEffectspeed
							});
							// 清空图片绑定标识img_lazy，以防止重复绑定
							_messageBox.find('img.' + _lazyloadImg)
									.removeClass(_lazyloadImg);
						}
						// 执行成功后执行函数
						if (_afterLoad != null && typeof (_afterLoad) === 'function') {
							_afterLoad();
						}
					}
				});
	}

	// 滚动设定方法
	function InitScroll() {
		$(window).scroll(
				function() {
					if (_scrollPage > _maxpage)
						_scrollPage = _maxpage;
					// 如果点击显示更多框出现在可见区域内,则自动加载
					var _see_next = _nextBox.find('a.next');
					if (_see_next.length > 0) {
						var _autoheight = _see_next.prop("offsetTop") - 100;
						if (_page < _scrollPage) {
							if (_autoheight >= $(window).scrollTop() && _autoheight < ($(window).scrollTop() + $(window).height())) {
								_page++;
								if (_page > _maxpage)
									_page = _maxpage;
								if (!_isloading) {
									GetData();
								}
							}
						}
					}
				});
	}
}

/* 列表操作 */
//actionButton 操作按钮对象
//actionMenu 操作菜单
//dataObject 数据对象
mobile.Grid = function (config) {
	var _actionButton = config.actionButton;
	var _actionMenu = config.actionMenu;
	var _dataObject = config.dataObject;
	var _defaultFunc = (typeof(config.defaultFunc) === 'function') ? config.defaultFunc : null;
	var _addFunc = (typeof(config.addFunc) === 'function') ? config.addFunc : null;
	var _editFunc = (typeof(config.editFunc) === 'function') ? config.editFunc : null;
	var _delFunc = (typeof(config.delFunc) === 'function') ? config.delFunc : null;
	var $actionBar=null;
	//当前状态
	var $actionState="";
	//初始化菜单
	_actionButton.bind('click',function(){
		if($actionState=="")
		{
			var _html='';
			_html+='<div id="action-menu" class="commom-modal">';
			for(var i=0;i<_actionMenu.length;i++)
			{
				if(i==_actionMenu.length-1)
				{
					_html+='<a id="'+_actionMenu[i][0]+'" class="border-no">'+_actionMenu[i][1]+'</a>';
				}
				else
				{
					_html+='<a id="'+_actionMenu[i][0]+'">'+_actionMenu[i][1]+'</a>';
				}
			}
			_html+='</div>';
			bootstrapExtend.Message.Modal({
				html:_html
			});
			//隐藏值
			$actionBar=$('#action-menu');
			//添加事件
			if(_addFunc!=null) add(_addFunc);
			//编辑事件
			if(_editFunc!=null) edit(_editFunc);
			//删除事件
			if(_delFunc!=null) del(_delFunc);
		}
		else
		{
			//重置页面状态
			var $li = _dataObject.find('.grid-li');
			var $icon = _dataObject.find('.grid-icon');
			if ($li.length > 0) {
			    if ($actionState == "edit") $icon.removeClass("glyphicon glyphicon-edit color_danger");
			    if ($actionState == "del") $icon.removeClass("glyphicon glyphicon-remove color_danger");
				// 置空点击事件
				if(_defaultFunc===null)
				{
					$li.unbind("click");
				}
				else
				{
					$li.unbind("click");
					$li.bind("click",_defaultFunc);
				}
				$icon.hide();
			}
			$actionState="";
			_actionButton.find('i').attr("class", "glyphicon glyphicon-plus glyphicon-circle-green");
		}
	});	

	//添加
	function add(objFunc)
	{
		if (typeof (objFunc) === 'function') {
			$actionBar.find('#add').bind('click', objFunc);
		}
	}

	//编辑
	function edit(objFunc) {
		if (typeof (objFunc) === 'function') {
			$actionBar.find('#edit').bind('click',function() {
				var $li = _dataObject.find('.grid-li');
				var $icon = _dataObject.find('.grid-icon');
				if ($li.length > 0) {
				    $icon.addClass("glyphicon glyphicon-edit color_warning");
					// 绑定点击事件
					$li.unbind("click").bind('click', objFunc);
					$icon.show();
				}
				//更改当前编辑状态
				$actionState="edit";
				_actionButton.find('i').attr("class", "glyphicon glyphicon-pencil-square-o glyphicon-circle-green");
				//关闭菜单
				bootstrapExtend.Message.Close();
			});
		}
	}

	//删除
	function del(objFunc) {
		if (typeof (objFunc) === 'function') {
			$actionBar.find('#del').bind('click',function() {
				var $li = _dataObject.find('.grid-li');
				var $icon = _dataObject.find('.grid-icon');
				if ($li.length > 0) {
				    $icon.addClass("glyphicon glyphicon-remove color_danger");
					// 绑定点击事件
					$li.unbind("click").bind('click', objFunc);
					$icon.show();
				}
				//更改当前编辑状态
				$actionState="del";
				_actionButton.find('i').attr("class", "glyphicon glyphicon-remove glyphicon-circle-green");
				//关闭菜单
				bootstrapExtend.Message.Close();
			});
		}
	}
}

/* Ajax执行函数 */
// url 数据读取地址
// type get/post,默认post
// data 参数
// dataType html/json,默认json
// button 按钮对象
// beforeFunc 加载中执行函数
// successFunc 成功后执行函数
mobile.ajax = function (config) {
	var _url = config.url;
	var _type = (config.type === undefined) ? 'post' : config.type;
	var _data = config.data;
	var _dataType = (config.dataType === undefined) ? 'json' : config.dataType;
	var _button = (config.button === undefined) ? null : config.button;
	var _beforeFunc = (typeof (config.beforeFunc) === 'function') ? config.beforeFunc : null;
	var _successFunc = (typeof (config.successFunc) === 'function') ? config.successFunc : null;
	$.ajax({
		url : _url,
		type : _type,
		data : config.data,
		dataType : _dataType,
		beforeSend : function(XMLHttpRequest) {
			if (_beforeFunc != null)
				_beforeFunc();
			if (_button != null)
				mobile.button.Disable(_button);
		},
		error : function(XMLHttpRequest, textStatus, errorThrown) {
			// 恢复按钮状态
			if (_button != null)
				mobile.button.Reset(_button);
			mobile.Message.Warn('系统错误,请重新尝试');
		},
		success : function(result, textStatus) {
			// 恢复按钮状态
			if (_button != null)
				mobile.button.Reset(_button);
			if (_successFunc != null)
				_successFunc(result);
		}
	});
}

/* 通用倒计时 */
//initNum 初始数值
//processFunc 过程中的执行方法
//completeFunc 结束后的执行方法
mobile.CountDownClock = function (config) {
	var _initNum = (config.initNum >= 0) ? config.initNum : 0;
	var _processFunc = config.processFunc;
	var _completeFunc = config.completeFunc;
	var _tempNum = _initNum;
	// 开始执行
	Process();
	// 循环函数
	function Process() {
		if (_tempNum > 0) {
			if (typeof (_completeFunc) === 'function'){
				_processFunc(_tempNum);
			}
			_tempNum--;
			setTimeout(Process, 1000);
		} else {
			if (typeof (config.completeFunc) === 'function') {
				_completeFunc();
			}
		}
	}
}

/* 信息提示 */
// msg 提示信息
//backdrop true:点击空白处关闭窗体
// closeFun　关闭窗体后执行函数
mobile.Message = {
	Success : function(msg, backdrop, closeFunc) {
		bootstrapExtend.Message.Alert({
			msg : msg,
			msgtype : 'success',
			backdrop : (backdrop===undefined)?'static':((backdrop)?true:'static'),
			keyboard : (backdrop===undefined)?false:true,
			closeFunction : (typeof (closeFunc) === 'function') ? closeFunc: null
		});
	},
	Warn : function(msg, closeFunc) {
		bootstrapExtend.Message.Alert({
			msg : msg,
			msgtype : 'warn',
			backdrop : true,
			keyboard : true,
			closeFunction : (typeof (closeFunc) === 'function') ? closeFunc: null
		});
	}
}

/* 按钮点击样式 */
mobile.button = {
	Disable : function(object) {
		var $obj_data = eval('(' + object.attr("data-text") + ')');
		var _text = "";
		if ($obj_data.loadingText) {
			_text = $obj_data.loadingText;
		} else {
			_text = "数据加载中";
		}
		object.html("<i class=\"glyphicon glyphicon-refresh btn-icon-spin\"></i>" + _text + "...");
		object.prop("disabled", true);
	},
	Reset : function(object) {
		var $obj_data = eval('(' + object.attr("data-text") + ')');
		var _text = $obj_data.buttonText;
		object.html(_text);
		object.prop("disabled", false);
	}
}

/* 上传文件 */
//back_object 回调对象
mobile.Upfile = {
	Image : function(back_object) {
		bootstrapExtend.Iframe.Open({
			title:'上传文件',
			url:'/upfile/image',
			height:275,
			backdrop:true,
			closeFunction:function()
			{
				mainFrame.window.SetFilePath(back_object);
			}
		});
	}
}

/* 上传文件 */
// object对象
// url 获取地址
// width 图片宽度
// height 图片高度
mobile.ValidateCode = function(object,url, width, height) {
	mobile.ajax({
		url:url,
		type:'post',
		data:{w:width,h:height},
		dataType:'json',
		successFunc:function(result)
		{
			object.html(result.image);
		}
	});
}

/* 固定头部 */
mobile.HeadFixed = function(object) {
	var _height = object.height();
	object.css({
		'position' : 'fixed',
		'top' : '0',
		'left' : '0',
		'z-index' : '1010'
	});
	$div = $("<div>", {
		"style" : "height:" + _height + "px"
	});
	object.after($div);
}

/* 滚动条监测回到头部 */
mobile.ScrollSpyTop = function () {
	$a = $("<a>", {
		"class" : "glyphicon glyphicon-arrow-up",
		"click":function()
		{
			window.scrollTo(0,0);
		}
	})
	$obj = $("<div>", {
		"class" : "gotop-toolbar",
		"style" : "display:none"
	});
	$obj.append($a);
	$('body').append($obj);
	// 滚动监测时间
	var _spy_top_status = false;
	$(window).scroll(function() {
		if ($(window).scrollTop() > 0) {
			if (!_spy_top_status) {
				if ($obj.css("display") == "none") {
					_spy_top_status = true;
					$obj.fadeIn('fast', function() {
						_spy_top_status = false;
					});
				}
			}
		} else {
			if (!_spy_top_status) {
				if ($obj.css("display") == "block") {
					_spy_top_status = true;
					$obj.fadeOut('fast', function() {
						_spy_top_status = false;
					});
				}
			}
		}
	})
}

/* 预加载图片 */
// object 要记载的图片对象
mobile.LoadImg = function(config) {
	var _obj_img = (config.object) ? config.object : $('body');
	// 创建遮罩层
	$loading = $(
			"<div>",
			{
				id : "com_img_loading",
				style : "width:100%;height:100%;position:fixed;top:0;z-index:99;background:#27aaff;color:#fff;"
			}).html($("<div>",
				{
					style : "width:30%;margin:auto;padding-top:30%;text-align:center;line-height:32px;",
					html : "<div>Loading...</div><div id=\"rate\" style=\"height:24px;line-height:24px;\"></div>"
				}));
	$('body').append($loading);
	// 加载图片
	var $imgs = _obj_img.find("img");// 图片数组
	var len = $imgs.length;// 图片数量
	var downImg = 0;// 已下载数量
	var percent = 0;// 百分比
	for (var t = 0; t < len; t++) {
		var $i = new Image();
		var imgsrc = $imgs.eq(t).attr("loadsrc");
		$i.src = imgsrc;
		if ($i.complete) {
			$imgs.eq(t).attr("src", imgsrc).removeAttr("loadsrc");// 有缓存
			imgDown();
		} else {
			$imgs.eq(t).attr("src", imgsrc).load(function() {
				$(this).removeAttr("loadsrc");// 无缓存
				imgDown();
			})
		}
	}

	function imgDown() {
		downImg++;
		percent = parseInt(100 * downImg / len);
		$('#com_img_loading  #rate').html(percent + "%");
		if (percent == 100) {
			$('#com_img_loading').fadeOut();
		}
	}
}

/* 是否是微信端 */
mobile.isWeixn = function() {
	var ua = navigator.userAgent.toLowerCase();
	if (ua.match(/MicroMessenger/i) == "micromessenger") {
		return true;
	} else {
		return false;
	}
}

/* 是否是苹果端 */
mobile.isIOS = function() {
	var userAgentInfo = navigator.userAgent;
	var Agents = new Array("iPhone");
	var flag = false;
	for (var v = 0; v < Agents.length; v++) {
		if (userAgentInfo.indexOf(Agents[v]) > 0) {
			flag = true;
			break;
		}
	}
	return flag;
}
