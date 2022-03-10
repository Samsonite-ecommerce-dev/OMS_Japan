/*
 * easyUIExtend.Grid DataGrid相关操作
 * >>Toolbar :工具栏操作
 * >>List: 列表操作
 * >>Search: 查询操作
 * >>SingleRowSelect: 获取DataGrid的checkbox的最后一条选中项
 * >>ComplexSelect: 获取DataGrid的checkbox选中数组集合
 * >>Refresh: 重新加载DataGrid
 * >>OpenDialog: 新开窗口
 * >>SaveDialog: 新开保存窗口
 * >>SaveOper:单选操作,同时新开保存窗口
 * >>ComplexSaveOper:多选操作,同时新开保存窗口
 * >>SingleOper:单选操作,同时新开窗口
 * >>ComplexOper:多选操作
 * >>SingleRowOrComplexOper:如果单条,弹出新窗口;如果多条,批量操作
 * >>CommonOper:简易操作

 * easyUIExtend.GridExtension datagrid扩展,需要引用datagrid-detailview.js
 * >>DetailView :下拉详细操作

 * easyUIExtend.ComboGrid 下拉Grid

 * easyUIExtend.AddMenuTab 新建菜单栏

 * easyUIExtend.IframeGrid 获取Url传递过来的参数

 * easyUIExtend.Control 自定义easyUI控件
 */
