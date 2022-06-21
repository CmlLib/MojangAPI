using CmlLib.Core.Auth;
using MojangAPI;
using MojangAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MojangAPISample
{
    internal class TestMojangAuth
    {
        public static async Task<MSession> Test(MojangAuth auth)
        {
            MojangAuthResponse res;

            Console.WriteLine("TryAutoLogin");
            res = await auth.TryAutoLogin();
            printAuthResponse(res);
            Console.WriteLine();

            if (res.IsSuccess)
                return new MSession
                {
                    AccessToken = res.Session.AccessToken,
                    UUID = res.Session.UUID,
                    Username = res.Session.Username
                };

            Console.WriteLine("Authenticate");
            Console.Write("Email : ");
            var email = Console.ReadLine();
            Console.Write("Password : ");
            var pw = Console.ReadLine();

            res = await auth.Authenticate(email, pw); // fill your mojang email and password
            printAuthResponse(res);
            Console.WriteLine();

            if (!res.IsSuccess)
                throw new Exception("failed to login");

            return new MSession
            {
                AccessToken = res.Session.AccessToken,
                UUID = res.Session.UUID,
                Username = res.Session.Username
            };
        }

        static void printAuthResponse(MojangAuthResponse res)
        {
            Console.WriteLine($"IsSuccess: {res.IsSuccess}, StatusCode: {res.StatusCode}");
            if (!res.IsSuccess)
                Console.WriteLine($"Error: {res.Error}, ErrorMessage: {res.ErrorMessage}");
            else
                Console.WriteLine($"Username: {res.Session.Username}, UUID: {res.Session.UUID}, " +
                    $"AccessToken: {res.Session.AccessToken.Substring(0, 7)}...");
        }
    }
}
