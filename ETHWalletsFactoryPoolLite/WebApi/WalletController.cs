using ETHWalletsFactoryPoolLite.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETHWalletsFactoryPoolLite.WebApi
{
    [Route("api/Wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {

        [HttpGet]
        public async Task<string> Get()
        {

            return "HELLO, I am ETH Wallet Pool Lite , Current Pointer:" + Startup.PeekCurrentPointer();

        }



        [HttpPost]
        [Route("GetWallet")]
        [ProducesResponseType(typeof(ResponseBodyBasic<string>), 200)]
        public async Task<IActionResult> GetWallet([FromBody] RequestGetWallet src)
        {
            if (string.IsNullOrEmpty(src.UserId))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "UserId  null";
                return Ok(eRes);
            }

            //Open comment for Sign
            //if u need.

            //if (src.Sign != Util.GetMD5(src.UserId, Startup.APIToken))
            //{
            //    var eRes = new ResponseBodyBasic<string>();
            //    eRes.Code = "403";
            //    eRes.Status = "ERROR";
            //    eRes.Message = "Not Auth";
            //    return Ok(eRes);
            //}

            var w = "";
            try
            {
                await Task.Run(() =>
                {
                    var ts = Startup.GetOneValue();

                    while (string.IsNullOrEmpty(w))
                    {
                        w = new WalletCreator(Startup.Words, Startup.Password).GetAddressByIndex(ts);
                     
                    }

                    var tableUserWallet = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, "UserWallet", false);
                    var tableWalletUser = new No2verse.AzureTable.Collections.Operator(Startup._AzRole, "WalletUser", false);

                    var wInfo = new Models.WalletInfo
                    {
                        PartitionKey = src.UserId,
                        RowKey = w,
                        Index = ts,
                        Stamp=DateTime.Now
                    };

                    tableUserWallet.Update(wInfo);


                    wInfo.PartitionKey = w;
                    wInfo.RowKey = src.UserId;

                    tableWalletUser.Update(wInfo);
                });
            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<string>();
            res.Content = w;
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = Util.GetMD5(res.Content);

            return Ok(res);

        }



        [HttpPost]
        [Route("GetUserInfo")]
        [ProducesResponseType(typeof(ResponseBodyBasic<WalletInfo[]>), 200)]
        public async Task<IActionResult> GetUserInfo([FromBody] RequestGetUserInfo src)
        {
            if (string.IsNullOrEmpty(src.UserId))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "UserId  null";
                return Ok(eRes);
            }

            //Open comment for Sign
            //if u need.

            //if (src.Sign != Util.GetMD5(src.UserId, Startup.APIToken))
            //{
            //    var eRes = new ResponseBodyBasic<string>();
            //    eRes.Code = "403";
            //    eRes.Status = "ERROR";
            //    eRes.Message = "Not Auth";
            //    return Ok(eRes);
            //}

            WalletInfo[] resWallets = null;
            try
            {
                await Task.Run(() =>
                {


                    var q = new No2verse.AzureTable.Collections.Query<WalletInfo>(Startup._AzRole, "UserWallet");
                    var tmpRes = q.DatasByPartitionKey(src.UserId).ToList();
                    if (tmpRes.Count > 0)
                    {
                        resWallets = tmpRes.ToArray();
                    }

                });
            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<WalletInfo[]>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<WalletInfo[]>();
            res.Content = resWallets;
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = Util.GetMD5(JsonConvert.SerializeObject(resWallets));

            return Ok(res);

        }

        [HttpPost]
        [Route("GetWalletInfo")]
        [ProducesResponseType(typeof(ResponseBodyBasic<WalletInfo[]>), 200)]
        public async Task<IActionResult> GetWalletInfo([FromBody] RequestGetWalletInfo src)
        {
            if (string.IsNullOrEmpty(src.Wallet))
            {
                var eRes = new ResponseBodyBasic<string>();
                eRes.Code = "400";
                eRes.Status = "ERROR";
                eRes.Message = "UserId  null";
                return Ok(eRes);
            }

            //Open comment for Sign
            //if u need.

            //if (src.Sign != Util.GetMD5(src.Wallet, Startup.APIToken))
            //{
            //    var eRes = new ResponseBodyBasic<string>();
            //    eRes.Code = "403";
            //    eRes.Status = "ERROR";
            //    eRes.Message = "Not Auth";
            //    return Ok(eRes);
            //}

            WalletInfo[] resWallets = null;
            try
            {
                await Task.Run(() =>
                {


                    var q = new No2verse.AzureTable.Collections.Query<WalletInfo>(Startup._AzRole, "WalletUser");
                    var tmpRes = q.DatasByPartitionKey(src.Wallet).ToList();
                    if (tmpRes.Count > 0)
                    {
                        resWallets = tmpRes.ToArray();
                    }

                });
            }
            catch (Exception ex)
            {
                var eRes = new ResponseBodyBasic<WalletInfo[]>();
                eRes.Code = "500";
                eRes.Status = "ERROR";
                eRes.Message = ex.Message;
                return Ok(eRes);
            }
            var res = new ResponseBodyBasic<WalletInfo[]>();
            res.Content = resWallets;
            res.Code = "200";
            res.Status = "SUCCESS";
            res.Sign = Util.GetMD5(JsonConvert.SerializeObject(resWallets));

            return Ok(res);

        }

    }
}