var easyUIExtend = {};
var _easyUI_LastSelectedRow;
// 语言
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
			messageAltText: 'Please select at least one record'
		},
		ComplexOper: {
			messageAltText: 'Please select at least one record',
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

// DataGrid列表
easyUIExtend.Grid = {
    /*objToolbar   toolbarID
     *objConfig:
     **powerTools 权限串(数组)
     **hideTools  当前功能栏(数组)
     */
	Toolbar: function (objToolbar, objConfig) {
		var _tool_array = new Array();
		//如果power值是false,表示排除验证直接显示
		var $o = objToolbar.find('a[power!="false"]');
		if (objConfig.powerTools != undefined) {
			for (var i = 0; i < $o.length; i++) {
				var _tool_id = $o.eq(i).attr("id");
				if ($.inArray(_tool_id, objConfig.powerTools) > -1) {
					if (objConfig.hideTools == undefined) {
						objToolbar.find('#' + _tool_id).show();
					}
					else {
						if ($.inArray(_tool_id, objConfig.hideTools) > -1) {
							objToolbar.find('#' + _tool_id).hide();
						}
						else {
							objToolbar.find('#' + _tool_id).show();
						}
					}
				}
				else {
					objToolbar.find('#' + _tool_id).hide();
				}
			}
		}
	},
	List: function (objGrid, objParams) {
		objParams = objParams || {};
		// 绑定列表
		var $O = objGrid.datagrid({
			// 宽度
			width: objParams.width,
			// 高度
			height: (objParams.height === undefined) ? $(document).height() : objParams.height,
			// 自动适屏功能
			fit: (objParams.fit === undefined) ? false : objParams.fit,
			// 数据表格列配置对象
            /*
            * columns: [[{ field: '0', checkbox: true },{ field: '', title: ''
            * }]],
            */
			// 跟列属性一样，但是这些列固定在左边，不会滚动
			frozenColumns: (objParams.frozenColumns === undefined) ? [[]] : objParams.frozenColumns,
			// 设置为true将自动使列适应表格宽度以防止出现水平滚动
			fitColumns: (objParams.fitColumns === undefined) ? true : objParams.fitColumns,
			// 定义设置行的高度，根据该行的内容。设置为false可以提高负载性能
			autoRowHeight: (objParams.autoRowHeight === undefined) ? true : objParams.autoRowHeight,
			// 数据表格顶部面板的工具栏
			toolbar: objParams.toolbar,
			// 设置为true将交替显示行背景
			striped: (objParams.striped === undefined) ? false : objParams.striped,
			// 请求远程数据的方法类型
			method: (objParams.method === undefined) ? 'post' : objParams.method,
			// 设置为true，当数据长度超出列宽时将会自动截取
			nowrap: (objParams.nowrap === undefined) ? true : objParams.nowrap,
			// 表明该列是一个唯一列
			/* idField :null, */
			// 一个用以从远程站点请求数据的超链接地址
			url: objParams.url,
			// 当从远程站点载入数据时，显示的一条快捷信息(只有当值为undefined时，才使用系统默认提示)
			loadMsg: objParams.loadMsg,
			// 设置true将在数据表格底部显示分页工具栏
			pagination: (objParams.pagination === undefined) ? true : objParams.pagination,
			// 设置为true将显示行数
			rownumbers: (objParams.rownumbers === undefined) ? true : objParams.rownumbers,
			// 设置为true将只允许选择一行
			singleSelect: (objParams.singleSelect === undefined) ? false : objParams.singleSelect,
			// 如果为true，该复选框被选中/取消选中，当用户点击某一行上。如果为false，该复选框仅检查/取消选中，当用户点击完全的复选框
			checkOnSelect: (objParams.checkOnSelect === undefined) ? true : objParams.checkOnSelect,
			// 当设置分页属性时，初始化分页码
			pageNumber: (objParams.pageNumber === undefined) ? 1 : objParams.pageNumber,
			// 如果设置为true，单击一个复选框，将始终选择行。如果为false，不会选择行选中该复选框
			selectOnCheck: (objParams.selectOnCheck === undefined) ? true : objParams.selectOnCheck,
			// 当设置分页属性时，初始化每页记录数
			pageSize: (objParams.pageSize === undefined) ? 10 : objParams.pageSize,
			// 当设置分页属性时，初始化每页记录数列表
			pageList: (objParams.pageList === undefined) ? [10, 15, 20, 25, 30, 35, 40, 45, 50] : objParams.pageList,
			// 当请求远程数据时，发送的额外参数
			queryParams: objParams.queryParams || {},
			// 当数据表格初始化时以哪一列来排序
			sortName: (objParams.sortName === undefined) ? null : objParams.sortName,
			// 定义排序顺序，可以是'asc'或者'desc'（正序或者倒序）
			sortOrder: (objParams.sortOrder === undefined) ? null : objParams.sortOrder,
			//多列排序
			multiSort: (objParams.multiSort === undefined) ? false : objParams.multiSort,
			// 定义是否通过远程服务器对数据排序
			remoteSort: (objParams.remoteSort === undefined) ? false : objParams.remoteSort,
			// 定义是否显示行底
			showFooter: (objParams.showFooter === undefined) ? false : objParams.showFooter,
			//当定义时显示可折叠面板的按钮。默认false
			collapsible: (objParams.collapsible === undefined) ? false : objParams.collapsible,
			// 返回样式
			rowStyler: (typeof (objParams.rowStyler) === 'function') ? objParams.rowStyler : null,
			//成功加载后
			onLoadSuccess: (typeof (objParams.onLoadSuccess) === 'function') ? objParams.onLoadSuccess : function () { },
			// 定义如何从远程服务器加载数据。返回false可以取消该操作
			/* loader: function () { }, */
			// 返回过滤的数据显示。该函数需要一个参数'data'，表示原始数据。您可以更改源数据的标准数据格式。此函数必须返回标准数据对象中包含的“total”和“rows”的属性
			/* loadFilter: function () { }, */
			onLoadError: function () {
				artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.List.onLoadErrorText);
			},
			/* 行点击时只能单选 */
			onClickRow: function (index, row) {
				objGrid.datagrid("uncheckAll");
				objGrid.datagrid("checkRow", index);
				// 记录最后选中行
				if (typeof (objParams.onClickRow) === 'function') {
					objParams.onClickRow(index, row);
				}
			},
			onSelect: function (index, row) {
				// 记录最后选中行
				_easyUI_LastSelectedRow = row;
			}
		});
		// 设置分页控件
		//objGrid.datagrid('getPager').pagination({
		//	beforePageText : '',
		//	afterPageText : '/{pages}',
		//	displayMsg : 'from {from} to {to},  total {total}'
		//});

		/**扩展**/
		//如果是允许进行cell编辑,需要加载datagrid-cellediting.js插件
		if (objParams.cellEdit) {
			$O.datagrid('enableCellEditing');
		}
	},
	Search: function (objGrid, objParams) {
		objGrid.datagrid('load', objParams);
	},
	// 获取DataGrid的checkbox的最后选中行
	SingleRowSelect: function (objGrid) {
		var _select_row = null;
		var _rows = objGrid.datagrid('getChecked');
		if (_rows) {
			if (_rows.length > 0) {
				// 默认为最后选中行
				if (_easyUI_LastSelectedRow) {
					_select_row = _easyUI_LastSelectedRow;
				} else {
					_select_row = _rows[0];
				}
			}
		}
		return _select_row;
	},
	// 获取DataGrid的checkbox选中行集合
	ComplexRowSelect: function (objGrid) {
		return objGrid.datagrid('getChecked');
	},
	// 获取DataGrid的checkbox选中数组集合
	ComplexSelect: function (objGrid) {
		var _rows = objGrid.datagrid('getChecked');
		var _id_array = new Array();
		for (var i = 0; i < _rows.length; i++) {
			_id_array.push(_rows[i].ck);
		}
		return _id_array;
	},
	// 重新加载DataGrid
	Refresh: function (objGrid) {
		objGrid.datagrid('reload');
	},
	// 新开窗口
	OpenDialog: function (config) {
		artDialogExtend.Dialog.Open(config.url, {
			title: (config.title) ? config.title : '',
			width: (config.width) ? config.width : '100%',
			height: (config.height) ? config.height : '100%',
			postWidth: (config.postWidth === undefined) ? false : config.postWidth,
			close: (typeof (config.close) === 'function') ? config.close : function () {
				if ($('#dg')) {
					easyUIExtend.Grid.Refresh($('#dg'));
				}
			}
		});
	},
	// 新开保存窗口
    /*
     * url 弹出窗口地址
     * title 弹出窗口标题
     * width 弹出窗口长度
     * height 弹出窗口高度
     * postWidth 是否传递窗体长度
     * okVal 弹出窗口操作按钮名称
     * ok 点击确认事件
     * confirm 是否有提示框
    */
	SaveDialog: function (config) {
		artDialogExtend.Dialog.Open(config.url, {
			title: (config.title === undefined) ? '' : config.title,
			width: (config.width === undefined) ? '100%' : config.width,
			height: (config.height === undefined) ? '100%' : config.height,
			postWidth: (config.postWidth === undefined) ? false : config.postWidth,
			okVal: (config.okVal === undefined) ? $.fn.easyUIExtend.Grid.SaveDialog.defaultbuttonText : config.okVal,
			ok: (typeof (config.ok) === 'function') ? config.ok : function () {
				$form = $(window.frames[0].document).find("form");
				if ($form.length > 0) {
					$this = this;
					$button = $(window.document).find('button');
					//是否有提示框,默认无
					if (config.confirm === undefined) {
						// 绑定子页面提交信息
						easyUIExtend.SubForm({
							form: $form,
							url: config.dataUrl,
							button: $button,
							iswin: true,
							dialog: $this,
							success: (typeof (config.success) === 'function') ? config.success : null
						});
					}
					else {
						artDialogExtend.Confirm($.fn.easyUIExtend.Grid.SaveDialog.confirmText, function () {
							// 绑定子页面提交信息
							easyUIExtend.SubForm({
								form: $form,
								url: config.dataUrl,
								button: $button,
								iswin: true,
								dialog: $this,
								success: (typeof (config.success) === 'function') ? config.success : null
							});
						}, function () { return true });
					}

				}
				return false;
			},
			cancelVal: '',
			cancel: true,
			close: (typeof (config.close) === 'function') ? config.close : function () {
				if ($('#dg')) {
					easyUIExtend.Grid.Refresh($('#dg'));
				}
			}
		});
	},
	// 单选操作,同时新开保存窗口
	SaveOper: function (object, config) {
		var _row = easyUIExtend.Grid.SingleRowSelect(object);
		if (_row) {
			config.url = config.url.replace('$', _row.ck);
			easyUIExtend.Grid.SaveDialog({
				url: config.url,
				title: config.title,
				width: config.width,
				height: config.height,
				dataUrl: config.dataUrl,
				okVal: config.okVal,
				confirm: config.confirm
			});
			var _index = object.datagrid("getRowIndex", _row);
			object.datagrid("uncheckAll");
			object.datagrid("checkRow", _index);
		} else {
			artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.SaveOper.messageAltText);
		}
	},
	// 多选操作,同时新开保存窗口
	ComplexSaveOper: function (object, config) {
		var _rows = easyUIExtend.Grid.ComplexSelect(object);
		if (_rows) {
			if (_rows.length == 0) {
				artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.ComplexSaveOper.messageAltText);
			} else {
				var _id_str = _rows.join(",");
				config.url = config.url.replace('$', encodeURI(_id_str));

				easyUIExtend.Grid.SaveDialog({
					url: config.url,
					title: config.title,
					width: config.width,
					height: config.height,
					dataUrl: config.dataUrl,
					okVal: config.okVal,
					confirm: config.confirm
				});
			}
		}
	},
	// 单选操作,同时新开窗口
	SingleOper: function (object, config) {
		var _row = easyUIExtend.Grid.SingleRowSelect(object);
		if (_row) {
			config.url = config.url.replace('$', _row.ck);
			easyUIExtend.Grid.OpenDialog({
				url: config.url,
				title: config.title,
				width: config.width,
				height: config.height
			});
			var _index = object.datagrid("getRowIndex", _row);
			object.datagrid("uncheckAll");
			object.datagrid("checkRow", _index);
		} else {
			artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.SingleOper.messageAltText);
		}
	},
	// 选中多条数据,执行ajax操作
	ComplexOper: function (object, objurl) {
		var _rows = easyUIExtend.Grid.ComplexSelect(object);
		if (_rows) {
			if (_rows.length == 0) {
				artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.ComplexOper.messageAltText);
			} else {
				artDialogExtend.Confirm($.fn.easyUIExtend.Grid.ComplexOper.confirmText, function () {
					var _id_str = _rows.join(",");
					var _data = {
						id: _id_str
					}
					//防御CSRF验证
					if (verificationTokenJs.Enabled) {
						_data.__RequestVerificationToken = verificationTokenJs.getVerificationToken();
					}
					$.ajax({
						url: objurl,
						type: 'post',
						data: _data,
						dataType: 'json',
						beforeSend: function (XMLHttpRequest) {
							artDialogExtend.Loading.Load();
						},
						error: function (XMLHttpRequest, textStatus, errorThrown) {
							artDialogExtend.Loading.Close();
							artDialogExtend.Tips.Error($.fn.easyUIExtend.Grid.ComplexOper.errorText, 5);
						},
						success: function (data, textStatus) {
							artDialogExtend.Loading.Close();
							if (data.result) {
								artDialogExtend.Tips.Success(data.msg, 2, function () {
									easyUIExtend.Grid.Refresh(object);
								});
							} else {
								artDialogExtend.Tips.Alert(data.msg, 5, function () {
									easyUIExtend.Grid.Refresh(object);
								});
							}
						}
					});
				}, function () {
					return true;
				});
			}
		}
	},
	//获取DataGrid的checkbox的选项，如果是单条则填出操作框，否则批量操作,操作提交页面均为dataUrl
	SingleRowOrComplexOper: function (objGrid, config) {
		var _rows = objGrid.datagrid('getChecked');
		if (_rows) {
			if (_rows.length == 1) {
				_row = _rows[0];
				config.url = config.url.replace('$', _row.ck);
				easyUIExtend.Grid.SaveDialog({
					url: config.url,
					title: config.title,
					width: config.width,
					height: config.height,
					dataUrl: config.dataUrl,
					okVal: config.okVal,
					confirm: config.confirm
				});
			}
			else {
				easyUIExtend.Grid.ComplexOper(objGrid, config.dataUrl);
			}
		}
		else {
			artDialogExtend.Message.Alert($.fn.easyUIExtend.Grid.SingleRowOrComplexOper.messageAltText);
		}
	},
	// 简易操作
	CommonOper: function (object, objurl, objpara) {
		objpara = objpara || {};
		//防御CSRF验证
		if (verificationTokenJs.Enabled) {
			objpara.__RequestVerificationToken = verificationTokenJs.getVerificationToken();
		}
		if (object) {
			$.ajax({
				url: objurl,
				type: 'post',
				data: objpara,
				dataType: 'json',
				beforeSend: function (XMLHttpRequest) {
					artDialogExtend.Loading.Load();
				},
				error: function (XMLHttpRequest, textStatus, errorThrown) {
					artDialogExtend.Loading.Close();
					artDialogExtend.Tips.Error($.fn.easyUIExtend.Grid.CommonOper.errorText, 5);
				},
				success: function (data, textStatus) {
					artDialogExtend.Loading.Close();
					if (data.result) {
						easyUIExtend.Grid.Refresh(object);
					} else {
						artDialogExtend.Tips.Alert(data.msg, 5, function () {
							easyUIExtend.Grid.Refresh(object);
						});
					}
				}
			});
		}
	}
}

