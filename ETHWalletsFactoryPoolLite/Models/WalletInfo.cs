using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETHWalletsFactoryPoolLite.Models
{
    public class WalletInfo : No2verse.AzureTable.Base.DTableEntity
    {

        public Int32 Index { get; set; }


        public DateTime Stamp { get; set; }


    }
}
