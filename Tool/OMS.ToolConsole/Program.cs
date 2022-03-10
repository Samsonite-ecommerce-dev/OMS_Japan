using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.ECommerce;
using Samsonite.OMS.ECommerce.Interface;
using Samsonite.OMS.ECommerce.Dto;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.Materials;
using Samsonite.OMS.Service.Sap.PIM;
using Samsonite.OMS.Service.Sap.WMSInventory;
using Samsonite.OMS.Service.Sap.Poslog;

using OMS.ToolConsole.Encryption;
using Samsonite.OMS.DTO.Sap;
using Samsonite.OMS.Service.Reserve;

namespace OMS.ToolConsole
{
    class Program : ECommerceBaseService
    {
        static void Main(string[] args)
        {
            Menu();
        }

        public static void Menu()
        {
            //菜单列表
            MenuList();
        }

        private static void MenuList()
        {
            Console.WriteLine(">>Function Option:");
            Console.WriteLine("----------SAP----------");
            Console.WriteLine("1.Down Product from SAP");
            Console.WriteLine("2.Down Price from SAP");
            Console.WriteLine("3.Down Image/Length/Height/Weight from PIM");
            Console.WriteLine("----------Product----------");
            Console.WriteLine("11.Down Product from E-Commerce platform");
            Console.WriteLine("----------Order----------");
            Console.WriteLine("21.Down Order from ECommerce platform");
            Console.WriteLine("22.Down Increment Order from E-Commerce platform");
            Console.WriteLine("----------Claim----------");
            Console.WriteLine("31.Down Order Claim from E-Commerce platform");
            Console.WriteLine("----------WMS Invenroty----------");
            Console.WriteLine("41.Down Invenroty from WMS");
            Console.WriteLine("----------Delivery Tracking No.----------");
            //Console.WriteLine("51.Apply Delivery Tracking No. from E-Commerce platform/Express");
            //Console.WriteLine("52.Push Ready Ship to E-Commerce platform/Express");
            Console.WriteLine("53.Get the information from E-Commerce platform/Express");
            Console.WriteLine("----------Iventory----------");
            Console.WriteLine("61.Send inventory to E-Commerce platform");
            Console.WriteLine("62.Send warning inventory to E-Commerce platform");
            Console.WriteLine("----------Price----------");
            Console.WriteLine("71.Send price to E-Commerce platform");
            Console.WriteLine("----------OrderDetail----------");
            Console.WriteLine("81.Send orderDetail to Demandware/Tumi");
            Console.WriteLine("----------Poslog----------");
            Console.WriteLine("91.Send Poslog to SAP");
            Console.WriteLine("----------Reserve----------");
            Console.WriteLine("201.Send Reserve Order");
            Console.WriteLine("----------Sales Statistics----------");
            Console.WriteLine("301.Daily order sales statistics");
            Console.WriteLine("302.Daily product sales statistics");
            Console.WriteLine("----------Version update assist----------");
            Console.WriteLine("901.Insert MallDetail");
            Console.WriteLine("902.Get missed document");
            //Console.WriteLine("903.Convert ducument PDF to image");
            //Console.WriteLine("----------Data Encrypt----------");
            //Console.WriteLine("301.MSSQL Table Encrypt");
            //Console.WriteLine("302.DefaultEncrytPwd");
            Console.WriteLine("Please select a option...");
            int _SelectID = VariableHelper.SaferequestInt(Console.ReadLine());
            MenuOption(_SelectID);
        }

        public static void MenuOption(int objOptionID)
        {
            //清空屏幕
            Console.Clear();
            switch (objOptionID)
            {
                case 1:
                    GetDataFromSAP();
                    break;
                case 2:
                    GetPriceFromSAP();
                    break;
                case 3:
                    GetFromPIM();
                    break;
                case 11:
                    GetProduct();
                    break;
                case 21:
                    GetOrder();
                    break;
                case 22:
                    GetIncrementOrder();
                    break;
                case 31:
                    GetClaim();
                    break;
                case 41:
                    GetWMSInventory();
                    break;
                //case 51:
                //    RequireDeliveryInvoice();
                //    break;
                //case 52:
                //    PushReadyShip();
                //    break;
                case 53:
                    GetExpress();
                    break;
                case 61:
                    SendInventory();
                    break;
                case 62:
                    SendWarningInventory();
                    break;
                case 71:
                    SendPrice();
                    break;
                case 81:
                    SendOrderDetail();
                    break;
                case 91:
                    SendPoslog();
                    break;
                case 201:
                    SendReserveOrder();
                    break;
                case 301:
                    OrderDailyData();
                    break;
                case 302:
                    ProductDailyData();
                    break;
                //case 301:
                //    TableEncrypt();
                //    break;
                //case 302:
                //    DefaultEncrptPwd();
                //    break;
                case 901:
                    InsertMallDetail();
                    break;
                case 902:
                    GetMissedDocument();
                    break;
                //case 903:
                //    ConvertDucumentPDFToImage();
                //    break;
                default:
                    Console.WriteLine("Option incorrect!");
                    break;
            }
        }