// GridExtension扩展
easyUIExtend.GridExtension = {
	DetailView: function (objGrid, objParams) {
		objParams = objParams || {};
		// 绑定列表
		objGrid.datagrid({
			// 宽度
			width: objParams.width,
			// 高度
			height: (objParams.height === undefined) ? $(document).height() : objParams.height,
			// 自动适屏功能
			fit: (objParams.fit === undefined) ? false : objParams.fit,
			// 数据表格列配置对象
            /*
             * columns: [[{ field: '0', checkbox: true },{ field: '', title: ''
             * }]],
             */
			// 跟列属性一样，但是这些列固定在左边，不会滚动
			frozenColumns: (objParams.frozenColumns === undefined) ? [[]] : objParams.frozenColumns,
			// 设置为true将自动使列适应表格宽度以防止出现水平滚动
			fitColumns: (objParams.fitColumns === undefined) ? true : objParams.fitColumns,
			// 定义设置行的高度，根据该行的内容。设置为false可以提高负载性能
			autoRowHeight: (objParams.autoRowHeight === undefined) ? true : objParams.autoRowHeight,
			// 数据表格顶部面板的工具栏
			toolbar: objParams.toolbar,
			// 设置为true将交替显示行背景
			striped: (objParams.striped === undefined) ? false : objParams.striped,
			// 请求远程数据的方法类型
			method: (objParams.method === undefined) ? 'post' : objParams.method,
			// 设置为true，当数据长度超出列宽时将会自动截取
			nowrap: (objParams.nowrap === undefined) ? true : objParams.nowrap,
			// 表明该列是一个唯一列
			/* idField :null, */
			// 一个用以从远程站点请求数据的超链接地址
			url: objParams.url,
			// 当从远程站点载入数据时，显示的一条快捷信息
			loadMsg: objParams.loadMsg,
			// 设置true将在数据表格底部显示分页工具栏
			pagination: (objParams.pagination === undefined) ? true : objParams.pagination,
			// 设置为true将显示行数
			rownumbers: (objParams.rownumbers === undefined) ? true : objParams.rownumbers,
			// 设置为true将只允许选择一行
			singleSelect: (objParams.singleSelect === undefined) ? false : objParams.singleSelect,
			// 如果为true，该复选框被选中/取消选中，当用户点击某一行上。如果为false，该复选框仅检查/取消选中，当用户点击完全的复选框
			checkOnSelect: (objParams.checkOnSelect === undefined) ? true : objParams.checkOnSelect,
			// 当设置分页属性时，初始化分页码
			pageNumber: (objParams.pageNumber === undefined) ? 1 : objParams.pageNumber,
			// 如果设置为true，单击一个复选框，将始终选择行。如果为false，不会选择行选中该复选框
			selectOnCheck: (objParams.selectOnCheck === undefined) ? true : objParams.selectOnCheck,
			// 当设置分页属性时，初始化每页记录数
			pageSize: (objParams.pageSize === undefined) ? 15 : objParams.pageSize,
			// 当设置分页属性时，初始化每页记录数列表
			pageList: (objParams.pageList === undefined) ? [10, 15, 20, 25, 30, 35, 40, 45, 50] : objParams.pageList,
			// 当请求远程数据时，发送的额外参数
			queryParams: objParams.queryParams || {},
			// 当数据表格初始化时以哪一列来排序
			sortName: (objParams.sortName === undefined) ? null : objParams.sortName,
			// 定义排序顺序，可以是'asc'或者'desc'（正序或者倒序）
			sortOrder: (objParams.sortOrder === undefined) ? null : objParams.sortOrder,
			//多列排序
			multiSort: (objParams.multiSort === undefined) ? false : objParams.multiSort,
			// 定义是否通过远程服务器对数据排序
			remoteSort: (objParams.remoteSort === undefined) ? false : objParams.remoteSort,
			// 定义是否显示行底
			showFooter: (objParams.showFooter === undefined) ? false : objParams.showFooter,
			//当定义时显示可折叠面板的按钮。默认false
			collapsible: (objParams.collapsible === undefined) ? false : objParams.collapsible,
			// 返回样式
			/* rowStyler: funcion(){ }, */
			// 定义如何从远程服务器加载数据。返回false可以取消该操作
			/* loader: function () { }, */
			// 返回过滤的数据显示。该函数需要一个参数'data'，表示原始数据。您可以更改源数据的标准数据格式。此函数必须返回标准数据对象中包含的“total”和“rows”的属性
			/* loadFilter: function () { }, */
			view: detailview,
			onLoadError: function () {
				artDialogExtend.Message.Alert($.fn.easyUIExtend.GridExtension.DetailView.onLoadErrorText);
			},
			detailFormatter: function (index, row) {
				return '<div class="datagridview" style="padding:5px 0;"></div>';
			},
			onExpandRow: function (index, row) {
				var $ddv = $(this).datagrid('getRowDetail', index).find('div.datagridview');
				$ddv.panel({
					height: (objParams.detail_height === undefined) ? null : objParams.detail_height,
					border: false,
					cache: true,
					href: objParams.detail_url + '?id=' + row.ck,
					onLoad: function () {
						objGrid.datagrid('fixDetailRowHeight', index);
					}
				});
				objGrid.datagrid('fixDetailRowHeight', index);
			},
			/* 行点击时只能单选 */
			onClickRow: function (index, row) {
				objGrid.datagrid("uncheckAll");
				objGrid.datagrid("checkRow", index);
				// 记录最后选中行
				_easyUI_LastSelectedRow = row;
			},
			onSelect: function (index, row) {
				// 记录最后选中行
				_easyUI_LastSelectedRow = row;
			}
		});
	}
}

