var verificationTokenJs = {};

/***是否开启防御CSRF验证***/
verificationTokenJs.Enabled = false;

/***存储Token的input名称***/
verificationTokenJs.Key = '__RequestVerificationToken';

/**获取验证用Token**/
verificationTokenJs.getVerificationToken = function () {
	var _value = $('input[name=' + verificationTokenJs.Key).val();
	if (!_value) _value = '';
	return _value;
}