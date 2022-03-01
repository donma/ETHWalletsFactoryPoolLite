using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETHWalletsFactoryPoolLite
{
    public class WalletCreator
    {
        private Wallet _Wallet { get; set; }


        public WalletCreator(string words, string password)
        {

            _Wallet = new Wallet(words, password);
        }

        public Nethereum.Web3.Accounts.Account GetHDWalletInfoByIndex(int index)
        {


            Account r = null;

            r = _Wallet.GetAccount(index);

            if (r == null)
            {
                return null;
            }
            else
            {
                return r;
            }


        }

        public string GetAddressByIndex(int index)
        {
         
            Account r = null;
            var skip = 0;
            while (r == null && skip <= 100)
            {
                r = _Wallet.GetAccount(index);

                skip++;
            }

            if (r == null)
            {
                return null;
            }
            else
            {
                return r.Address;
            }

        }
    }

    public class Util
    {

        public static string GetMD5(string input, string salt = "Donma")
        {
            var x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input + "|" + salt);
            bs = x.ComputeHash(bs);
            var s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }


    }
}