//下拉Grid
easyUIExtend.ComboGrid = function (objComboGrid, objParams) {
	objComboGrid.combogrid({
		panelWidth: objParams.panelWidth,
		url: objParams.url,
		idField: objParams.idField,
		textField: objParams.textField,
		queryParams: objParams.queryParams || {},
		mode: 'remote',
		striped: (objParams.striped === undefined) ? false : objParams.striped,
		rownumbers: (objParams.rownumbers === undefined) ? true : objParams.rownumbers,
		pagination: (objParams.pagination === undefined) ? true : objParams.pagination,
		multiple: (objParams.multiple === undefined) ? false : objParams.multiple,
		fitColumns: true,
		pageSize: (objParams.pageSize === undefined) ? 15 : objParams.pageSize,
		pageList: (objParams.pageList === undefined) ? [10, 15, 20, 25, 30, 35, 40, 45, 50] : objParams.pageList,
		columns: (objParams.columns === undefined) ? [[]] : objParams.columns,
		onClickRow: (typeof (objParams.onClickRow) === 'function') ? objParams.onClickRow : function () { }
	});
}

// easyUI提交表单
/*
 * form 表单对象
 * url 操作地址
 * button 按钮对象
 * success 成功操作方法
 * iswin 是否是弹出框
 * dialog 如果是弹出框,则为窗体对象
*/
easyUIExtend.SubForm = function (config) {
	var _queryParams = {};
	//防御CSRF验证
	if (verificationTokenJs.Enabled) {
		_queryParams.__RequestVerificationToken = verificationTokenJs.getVerificationToken();
	}
	config.form.form('submit', {
		url: config.url,
		queryParams: _queryParams,
		onSubmit: function () {
			artDialogExtend.Loading.Load();
			if (config.button) {
				config.button.html($.fn.easyUIExtend.SubForm.defaultbuttonText + '...');
				config.button.prop("disabled", true);
			}
		},
		success: (typeof (config.success) === 'function') ? config.success : function (data) {
			artDialogExtend.Loading.Close();
			if (config.button) {
				config.button.html($.fn.easyUIExtend.SubForm.defaultbuttonText);
				config.button.prop("disabled", false);
			}
			// 转换成json格式
			try {
				data = eval('(' + data + ')');
				if (data.result) {
					if (config.iswin) {
						artDialogExtend.Tips.Success(data.msg, 2, function () {
							config.dialog.close();
						});
					}
					else {
						artDialogExtend.Tips.Success(data.msg, 2);
					}
				} else {
					artDialogExtend.Tips.Alert(data.msg, 5);
				}
			} catch (e) {
				artDialogExtend.Tips.Error($.fn.easyUIExtend.SubForm.errorText, 5);
			}
		}
	});
}

