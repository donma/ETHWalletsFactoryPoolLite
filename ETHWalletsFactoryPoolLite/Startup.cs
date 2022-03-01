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
        /// �ˬd�ثe���쪺 Pointer �M�ᰵ�x�s
        /// </summary>
        private static Timer _PointerChecker { get; set; }


        /// <summary>
        /// �� �Ȧs�ƶq
        /// </summary>
        private readonly static int _SwapCount = 50;


        /// <summary>
        /// ���O�� �A�ۦ���
        /// </summary>
        public readonly static string Words = "�� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��";


        /// <summary>
        /// �K�X �ۦ���
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
        /// ��l�ư_�l�ƭ�
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
        /// �O�_���b�����ˬd
        /// �p�G���b������}
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
        /// ���s�Ұ��ˬd�� Timer
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
                //�ˬd�ثe������̨åB�ۼg�^ file.
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
        /// ��l�ƶ�J�h�֪�buffer pointer.
        /// </summary>
        /// <param name="num"></param>
        private void FillPointer(int num)
        {
            //���F�קK�����i��Q���e�ιL
            //�ҥH�����n�� ������ buffer �Ʃ���[�J
            var nP = _IndexPointer + 1 + _SwapCount;
            for (var i = (nP); i < (nP + num); i++)
            {
                _IndexPointerSwap.Enqueue(i);
            }

        }


        /// <summary>
        /// ���o�@�Ӽƭ�
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
