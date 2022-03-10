/*会员中心导航固定*/
if($('.main-menu').length>0)
{
	mobile.HeadFixed($('.main-menu'));
}

/* 头部返回上一页按钮 */
var $header_go_back = $('#header-goback');
if ($header_go_back.length > 0) {
	$header_go_back.on('click', function() {
	    mobile.pageBack();
	});
}
/* 回到首页按钮 */
mobile.ScrollSpyTop();

/* 自定义控件 */
//bootstrapExtend.defineControl.Text();