        #region SAP
        public static void GetDataFromSAP()
        {
            try
            {
                Console.WriteLine("**********Update Samsonite's Material from SAP.**********");
                Console.WriteLine($"Begin to down,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    FTPResult _eanFtpFiles = MaterialService.DownEANFileFormSAP(MaterialConfig.SamsoniteFtpConfig);
                    CommonResult _result = new CommonResult();
                    for (var i = 0; i < _eanFtpFiles.SuccessFile.Count; i++)
                    {
                        try
                        {
                            CommonResult _f = MaterialService.ReadToSaveEAN(_eanFtpFiles.SuccessFile[i]);
                            _result.TotalRecord += _f.TotalRecord;
                            _result.SuccessRecord += _f.SuccessRecord;
                            _result.FailRecord += _f.FailRecord;
                            //不显示文件全路径
                            _eanFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]);
                        }
                        catch
                        {
                            //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                            _eanFtpFiles.FailFile.Add(FileHelper.GetFileName(_eanFtpFiles.SuccessFile[i]));
                            //删除错误文件
                            _eanFtpFiles.SuccessFile.Remove(_eanFtpFiles.SuccessFile[i]);
                        }
                    }
                    //删除重复SKU
                    MaterialService.CheckRepeatSku();
                    Console.WriteLine($"Samsonite:Material File download success:[{string.Join(",", _eanFtpFiles.SuccessFile)}],fail:[{string.Join(",", _eanFtpFiles.FailFile)}],Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.");
                }
                else
                {
                    GetDataFromSAP();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        public static void GetPriceFromSAP()
        {
            try
            {
                Console.WriteLine("**********Update Samsonite's Price from SAP.**********");
                Console.WriteLine($"Begin to down,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    FTPResult _priceFtpFiles = MaterialService.DownPriceFileFormSAP(MaterialConfig.SamsoniteFtpConfig);
                    CommonResult _result = new CommonResult();
                    for (var i = 0; i < _priceFtpFiles.SuccessFile.Count; i++)
                    {
                        try
                        {
                            CommonResult _f = MaterialService.ReadToSavePrice(_priceFtpFiles.SuccessFile[i]);
                            _result.TotalRecord += _f.TotalRecord;
                            _result.SuccessRecord += _f.SuccessRecord;
                            _result.FailRecord += _f.FailRecord;
                            //不显示文件全路径
                            _priceFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]);
                        }
                        catch
                        {
                            //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中 
                            _priceFtpFiles.FailFile.Add(FileHelper.GetFileName(_priceFtpFiles.SuccessFile[i]));
                            //删除错误文件
                            _priceFtpFiles.SuccessFile.Remove(_priceFtpFiles.SuccessFile[i]);
                        }
                    }
                    Console.WriteLine($"Samsonite:Price File download success:[{string.Join(",", _priceFtpFiles.SuccessFile)}],fail:[{string.Join(",", _priceFtpFiles.FailFile)}],Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.");
                }
                else
                {
                    GetPriceFromSAP();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        public static void GetFromPIM()
        {
            try
            {
                Console.WriteLine("**********Update Samsonite's PIM from SAP.**********");
                Console.WriteLine($"Begin to down,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    FTPResult _pimFtpFiles = PIMService.DownPIMFileFormSAP(PIMConfig.SamsoniteFtpConfig);
                    CommonResult _result = new CommonResult();
                    for (var i = 0; i < _pimFtpFiles.SuccessFile.Count; i++)
                    {
                        try
                        {
                            CommonResult _f = PIMService.ReadToSavePIM(_pimFtpFiles.SuccessFile[i]);
                            _result.TotalRecord += _f.TotalRecord;
                            _result.SuccessRecord += _f.SuccessRecord;
                            _result.FailRecord += _f.FailRecord;
                            //不显示文件全路径
                            _pimFtpFiles.SuccessFile[i] = FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]);
                        }
                        catch
                        {
                            //如果文件读取失败,则从正确文件中删除,并放到错误文件列表中
                            _pimFtpFiles.FailFile.Add(FileHelper.GetFileName(_pimFtpFiles.SuccessFile[i]));
                            //删除错误文件
                            _pimFtpFiles.SuccessFile.Remove(_pimFtpFiles.SuccessFile[i]);
                        }
                    }
                    Console.WriteLine($"Samsonite:PIM File download success:[{string.Join(",", _pimFtpFiles.SuccessFile)}],fail:[{string.Join(",", _pimFtpFiles.FailFile)}],Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.");
                }
                else
                {
                    GetFromPIM();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 产品
        /// <summary>
        /// 下载产品
        /// </summary>
        public static void GetProduct()
        {
            try
            {
                Console.WriteLine("**********Down Product from ECommerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList();
                Console.WriteLine("Input a Mall Serial Number(If need all store. Please enter \"0\"):");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (_MallSapCode.ToUpper() == "ALL")
                    {
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs());
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }
                    //循环操作
                    foreach (var api in Malls)
                    {
                        List<ItemDto> objItemDto_List = api.GetItems();
                        if (objItemDto_List != null)
                        {
                            //防止某种情况下没有成功读取到产品集合
                            if (objItemDto_List.Count > 0)
                            {
                                //全部设置下架
                                ECommerceBaseService.SetItemsOffSale(api.StoreSapCode());
                            }
                            //保存产品
                            var _result = ECommerceBaseService.SaveItems(objItemDto_List);
                            //计算当前价格
                            ECommerceBaseService.CalculateMallSkuSalesPrice(api.StoreSapCode());
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetProduct();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 订单
        /// <summary>
        /// 下载订单
        /// </summary>
        public static void GetOrder()
        {
            try
            {
                Console.WriteLine("**********Down Order from ECommerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList();
                Console.WriteLine("Input a Mall Serial Number(If need all store. Please enter \"0\"):");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine("Input a start time(yyyy-MM-dd HH:mm:ss):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd HH:mm:ss):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Mall:{_MallSapCode},Time Quantum:{_BeginTime.ToString("yyyy/MM/dd HH:mm:ss")}-{_EndTime.ToString("yyyy/MM/dd HH:mm:ss")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (_MallSapCode.ToUpper() == "ALL")
                    {
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs());
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }
                    //循环操作
                    foreach (var api in Malls)
                    {
                        List<TradeDto> objTradeDto_List = api.GetTrades(_BeginTime, _EndTime);
                        if (objTradeDto_List != null)
                        {
                            var _result = ECommerceBaseService.SaveTrades(objTradeDto_List);
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetOrder();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 下载增益订单
        /// </summary>
        public static void GetIncrementOrder()
        {
            try
            {
                Console.WriteLine("**********Down Increment Order from ECommerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList();
                Console.WriteLine("Input a Mall Serial Number(If need all store. Please enter \"0\"):");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine("Input a start time(yyyy-MM-dd HH:mm:ss):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd HH:mm:ss):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Mall:{_MallSapCode},Time Quantum:{_BeginTime.ToString("yyyy/MM/dd HH:mm:ss")}-{_EndTime.ToString("yyyy/MM/dd HH:mm:ss")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (_MallSapCode.ToUpper() == "ALL")
                    {
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs());
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }
                    //循环操作
                    foreach (var api in Malls)
                    {
                        List<TradeDto> objTradeDto_List = api.GetIncrementTrades(_BeginTime, _EndTime);
                        if (objTradeDto_List != null)
                        {
                            var _result = ECommerceBaseService.SaveTrades(objTradeDto_List);
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetIncrementOrder();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region Claim
        /// <summary>
        /// 下载Claim
        /// </summary>
        public static void GetClaim()
        {
            try
            {
                Console.WriteLine("**********Down Order Claim from ECommerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList();
                Console.WriteLine("Input a Mall Serial Number(If need all store. Please enter \"0\"):");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (_MallSapCode.ToUpper() == "ALL")
                    {
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs());
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }
                    //循环操作
                    foreach (var api in Malls)
                    {
                        CommonResult<ClaimResult> _result = new CommonResult<ClaimResult>();
                        List<ClaimInfoDto> objClaimInfoDto_List = api.GetTradeClaims();
                        if (objClaimInfoDto_List != null)
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}]");
                            //******取消订单**************************************************************************************
                            List<ClaimInfoDto> objCancelClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Cancel).ToList();
                            _result = ECommerceBaseService.SaveClaims(objCancelClaims, ClaimType.Cancel);
                            //返回信息
                            Console.WriteLine($"->Cancel Claims,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                            //******退货订单**************************************************************************************
                            List<ClaimInfoDto> objReturnClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Return).ToList();
                            _result = ECommerceBaseService.SaveClaims(objReturnClaims, ClaimType.Return);
                            //返回信息
                            Console.WriteLine($"->Return Claims,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                            //******换货订单**************************************************************************************
                            List<ClaimInfoDto> objExchangeClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Exchange).ToList();
                            _result = ECommerceBaseService.SaveClaims(objExchangeClaims, ClaimType.Exchange);
                            //返回信息
                            Console.WriteLine($"->Exchange Claims,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                            //******拒收订单***************************************************************************************
                            List<ClaimInfoDto> objRejectClaims = objClaimInfoDto_List.Where(p => p.ClaimType == ClaimType.Reject).ToList();
                            _result = ECommerceBaseService.SaveClaims(objRejectClaims, ClaimType.Reject);
                            //返回信息
                            Console.WriteLine($"->Reject Claims,Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}.");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetClaim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region WMS
        /// <summary>
        /// 从WMS下载库存
        /// </summary>
        private static void GetWMSInventory()
        {
            try
            {
                Console.WriteLine("**********Down inventory from WMS**********");
                Console.WriteLine($"Begin to down inventory from WMS,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    //开始从FTP读取库存文件,并下载到本地
                    FTPResult _samFtpFiles = WMSInventoryService.DownInventoryFileFormSAP(WMSInventoryConfig.SamsoniteFtpConfig);
                    Console.WriteLine($"Samsonite Inventory File download success:[{string.Join(",", _samFtpFiles.SuccessFile)}],fail:[{string.Join(",", _samFtpFiles.FailFile)}],");
                    FTPResult _tumiFtpFiles = WMSInventoryService.DownInventoryFileFormSAP(WMSInventoryConfig.TumiFtpConfig);
                    Console.WriteLine($"Tumi Inventory File download success:[{string.Join(",", _tumiFtpFiles.SuccessFile)}],fail:[{string.Join(",", _tumiFtpFiles.FailFile)}],");
                    //合并结果文件
                    FTPResult _inventoryFtpFiles = new FTPResult()
                    {
                        SuccessFile = new List<string>(),
                        FailFile = new List<string>()
                    };
                    _inventoryFtpFiles.SuccessFile.AddRange(_samFtpFiles.SuccessFile);
                    _inventoryFtpFiles.FailFile.AddRange(_samFtpFiles.FailFile);
                    _inventoryFtpFiles.SuccessFile.AddRange(_tumiFtpFiles.SuccessFile);
                    _inventoryFtpFiles.FailFile.AddRange(_tumiFtpFiles.FailFile);
                    CommonResult _result = new CommonResult();
                    foreach (var _str in _inventoryFtpFiles.SuccessFile)
                    {
                        CommonResult _f = WMSInventoryService.ReadToSaveInventory(_str);
                        _result.TotalRecord += _f.TotalRecord;
                        _result.SuccessRecord += _f.SuccessRecord;
                        _result.FailRecord += _f.FailRecord;
                    }
                    Console.WriteLine($"Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.");
                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetWMSInventory();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 快递号
        /// <summary>
        /// 获取快递号
        /// </summary>
        public static void RequireDeliveryInvoice()
        {
            try
            {
                Console.WriteLine("**********Delivery Tracking No. from E-Commerce platform/Express**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(false);
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //操作
                        var _result = api.RequireDeliverys();
                        if (_result != null)
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    RequireDeliveryInvoice();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 推送Ready Ship
        /// </summary>
        public static void PushReadyShip()
        {
            try
            {
                Console.WriteLine("**********Push Ready Ship to E-Commerce platform/Express**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(false);
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //操作
                        var _result = api.SendDeliverys();
                        if (_result != null)
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    PushReadyShip();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 获取快递状态或者订单状态
        /// </summary>
        public static void GetExpress()
        {
            try
            {
                Console.WriteLine("**********Get the information from E-Commerce platform/Express**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(false);
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //操作
                        var _result = api.GetExpresses();
                        if (_result != null)
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total Record:{_result.ResultData.Count},Success Record:{_result.ResultData.Where(p => p.Result).Count()},Fail Record:{_result.ResultData.Where(p => !p.Result).Count()}");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    PushReadyShip();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 库存
        /// <summary>
        /// 推送库存
        /// </summary>
        public static void SendInventory()
        {
            try
            {
                Console.WriteLine("**********Send inventory to E-Commerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(new List<int>() {(int)PlatformType.TUMI_Japan });
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //操作
                        var _result = api.SendInventorys();
                        if (_result != null)
                        {
                            if (!string.IsNullOrEmpty(_result.FileName))
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()},File Name:{_result.FileName}.");
                            }
                            else
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    SendInventory();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        public static void SendWarningInventory()
        {
            try
            {
                Console.WriteLine("**********Send warning inventory to E-Commerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(new List<int>() { (int)PlatformType.DEMANDWARE_Singapore, (int)PlatformType.TUMI_Singapore });
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //操作
                        var _result = api.SendInventorys();
                        if (_result != null)
                        {
                            if (!string.IsNullOrEmpty(_result.FileName))
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()},File Name:{_result.FileName}.");
                            }
                            else
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    SendInventory();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 价格
        /// <summary>
        /// 推送库存
        /// </summary>
        public static void SendPrice()
        {
            try
            {
                Console.WriteLine("**********Send price to E-Commerce platform**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(new List<int>() { (int)PlatformType.DEMANDWARE_Singapore, (int)PlatformType.TUMI_Singapore });
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine($"Mall:{_MallSapCode},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        //计算当前店铺销售价格
                        ECommerceBaseService.CalculateMallSkuSalesPrice(api.StoreSapCode());
                        //操作
                        var _result = api.SendPrices();
                        if (_result != null)
                        {
                            if (!string.IsNullOrEmpty(_result.FileName))
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()},File Name:{_result.FileName}.");
                            }
                            else
                            {
                                Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    SendPrice();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 预售订单
        private static void SendReserveOrder()
        {
            try
            {
                Console.WriteLine("**********Send reserve order**********");
                Console.WriteLine($"Begin to send,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var _result = ReserveService.SetReserveOrder();
                    Console.WriteLine($"Reserve send success,Total Record:{_result.TotalRecord},Success Record:{_result.SuccessRecord},Fail Record:{_result.FailRecord}.");

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    SendReserveOrder();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region OrderDetail
        private static void SendOrderDetail()
        {
            try
            {
                Console.WriteLine("**********Send orderDetail to Demandware/Tumi**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList(new List<int>() { (int)PlatformType.DEMANDWARE_Singapore, (int)PlatformType.TUMI_Singapore });
                Console.WriteLine("Input a Mall Serial Number:");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine("Input a start time(yyyy-MM-dd HH:mm:ss):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd HH:mm:ss):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Mall:{_MallSapCode},Time Quantum:{_BeginTime.ToString("yyyy/MM/dd HH:mm:ss")}-{_EndTime.ToString("yyyy/MM/dd HH:mm:ss")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    var api = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                    if (api != null)
                    {
                        var _result = api.SendOrderDetails(_BeginTime, _EndTime);
                        if (_result != null)
                        {
                            Console.WriteLine($"Time:{_BeginTime.ToString("yyyy-MM-dd")}To{_EndTime.ToString("yyyy-MM-dd")}");
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],OrderDetail Send Successfull,Total:{_result.ResultData.Count},Success:{_result.ResultData.Where(p => p.Result).Count()},Fail:{_result.ResultData.Where(p => !p.Result).Count()},File Name:{_result.FileName}.");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing MallSapCode!");
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    SendOrderDetail();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region Poslog
        private static void SendPoslog()
        {
            try
            {
                Console.WriteLine("**********Send Poslog to SAP**********");
                Console.WriteLine("Wait for the Mall List...");
                //显示店铺sapcode列表
                var _m_list = MallSapCodeList();
                Console.WriteLine("Input a Mall Serial Number(If need all store. Please enter \"0\"):");
                string _MallNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _MallSapCode = _m_list[_MallNum];
                Console.WriteLine("Input a start time(yyyy-MM-dd HH:mm:ss):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd HH:mm:ss):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Mall:{_MallSapCode},Time Quantum:{_BeginTime.ToString("yyyy/MM/dd HH:mm:ss")}-{_EndTime.ToString("yyyy/MM/dd HH:mm:ss")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    List<IECommerceAPI> Malls = new List<IECommerceAPI>();
                    //如果是全选
                    if (_MallSapCode.ToUpper() == "ALL")
                    {
                        Malls.AddRange(ECommerceUtil.GetWholeAPIs());
                    }
                    else
                    {
                        var _singleMall = ECommerceUtil.GetWholeAPIs().Where(p => p.StoreSapCode() == _MallSapCode).SingleOrDefault();
                        if (_singleMall != null)
                        {
                            Malls.Add(_singleMall);
                        }
                        else
                        {
                            throw new Exception("Missing MallSapCode!");
                        }
                    }
                    //循环操作
                    foreach (var api in Malls)
                    {
                        var _result = PoslogService.UploadPosLogs(_BeginTime, _EndTime, api.StoreSapCode());
                        if (_result != null)
                        {
                            Console.WriteLine($"Time:{_BeginTime.ToString("yyyy-MM-dd")}To{_EndTime.ToString("yyyy-MM-dd")}");
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}]");
                            Console.WriteLine($"KE,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KE).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KE && p.Status == (int)SapState.Error).Count()}.");
                            Console.WriteLine($"KR,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.KR).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.KR && p.Status == (int)SapState.Error).Count()}.");
                            //Console.WriteLine($"ZKA,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKA && p.Status == (int)SapState.Error).Count()}.");
                            //Console.WriteLine($"ZKB,Total Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB).Count()},Success Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.ToSap).Count()},Fail Record:{_result.Where(p => p.LogType == (int)SapLogType.ZKB && p.Status == (int)SapState.Error).Count()}.");
                        }
                        else
                        {
                            Console.WriteLine($"Mall:{api.StoreName()}[{api.StoreSapCode()}],No Service Power!");
                        }
                    }

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    GetIncrementOrder();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 数据统计
        /// <summary>
        /// 每日订单销售统计
        /// </summary>
        public static void OrderDailyData()
        {
            AnalysisService objAnalysisService = new AnalysisService();
            try
            {
                Console.WriteLine("**********Daily order sales statistics**********");
                Console.WriteLine("Input a start time(yyyy-MM-dd):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Time Quantum:{_BeginTime.ToString("yyyy/MM/dd")}-{_EndTime.ToString("yyyy/MM/dd")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    for (var t = _BeginTime; t <= _EndTime; t = t.AddDays(1))
                    {
                        int _result = objAnalysisService.OrderDailyStatistics(t);
                        Console.WriteLine($"{t.ToString("yyyy-MM-dd")},Total Record:{_result}.");
                    }
                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    OrderDailyData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 每日产品销售统计
        /// </summary>
        public static void ProductDailyData()
        {
            AnalysisService objAnalysisService = new AnalysisService();
            try
            {
                Console.WriteLine("**********Daily product sales statistics**********");
                Console.WriteLine("Input a start time(yyyy-MM-dd):");
                DateTime _BeginTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine("Input a end time(yyyy-MM-dd):");
                DateTime _EndTime = VariableHelper.SaferequestTime(Console.ReadLine());
                Console.WriteLine($"Time Quantum:{_BeginTime.ToString("yyyy/MM/dd")}-{_EndTime.ToString("yyyy/MM/dd")},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    for (var t = _BeginTime; t <= _EndTime; t = t.AddDays(1))
                    {
                        int _result = objAnalysisService.ProductDailyStatistics(t);
                        Console.WriteLine($"{t.ToString("yyyy-MM-dd")},Total Record:{_result}.");
                    }
                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    OrderDailyData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion 

        #region 表字段加密
        private static void TableEncrypt()
        {
            try
            {
                Console.WriteLine("**********MSSQL Table Encrypt**********");
                Console.WriteLine("Wait for the Table List...");
                //---显示表列表
                int js = 0;
                string[] tables = new string[] { "Customer", "OrderReceive", "OrderModify", "OrderReturn", "OrderBilling" };
                Dictionary<string, string> _dict = new Dictionary<string, string>();
                Console.WriteLine("<Table List：");
                //所有店铺序号选项
                foreach (var _o in tables)
                {
                    js++;
                    //增加序号选项
                    _dict.Add(js.ToString(), _o);
                    //显示店铺列表
                    Console.WriteLine($" {js}-{_o}");
                }
                Console.WriteLine(">");
                //----
                Console.WriteLine("Input a Table Serial Number:");
                string _TableNum = VariableHelper.SaferequestNull(Console.ReadLine());
                string _TableName = _dict[_TableNum];
                Console.WriteLine($"Table:{_TableName},Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    Console.WriteLine("Begin to run,Please wait a moment...");
                    //加密
                    TestEncryption.EncryptionHistoricalData(VariableHelper.SaferequestInt(_TableNum));

                    Console.WriteLine("Run finish,Please press any key to exit...");
                }
                else
                {
                    TableEncrypt();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private static void DefaultEncrptPwd()
        {
            //string _defaultPwd = "asia+123";
            //using (var db = new ebEntities())
            //{
            //    Console.WriteLine("Begin to run...");
            //    List<UserInfo> objUserInfoList = db.UserInfo.ToList();
            //    Console.WriteLine("Total User:" + objUserInfoList.Count);
            //    foreach (var item in objUserInfoList)
            //    {
            //        item.Pwd = UserLoginService.EncryptPassword(_defaultPwd, item.PrivateKey);
            //    }
            //    db.SaveChanges();
            //    Console.WriteLine("Finish!");
            //}
        }
        #endregion

        #region 辅助操作
        private static void InsertMallDetail()
        {
            try
            {
                Console.WriteLine("**********Insert MallDetail**********");
                Console.WriteLine($"Begin to insert MallDetail,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    using (var db = new ebEntities())
                    {
                        Console.WriteLine("Begin to run,Please wait a moment...");
                        List<Mall> objMallList = db.Mall.ToList();
                        List<MallDetail> objMallDetailList = db.MallDetail.ToList();
                        MallDetail objMallDetail = new MallDetail();
                        foreach (var item in objMallList)
                        {
                            objMallDetail = objMallDetailList.Where(p => p.MallId == item.Id).SingleOrDefault();
                            if (objMallDetail == null)
                            {
                                db.MallDetail.Add(new MallDetail()
                                {
                                    MallId = item.Id,
                                    MallName = string.Empty,
                                    City = string.Empty,
                                    District = string.Empty,
                                    Address = string.Empty,
                                    ZipCode = string.Empty,
                                    ContactReceiver = string.Empty,
                                    ContactPhone = string.Empty,
                                    Latitude = string.Empty,
                                    Longitude = string.Empty,
                                    StoreType = string.Empty,
                                    Remark = string.Empty,
                                    RelatedBrandStore = String.Empty
                                });
                                db.SaveChanges();
                                Console.WriteLine($"ID:{item.Id},Mall Sap Code:{item.SapCode}");
                            }
                        }
                        Console.WriteLine("Run finish,Please press any key to exit...");
                    }
                }
                else
                {
                    InsertMallDetail();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private static void GetMissedDocument()
        {
            try
            {
                Console.WriteLine("**********Get Missed Document**********");
                Console.WriteLine("Input a Order No(Shopee).:");
                string _orderNo = VariableHelper.SaferequestNull(Console.ReadLine());
                //string _orderNo = "210618MHHR773Q";
                Console.WriteLine($"Begin to get missed document,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    using (var db = new ebEntities())
                    {
                        Console.WriteLine("Begin to run,Please wait a moment...");
                        var objOrder = db.Order.Where(p => p.OrderNo == _orderNo).SingleOrDefault();
                        if (objOrder != null)
                        {
                            if (objOrder.PlatformType == (int)PlatformType.SHOPEE_Singapore)
                            {
                                View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objOrder.MallSapCode && p.PlatformCode == (int)PlatformType.SHOPEE_Singapore).SingleOrDefault();
                                ShopeeAPI objShopeeAPI = new ShopeeAPI()
                                {
                                    MallName = objView_Mall_Platform.MallName,
                                    MallSapCode = objView_Mall_Platform.SapCode,
                                    UserID = objView_Mall_Platform.UserID,
                                    Token = objView_Mall_Platform.Token,
                                    FtpID = objView_Mall_Platform.FtpID,
                                    PlatformCode = objView_Mall_Platform.PlatformCode,
                                    Url = objView_Mall_Platform.Url,
                                    AppKey = objView_Mall_Platform.AppKey,
                                    AppSecret = objView_Mall_Platform.AppSecret,
                                };
                                //子订单集合
                                var objOrderDetailList = db.View_OrderDetail.Where(p => p.OrderNo == _orderNo).ToList();
                                //获取文档
                                foreach (var item in objOrderDetailList)
                                {
                                    objShopeeAPI.GetShippingDocument(item);
                                }
                            }
                            else
                            {
                                throw new Exception("Only support shopee!");
                            }
                        }
                        else
                        {
                            throw new Exception("The order dose not exists!");
                        }

                        Console.WriteLine("Run finish,Please press any key to exit...");
                    }
                }
                else
                {
                    GetMissedDocument();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private static void ConvertDucumentPDFToImage()
        {
            try
            {
                Console.WriteLine("**********Convert Ducument PDF To Image**********");
                Console.WriteLine("Input a Order No(Shopee).:");
                string _orderNo = VariableHelper.SaferequestNull(Console.ReadLine());
                Console.WriteLine($"Begin to convert pdf to image,Y/N?");
                string _s = Console.ReadLine();
                if (_s.ToUpper() == "Y")
                {
                    string filePath = $"{ShopeeConfig.shippingPhysicalFilePath}{DateTime.Now.ToString("yyyy-MM")}/";
                    string saveFilePath = $"{ShopeeConfig.shippingPhysicalFilePath}TMP/";
                    string urlPath = $"{AppGlobalService.HTTP_URL}/{ShopeeConfig.shippingFilePath}{DateTime.Now:yyyy-MM}/";
                    string _fileName = $"{_orderNo}.pdf";
                    string _orderFilePath = $"{filePath}{_fileName}";
                    Console.WriteLine(filePath);
                    Console.WriteLine(urlPath);
                    Console.WriteLine(_fileName);
                    Console.WriteLine(_orderFilePath);
                    PDFHelper.ConvertPDFToImage(_orderFilePath, saveFilePath, _orderNo, ImageFormat.Jpeg, 2);
                    Console.WriteLine("Convert successful!");
                }
                else
                {
                    ConvertDucumentPDFToImage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }
        #endregion

        #region 函数
        /// <summary>
        /// 店铺列表
        /// </summary>
        /// <param name="IsAll"></param>
        /// <returns></returns>
        private static Dictionary<string, string> MallSapCodeList(bool IsAll = true)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            int js = 0;
            using (var db = new ebEntities())
            {
                List<Mall> objMall_List = db.Mall.OrderBy(p => p.SortID).ToList();
                Console.WriteLine("<Mall List：");
                //所有店铺序号选项
                if (IsAll)
                {
                    _result.Add("0", "ALL");
                    Console.WriteLine($" 0-ALL Malls");
                }
                foreach (var _o in objMall_List)
                {
                    js++;
                    //增加序号选项
                    _result.Add(js.ToString(), _o.SapCode);
                    //显示店铺列表
                    Console.WriteLine($" {js}-{_o.Name}[{_o.SapCode}]");
                }
                Console.WriteLine(">");
            }
            return _result;
        }

        /// <summary>
        /// 按平台显示店铺列表
        /// </summary>
        /// <param name="objPlatformCode"></param>
        /// <returns></returns>
        private static Dictionary<string, string> MallSapCodeList(List<int> objPlatformCode)
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            int js = 0;
            using (var db = new ebEntities())
            {
                List<Mall> objMall_List = db.Mall.Where(p => objPlatformCode.Contains(p.PlatformCode)).OrderBy(p => p.SortID).ToList();
                Console.WriteLine("<Mall List：");
                foreach (var _o in objMall_List)
                {
                    js++;
                    //增加序号选项
                    _result.Add(js.ToString(), _o.SapCode);
                    //显示店铺列表
                    Console.WriteLine($" {js}-{_o.Name}[{_o.SapCode}]");
                }
                Console.WriteLine(">");
            }
            return _result;
        }
        #endregion
    }
}
