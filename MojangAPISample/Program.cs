﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using MojangAPI;
using MojangAPI.SecurityQuestion;
using CmlLib.Core.Auth;
using XboxAuthNet.Game.Msal;
using CmlLib.Core.Auth.Microsoft;

namespace MojangAPISample
{
    class Program
    {
        static int success = 0;
        static int fail = 0;

        static async Task Main(string[] args)
        {
            var clientHandler = new HttpClientHandler();
            //clientHandler.Proxy = new WebProxy("127.0.0.111:8080");
            //var loggingHandler = new LoggingHandler(clientHandler);
            
            var httpClient = new HttpClient(clientHandler);

            Mojang mojang = new Mojang(httpClient);
            MojangAuth auth = new MojangAuth(httpClient);
            QuestionFlow qflow = new QuestionFlow(httpClient);

            MSession session = new MSession("testaccesstoken", "testuuid", "testuuid");
            bool useXboxAccount = true;
            if (useXboxAccount)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("LoginMode: Xbox");
                Console.ForegroundColor = ConsoleColor.White;
                session = await loginXbox();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("LoginMode: Mojang");
                Console.ForegroundColor = ConsoleColor.White;
                session = await TestMojangAuth.Test(auth);
            }

            await testMojangApi(mojang, session, false);

            Console.WriteLine();
            Console.WriteLine("Done! Test result: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS: " + success);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAIL: " + fail);
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
        }

        static async Task testMojangApi(Mojang mojang, MSession session, bool includeDangerousApis)
        {
            var tester = new TestMojangApi(mojang, session);

            await testMethod("GetUUID", () => tester.TestGetUUID());
            await testMethod("GetNameHistories", () => tester.TestGetNameHistories());
            await testMethod("GetProfileUsingUUID", () => tester.TestGetProfileUsingUUID());
            await testMethod("GetProfileUsingAccessToken", () => tester.TestGetProfileUsingAccessToken());
            await testMethod("GetPlayerAttributes", () => tester.TestGetPlayerAttributes());
            await testMethod("GetBlocklist", () => tester.TestGetBlocklist());
            await testMethod("GetPlayerCertificates", () => tester.TestGetPlayerCertificates());
            await testMethod("GetBlockServers", () => tester.TestGetBlockServers());
            await testMethod("CheckGameOwnership", () => tester.TestCheckGameOwnership());
            await testMethod("GetStatistics", () => tester.TestGetStatistics());
            await testMethod("CheckNameAvailability", () => tester.TestCheckNameAvailability());

            if (includeDangerousApis)
            {
                await testMethod("ChangeName", () => tester.TestChangeName());
                await testMethod("ChangeSkin", () => tester.TestChangeSkin());
                await testMethod("UploadSkin", () => tester.TestUploadSkin());
                await testMethod("ResetSkin", () => tester.TestResetSkin());
            }
        }

        static async Task<MSession> loginXbox()
        {
            var app = MsalClientHelper.CreateDefaultApplicationBuilder("499c8d36-be2a-4231-9ebd-ef291b7bb64c").Build();
            var handler = JELoginHandlerBuilder.BuildDefault();

            var authenticator = handler.CreateAuthenticatorWithDefaultAccount();
            authenticator.AddMsalOAuth(app, msal => msal.InteractiveWithSingleAccount());
            authenticator.AddXboxAuthForJE(xbox => xbox.Basic());
            authenticator.AddJEAuthenticator();

            var session = await authenticator.ExecuteForLauncherAsync();
            return session;
        }

        static async Task testMethod(string name, Func<Task<bool>> task)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{name}] Start test");
            Console.ForegroundColor = ConsoleColor.White;

            bool result = false;
            try
            {
                result = await task.Invoke();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{name}] Exception!");
                Console.WriteLine(ex);
            }

            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{name}] SUCCESS");
                success++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{name}] FAIL");
                fail++;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
