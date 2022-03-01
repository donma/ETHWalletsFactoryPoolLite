using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace ETHWalletsFactoryPoolLite
{
    public class Startup
    {


        private static ConcurrentQueue<Int32> _IndexPointerSwap;

        private static int _IndexPointer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        /// <summary>
        /// 檢查目前走到的 Pointer 然後做儲存
        /// </summary>
        private static Timer _PointerChecker { get; set; }


        /// <summary>
        /// Ｑ 暫存數量
        /// </summary>
        private readonly static int _SwapCount = 50;


        /// <summary>
        /// 註記詞 ，自行更改
        /// </summary>
        public readonly static string Words = "當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻 當 麻";


        /// <summary>
        /// 密碼 自行更改
        /// </summary>
        public readonly static string Password = "password";


        public readonly static string APIToken = "token";

        /// <summary>
        /// Auzre Blob Table Connection String 
        /// </summary>
        public readonly static string _BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=....";

        public static No2verse.AzureTable.Base.AzureTableRole _AzRole = new No2verse.AzureTable.Base.AzureTableRole("ETHWALLETPOOL", new No2verse.AzureTable.AzureStorageSettings
        {
            ConnectionString = _BlobConnectionString

        });



        /// <summary>
        /// 初始化起始數值
        /// </summary>
        private void InitPointerFromFile()
        {

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "COUNT"))
            {

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT", "1000");
                _IndexPointer = 1000;
            }
            else
            {

                var tmp = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT");
                _IndexPointer = int.Parse(tmp);
            }
        }


        /// <summary>
        /// 是否正在執行檢查
        /// 如果正在執行跳開
        /// </summary>
        private static bool IsRunningChecker { get; set; }



        /// <summary>
        /// Peek Now Pointer.
        /// </summary>
        /// <returns></returns>
        public static int PeekCurrentPointer() {

            int tmp = 0;
            while (_IndexPointerSwap.TryPeek(out tmp))
            {
                break;
            }

            return tmp;

        }

        /// <summary>
        /// 重新啟動檢查的 Timer
        /// </summary>
        public static void RestartTimerChecker()
        {

            _PointerChecker = new Timer();
            _PointerChecker.Elapsed += (sender, args) =>
            {

                if (IsRunningChecker) return;

                //simple lock for recyle.
                IsRunningChecker = true;

                int tmp = -1;
                //檢查目前走到哪裡並且抄寫回 file.
                if (_IndexPointerSwap.TryPeek(out tmp))
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT", tmp.ToString());
                }


                IsRunningChecker = false;

            };

            _PointerChecker.Interval = 500;

            _PointerChecker.Start();


        }

        /// <summary>
        /// 初始化填入多少的buffer pointer.
        /// </summary>
        /// <param name="num"></param>
        private void FillPointer(int num)
        {
            //為了避免中間可能被之前用過
            //所以必須要用 中間的 buffer 數往後加入
            var nP = _IndexPointer + 1 + _SwapCount;
            for (var i = (nP); i < (nP + num); i++)
            {
                _IndexPointerSwap.Enqueue(i);
            }

        }


        /// <summary>
        /// 取得一個數值
        /// </summary>
        /// <returns></returns>
        public static int GetOneValue()
        {

            int res = -1;
            while (!_IndexPointerSwap.TryDequeue(out res))
            {

            }

            _IndexPointerSwap.Enqueue(res + _SwapCount);
            return res;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddRazorPages();


            services.AddSwaggerDocument(settings =>
            {
                settings.Title = "ETHWalletFactoryPoolLite Dev API";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //SetPointer From File
            InitPointerFromFile();

            //Fille data to quqeue.
            _IndexPointerSwap = new ConcurrentQueue<int>();


            //Fill 100 to _IndexPointerSwap
            FillPointer(_SwapCount);

            //Start Timer
            RestartTimerChecker();

            //Init Azure Blob Table
            var tableUserWallet = new No2verse.AzureTable.Collections.Operator(_AzRole,"UserWallet", true);
            var tableWalletUser = new No2verse.AzureTable.Collections.Operator(_AzRole, "WalletUser", true);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
            
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }



    }
}
