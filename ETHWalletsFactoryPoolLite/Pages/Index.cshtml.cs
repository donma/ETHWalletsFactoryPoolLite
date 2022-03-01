using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Nethereum.HdWallet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ETHWalletsFactoryPoolLite.Pages
{


    public class IndexModel : PageModel
    {
  

        
        public void OnGet()
        {

            Response.Redirect("/api/Wallet");
        }


    }
}
