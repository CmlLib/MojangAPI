using System;
using System.Threading.Tasks;
using System.Net.Http;
using MojangAPI;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;

namespace MojangAPISample
{
    class Program
    {
        static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Mojang mojang = new Mojang(httpClient);
            MojangAuth auth = new MojangAuth(httpClient);
            QuestionFlow qflow = new QuestionFlow(httpClient);
            Session session = new Session()
            {
                AccessToken = "at123",
                UUID = "test_uuid123",
                Username = "test_user123"
            };
            
            session = await testMojangAuth(auth);
            await testMojangAPIs(mojang, session);
            await testSecurityFlow(qflow, session);
            //await test_DANGEROUS_apis(mojang, session);

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static async Task testMojangAPIs(Mojang mojang, Session session)
        {
            Console.WriteLine("GetUUID");
            PlayerUUID uuid = await mojang.GetUUID(session.Username);
            printResponse(uuid);
            Console.WriteLine($"UUID: {uuid.UUID}, IsLegacy: {uuid.IsLegacy}, IsDemo: {uuid.IsDemo}\n");

            Console.WriteLine("GetNameHistories");
            NameHistory[] historyRes = await mojang.GetNameHistories(session.UUID);
            if (historyRes != null)
            {
                foreach (var item in historyRes)
                {
                    Console.WriteLine($"[{item.ChangedTime}] {item.Name}");
                }
            }
            else
                Console.WriteLine("failed");
            Console.WriteLine();

            Console.WriteLine("GetProfileUsingUUID");
            PlayerProfile uuidProfile = await mojang.GetProfileUsingUUID(session.UUID);
            printResponse(uuidProfile);
            printProfile(uuidProfile);
            Console.WriteLine();

            Console.WriteLine("GetProfileUsingAccessToken");
            PlayerProfile atProfile = await mojang.GetProfileUsingAccessToken(session.AccessToken);
            printResponse(atProfile);
            printProfile(atProfile);
            Console.WriteLine();

            Console.WriteLine("GetBlockedServers");
            string[] servers = await mojang.GetBlockedServer();
            Console.WriteLine($"There are {servers.Length} blocked servers");
            Console.WriteLine();

            Console.WriteLine("CheckGameOwnership");
            bool checkGameOwnership = await mojang.CheckGameOwnership(session.AccessToken);
            Console.WriteLine(checkGameOwnership);
            Console.WriteLine();

            Console.WriteLine("GetStatistics");
            Statistics stat = await mojang.GetStatistics(StatisticOption.ItemSoldMinecraft, StatisticOption.ItemSoldDungeons);
            printResponse(stat);
            Console.WriteLine($"Total: {stat.Total}, Last24: {stat.Last24h}, Velocity/s: {stat.SaleVelocityPerSeconds}");
            Console.WriteLine();
        }

        static async Task<Session> testMojangAuth(MojangAuth auth)
        {
            MojangAuthResponse res;

            Console.WriteLine("TryAutoLogin");
            res = await auth.TryAutoLogin();
            printAuthResponse(res);
            Console.WriteLine();

            if (res.IsSuccess)
                return res.Session;

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

            return res.Session;
        }

        static async Task<Session> testMicrosoftAuth(MojangAuth auth, string uhs, string xstsToken)
        {
            MojangAuthResponse res;

            Console.WriteLine("LoginWithXbox");
            res = await auth.RequestSessionWithXbox(uhs, xstsToken);
            printAuthResponse(res);

            if (!res.IsSuccess)
                throw new Exception("failed to login");

            return res.Session;
        }

        static async Task testSecurityFlow(QuestionFlow q, Session session)
        {
            MojangAPIResponse trusted = await q.CheckTrusted(session.AccessToken);
            printResponse(trusted);

            if (trusted.IsSuccess)
            {
                Console.WriteLine("Your IP was trusted. You don't have to answer security questions.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("!! You have to answer security questions !!");
            Console.WriteLine();

            QuestionFlowResponse res = await q.GetQuestionList(session.AccessToken);
            printResponse(res);

            if (!res.IsSuccess)
                throw new Exception("failed to get questions");

            QuestionList questions = res.Questions;
            for (int i = 0; i < questions.Count; i++)
            {
                Question question = questions[i];
                Console.WriteLine($"Q{i + 1}. [{question.QuestionId}] {question.QuestionMessage}");
                Console.Write("Answer? : ");

                var answer = Console.ReadLine();
                question.Answer = answer;
                Console.WriteLine();
            }

            MojangAPIResponse answerResponse = await q.SendAnswers(questions, session.AccessToken);
            printResponse(answerResponse);

            if (answerResponse.IsSuccess)
                return;
            else
                throw new Exception();
        }

        static async Task test_DANGEROUS_apis(Mojang mojang, Session session)
        {
            Console.WriteLine("ChangeName");
            PlayerProfile changeNameProfile = await mojang.ChangeName(session.AccessToken, "NEWNAME");
            printResponse(changeNameProfile);
            printProfile(changeNameProfile);

            Console.WriteLine("ChangeSkin");
            MojangAPIResponse changeSkinRes = await mojang.ChangeSkin(session.UUID, session.AccessToken, SkinType.Steve, "https://google.com/favcon.ico");
            printResponse(changeSkinRes);

            Console.WriteLine("UploadSkin");
            MojangAPIResponse uploadSkinRes = await mojang.UploadSkin(session.AccessToken, SkinType.Steve, "skin.png");
            printResponse(uploadSkinRes);

            Console.WriteLine("ResetSkin");
            MojangAPIResponse resetSkinRes = await mojang.ResetSkin(session.UUID, session.AccessToken);
            printResponse(resetSkinRes);
        }

        static void printResponse(MojangAPIResponse res)
        {
            Console.WriteLine($"IsSuccess: {res.IsSuccess}, StatusCode: {res.StatusCode}");
            if (!res.IsSuccess)
            {
                Console.WriteLine($"Error: {res.Error}, ErrorMessage: {res.ErrorMessage}");
            }

        }

        static void printProfile(PlayerProfile profile)
        {
            if (!profile.IsSuccess)
                return;

            Console.WriteLine($"{profile.UUID}: {profile.Name}, IsLegacy: {profile.IsLegacy}");
            Console.WriteLine($"SKIN: {profile.Skin.Url}, {profile.Skin.Model}");
        }

        static void printAuthResponse(MojangAuthResponse res)
        {
            Console.WriteLine($"IsSuccess: {res.IsSuccess}, StatusCode: {res.StatusCode}");
            if (!res.IsSuccess)
                Console.WriteLine($"Error: {res.Error}, ErrorMessage: {res.ErrorMessage}");
            else
                Console.WriteLine($"Username: {res.Session.Username}, UUID: {res.Session.UUID}, " +
                    $"AccessToken: {res.Session.AccessToken.Substring(0,7)}...");
        }
    }
}