//新建菜单栏
easyUIExtend.AddMenuTab = function (objTitle, objUrl, objReload) {
	//最大iframe数
	var _max = 10;
	var $o = $('#tabs');
	var $tabs = $o.tabs("tabs");
	if (!$o.tabs('exists', objTitle)) {
		//关闭最早的iframe
		if ($tabs.length >= _max) { $o.tabs("close", 0) };
		$o.tabs('add', {
			title: objTitle,
			content: createFrame(objUrl),
			closable: true,
			style: '{overflow:hidden}',
			width: $('#mainPanle').width() - 10,
			height: $('#mainPanle').height() - 26
		});
	} else {
		$o.tabs('select', objTitle);
		//是否需要重新加载
		if (objReload) {
			//var _O = $('#tabs').tabs('getTab', objTitle);
			//var _url = _O.find('iframe[name=mainFrame]').attr("src");
			$('#tabs').tabs('update', {
				tab: $('#tabs').tabs('getSelected'),
				options: {
					content: createFrame(objUrl)
				}
			});
		}
	}
}

function createFrame(objUrl) {
	return '<iframe name="mainFrame" scrolling="auto" frameborder="0" src="' + objUrl + '" style="width:100%;height:99%;"></iframe>';
}

// 解决弹出框iframe中的datagrid不能取到正确宽度
easyUIExtend.IframeGrid = {
	Width: function () {
		return $.requestQueryString("iwidth");
	},
	Height: function () {
		return $.requestQueryString("iheight");
	}
}


