var bootstrapExtend = {};
var bootstrap_title = "温馨提示";

/* 弹出框 */
bootstrapExtend.Message = {
	Modal : function(objConfig)
	{
		// 创建div
		var _id = "bootstrap-modal-message";
		var $obj = $('#' + _id);
		var _inner = "";
		_inner += "<div class=\"modal-dialog\">";
		_inner += "<div class=\"modal-content\">";
		_inner += objConfig.html;
		_inner += "</div>";
		_inner += "</div>";
		if ($obj.length == 0) {
			$obj = $("<div>", {
				"class" : "modal fade",
				"tabindex" : "-1",
				"id" : _id,
				"role" : "dialog",
				"html" : _inner
			});
			$('body').append($obj);
		} else {
			$obj.html(_inner);
		}
		//设置modal
		$obj.modal({
			backdrop : true,
			keyboard : true
		});
	},
	Alert : function(objConfig) {
		var _icon_msg = "";
		if (objConfig.msgtype === undefined) {
			_icon_msg = "";
		} else {
			switch (objConfig.msgtype.toLowerCase()) {
			case "success":
				_icon_msg = "glyphicon glyphicon-ok-sign success";
				break;
			case "warn":
				_icon_msg = "glyphicon glyphicon-warning-sign warning";
				break;
			case "question":
				_icon_msg = "glyphicon glyphicon-question-sign question";
				break;
			case "error":
			    _icon_msg = "glyphicon glyphicon-remove-circle";
				break;
			default:
				break;
			}
		}
		var _backdrop = (objConfig.backdrop === undefined) ? 'static' : objConfig.backdrop;
		var _keyboard = (objConfig.keyboard === undefined) ? false : objConfig.keyboard;
		var _buttonname = (objConfig.buttonname === undefined) ? "确定" : objConfig.buttonname;
		// 创建div
		var _id = "bootstrap-modal-alert";
		var $obj = $('#' + _id);
		var _inner = "";
		_inner += "<div class=\"modal-dialog\">";
		_inner += "<div class=\"modal-content\">";
		_inner += "<div class=\"modal-header\">";
		_inner += "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>";
		_inner += "<h4 class=\"modal-title\" >" + bootstrap_title + "</h4>";
		_inner += "</div>";
		_inner += "<div class=\"modal-body\">";
		_inner += "<i class=\"" + _icon_msg + "\"></i>" + objConfig.msg;
		_inner += "</div>";
		_inner += "<div class=\"modal-footer\">";
		_inner += "<button type=\"button\" class=\"btn btn-primary\">" + _buttonname + "</button>";
		_inner += "</div>";
		_inner += "</div>";
		_inner += "</div>";
		if ($obj.length == 0) {
			$obj = $("<div>", {
				"class" : "modal fade",
				"tabindex" : "-1",
				"id" : _id,
				"role" : "dialog",
				"html" : _inner
			});
			$('body').append($obj);
		} else {
			$obj.html(_inner);
		}
		//设置modal
		$obj.modal({
			backdrop : _backdrop,
			keyboard : _keyboard
		});
		// 绑定点击事件
		if (objConfig.closeFunction !== undefined && typeof (objConfig.closeFunction) === 'function') {
			$obj.find('.modal-footer button').on('click',objConfig.closeFunction);
		}
		else
		{
			$obj.find('.modal-footer button').on('click',function()
			{
				$obj.modal('hide');
			});
		}
	},
	Confirm : function(objConfig) {
		var _msg = (objConfig.msg === undefined) ? "确定要进行该操作吗?" : objConfig.msg;
		var _id = "bootstrap-modal-confirm";
		var $obj = $('#' + _id);
		var _inner = "";
		_inner += "<div class=\"modal-dialog\">";
		_inner += "<div class=\"modal-content\">";
		_inner += "<div class=\"modal-header\">";
		_inner += "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>";
		_inner += "<h4 class=\"modal-title\" >" + bootstrap_title + "</h4>";
		_inner += "</div>";
		_inner += "<div class=\"modal-body\">";
		_inner += "<i class=\"glyphicon glyphicon-question-sign question\"></i>" + _msg;
		_inner += "</div>";
		_inner += "<div class=\"modal-footer\">";
		_inner += "<button type=\"button\" class=\"btn btn-primary\">确定</button>";
		_inner += "<button type=\"button\" class=\"btn btn-default\">取消</button>";
		_inner += "</div>";
		_inner += "</div>";
		_inner += "</div>";
		if ($obj.length == 0) {
			$obj = $("<div>", {
				"class" : "modal fade",
				"tabindex" : "-1",
				"id" : _id,
				"html" : _inner
			});
			$('body').append($obj);
		} else {
			$obj.html(_inner);
		}
		//设置modal
		$obj.modal({
			backdrop :  'static',
			keyboard : false
		});
		// 重新绑定点击事件
		var $sureBtn = $obj.find('.modal-footer button').eq(0);
		var $cancelBtn = $obj.find('.modal-footer button').eq(1);
		if (objConfig.sureFunction !== undefined && typeof (objConfig.sureFunction) == 'function') {
			$sureBtn.on('click', objConfig.sureFunction);
		} else {
			$sureBtn.on('click', function() {
				return true;
			});
		}
		$cancelBtn.on('click', function() {
			$obj.modal('hide');
		});
	},
	Close : function() {
		if ($('#bootstrap-modal-message')) {
			$('#bootstrap-modal-message').modal('hide');
		}
		if ($('#bootstrap-modal-alert')) {
			$('#bootstrap-modal-alert').modal('hide');
		}
		if ($('#bootstrap-modal-confirm')) {
			$('#bootstrap-modal-confirm').modal('hide');
		}
	}
}