// 自定义easyui控件编译
easyUIExtend.Control = function () {
	return {
		ChooseBox: function () {
			var $obj = $("input[class^=easyui-choosebox]");
			if ($obj.length > 0) {
				var _name, _width, _height, _prompt, _click, _close, _value, _val, _text, _text_width;
				var _str = "";
				for (var i = 0; i < $obj.length; i++) {
					_name = ($obj.eq(i).attr("name")) ? $obj.eq(i).attr("name")
						: "";
					_width = $obj.eq(i).width();
					_prompt = ($obj.eq(i).attr("prompt")) ? $obj.eq(i).attr(
						"prompt") : "";
					_click = $obj.eq(i).attr("onClickButton");
					_close = (!$obj.eq(i).attr("showClose")) ? true : (($obj
						.eq(i).attr("showClose") == "true") ? true : false);
					_text_width = (_close) ? _width - 44 : _width - 28;
					_value = $obj.eq(i).val();
					if (!_value.match("^\{(.+:.+,*){1,}\}$")) {
						_val = _value;
						_text = _value;
					} else {
						_value = JSON.parse(_value);
						_val = _value.value;
						_text = _value.text;
					}
					// 隐藏控件
					$obj.eq(i).css("display", "none");
					$obj.eq(i).removeAttr("name");
					$obj.eq(i).attr("textboxname", _name);
					// 生成控件代码
					// 创建span,注ie下class为敏感变量需要写成"class"
					var $span_whole = $("<span>", {
						"class": "easyui_exd-choosebox",
						style: "width:" + _width + "px;"
					}).insertAfter($obj.eq(i));
					// 创建点击框
					$("<a>", {
						"class": "button",
						href: "javascript:void(0);",
						html: "<span><label>...</label></span>",
						onclick: (_click) ? _click : ";"
					}).appendTo($span_whole);
					// 创建关闭框
					if (_close) {
						$("<a>",
							{
								href: "javascript:void(0);",
								"class": "close",
								title: "Click to clear",
								click: function () {
									$(this).parent().find(
										'input[type=text]').val("");
									$(this).parent().find(
										'input[type=hidden]').val("");
								},
								html: '<i class="fa fa-close"></i>'
							}).appendTo($span_whole);
					}
					// 创建显示框
					$("<input>", {
						"class": "text",
						style: "width:" + _text_width + "px",
						type: "text",
						autocomplete: "off",
						placeholder: _prompt,
						value: _text,
						readonly: "readonly"
					}).appendTo($span_whole);
					// 创建隐藏域值
					$("<input>", {
						name: _name,
						type: "hidden",
						value: _val
					}).appendTo($span_whole);
				}
			}
		}
	};
}();

// 自定义easyui控件扩展方法
$.fn.extend({
	choosebox: function (action, value) {
		if ($(this)) {
			switch (action) {
				case "setValue":
					$(this).next('span[class="easyui_exd-choosebox"]').find('input[type=hidden]').val(value);
					break;
				case "setText":
					$(this).next('span[class="easyui_exd-choosebox"]').find('input[type=text]').val(value);
					break;
				case "clear":
					$(this).next('span[class="easyui_exd-choosebox"]').find('input[type=text]').val("");
					$(this).next('span[class="easyui_exd-choosebox"]').find('input[type=hidden]').val("");
					break;
				case "getValue":
					return $(this).next('span[class="easyui_exd-choosebox"]').find('input[type=hidden]').val();
					break;
				case "getText":
					return $(this).next('span[class="easyui_exd-choosebox"]').find('input[type=text]').val();
					break;
				default:
					break;
			}
		}
	}
});