/* iframe */
bootstrapExtend.Iframe = {
	Open : function(objConfig) {
		var _title = (objConfig.title === undefined) ? '' : objConfig.title;
		var _url = (objConfig.url === undefined) ? '#' : objConfig.url;
		var _height = (objConfig.height === undefined) ? '' : objConfig.height;
		var _backdrop = (objConfig.backdrop === undefined) ? true : objConfig.backdrop;
		var _id = "bootstrap-modal-iframe";
		var $obj = $('#' + _id);
		var _inner = "";
		_inner += "<div class=\"modal-dialog\">";
		_inner += "<div class=\"modal-content\">";
		_inner += "<div class=\"modal-header\">";
		_inner += "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-label=\"Close\"><span aria-hidden=\"true\">&times;</span></button>";
		_inner += "<h4 class=\"modal-title\" >" + _title + "</h4>";
		_inner += "</div>";
		 if(_height=='')
		 {
			 _inner += "<div class=\"modal-body\">";
		 }
		 else
		 {
			 _inner += "<div class=\"modal-body\" style=\"height:"+_height+"px;\">";
		 }
		_inner += "<iframe name=\"mainFrame\" scrolling=\"auto\" frameborder=\"0\" src=\"" + _url + "\" style=\"width:100%;height:90%;\"></iframe>";
		_inner += "</div>";
		_inner += "<div class=\"modal-footer\">";
		_inner += "<button type=\"button\" class=\"btn btn-primary\">确定</button>";
		_inner += "</div>";
		_inner += "</div>";
		_inner += "</div>";
		if ($obj.length == 0) {
			$obj = $("<div>", {
				"class" : "modal fade",
				"tabindex" : "-1",
				"id" : _id,
				"role" : "dialog",
				"html" : _inner
			});
			$('body').append($obj);
		} else {
			$obj.html(_inner);
		}
		// 设置modal
		$obj.modal({
			backdrop : _backdrop,
			keyboard : _backdrop
		});
		// 绑定点击事件
		if (objConfig.closeFunction !== undefined && typeof (objConfig.closeFunction) === 'function') {
			$obj.find('.modal-footer button').on('click',objConfig.closeFunction);
		}
		else
		{
			$obj.find('.modal-footer button').on('click',function()
			{
				$obj.modal('hide');
			});
		}
	},
	Close : function() {
		$('#bootstrap-modal-iframe').modal('hide');
	}
}

/* loading效果 */
bootstrapExtend.Loading = {
	Load : function() {
		var _id = "bootstrap-modal-loading";
		var $obj = $('#' + _id);
		if ($obj.length == 0) {
			var _inner = "";
			_inner += "<div class=\"modal-dialog\">";
			_inner += "<div class=\"modal-content\">";
			_inner += "<div class=\"modal-body\">";
			_inner += "<i class=\"glyphicon glyphicon-refresh btn-icon-spin\"></i>Loading...";
			_inner += "</div>";
			_inner += "</div>";
			_inner += "</div>";
			$obj = $("<div>", {
				"class" : "modal fade",
				"tabindex" : "-1",
				"id" : "bootstrap-modal-loading",
				"html" : _inner
			});
			$('body').append($obj);
		}
		//设置modal
		$obj.modal({
			backdrop :  'static',
			keyboard : false
		});
	},
	Close : function() {
		$('#bootstrap-modal-loading').modal('hide');
	}
}

/* 自定义控件效果 */
bootstrapExtend.defineControl = {
	Text : function() {
		var $obj = $("input[class^=bootstrap-control-text]");
		if ($obj.length > 0) {
			for (var i = 0; i < $obj.length; i++) {
				// 创建删除控件
				var $i_del = $obj.eq(i).parent().find('.my-input-clear');
				$i_del = $("<span>", {
					"class" : "my-input-clear",
					"html": "<i class=\"glyphicon glyphicon-remove\"></i>",
					click : function() {
						$(this).prev().val('');
						$(this).hide();
					}
				});
				$i_del.insertAfter($obj.eq(i));
				// 初始状态
				if ($obj.eq(i).val().length > 0) {
					$i_del.show();
				} else {
					$i_del.hide();
				}
				// 绑定按钮事件
				$obj.eq(i).keyup(function() {
					if ($(this).val().length > 0) {
						$(this).next().show();
					} else {
						$(this).next().hide();
					}
				});
			}
		}
	}
}