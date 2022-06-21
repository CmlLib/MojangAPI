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
    internal class TestMojangApi
    {
        private readonly Mojang mojang;
        private readonly MSession session;

        public TestMojangApi(Mojang _mojang, MSession _session)
        {
            this.mojang = _mojang;
            this.session = _session;
        }

        public async Task<bool> TestGetUUID()
        {
            PlayerUUID uuid = await mojang.GetUUID(session.Username);
            printResponse(uuid);
            Console.WriteLine($"UUID: {uuid.UUID}, IsLegacy: {uuid.IsLegacy}, IsDemo: {uuid.IsDemo}");
            return uuid.IsSuccess;
        }

        public async Task<bool> TestGetNameHistories()
        {
            NameHistoryResponse historyRes = await mojang.GetNameHistories(session.UUID);
            printResponse(historyRes);
            if (historyRes.IsSuccess)
            {
                Console.WriteLine("Count: " + historyRes.Histories.Length);
                foreach (var item in historyRes.Histories)
                {
                    Console.WriteLine($"{item.ChangedTime}: {item.Name}");
                }
            }
            return historyRes.IsSuccess;
        }

        public async Task<bool> TestGetProfileUsingUUID()
        {
            PlayerProfile uuidProfile = await mojang.GetProfileUsingUUID(session.UUID);
            printResponse(uuidProfile);
            printProfile(uuidProfile);
            return uuidProfile.IsSuccess;
        }

        public async Task<bool> TestGetProfileUsingAccessToken()
        {
            note("This api only works on xbox account");
            PlayerProfile atProfile = await mojang.GetProfileUsingAccessToken(session.AccessToken);
            printResponse(atProfile);
            printProfile(atProfile);
            return atProfile.IsSuccess;
        }

        public async Task<bool> TestGetPlayerAttributes()
        {
            note("This api only works on xbox account");
            PlayerAttributes attributes = await mojang.GetPlayerAttributes(session.AccessToken);
            Console.WriteLine("Privileges.Telemtry: " + attributes.Privileges.Telemtry);
            Console.WriteLine("Privileges.MultiplayerServer: " + attributes.Privileges.MultiplayerServer);
            Console.WriteLine("Privileges.MultiplayerRealms: " + attributes.Privileges.MultiplayerRealms);
            Console.WriteLine("Privileges.OnlineChat: " + attributes.Privileges.OnlineChat);
            Console.WriteLine("ProfanityFilterPreferences.ProfanityFilterOn: " + attributes.ProfanityFilterPreferences.ProfanityFilterOn);
            return true;
        }

        public async Task<bool> TestGetBlocklist()
        {
            note("This api only works on xbox account");
            string[] blocks = await mojang.GetPlayerBlocklist(session.AccessToken);
            Console.WriteLine("Count: " + blocks.Length);
            foreach (string block in blocks)
            {
                Console.WriteLine(block);
            }
            return true;
        }

        public async Task<bool> TestGetPlayerCertificates()
        {
            note("This api only works on xbox account");
            PlayerCertificates certificates = await mojang.GetPlayerCertificates(session.AccessToken);
            Console.WriteLine("KeyPair.PrivateKey: " + certificates.KeyPair.PrivateKey);
            Console.WriteLine("KeyPair.PublicKey: " + certificates.KeyPair.PublicKey);
            Console.WriteLine("PublicKeySignature: " + certificates.PublicKeySignature);
            Console.WriteLine("ExpiresAt: " + certificates.ExpiresAt);
            Console.WriteLine("RefreshedAfter: " + certificates.RefreshedAfter);
            return true;
        }

        public async Task<bool> TestGetBlockServers()
        {
            string[] servers = await mojang.GetBlockedServer();
            Console.WriteLine($"There are {servers.Length} blocked servers");
            return true;
        }

        public async Task<bool> TestCheckGameOwnership()
        {
            note("This api only works on xbox account");
            bool checkGameOwnership = await mojang.CheckGameOwnership(session.AccessToken);
            if (!checkGameOwnership)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(checkGameOwnership);
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }

        public async Task<bool> TestChangeName()
        {
            note("This api only works on xbox account");
            PlayerProfile changeNameProfile = await mojang.ChangeName(session.AccessToken, "NEWNAME");
            printResponse(changeNameProfile);
            printProfile(changeNameProfile);
            return changeNameProfile.IsSuccess;
        }

        public async Task<bool> TestChangeSkin()
        {
            note("This api only works on xbox account");
            MojangAPIResponse changeSkinRes = await mojang.ChangeSkin(session.UUID, session.AccessToken, SkinType.Alex, SkinTemplates.AlexUrl);
            printResponse(changeSkinRes);
            return changeSkinRes.IsSuccess;
        }

        public async Task<bool> TestUploadSkin()
        {
            note("This api only works on xbox account");
            MojangAPIResponse uploadSkinRes = await mojang.UploadSkin(session.AccessToken, SkinType.Steve, "skin.png");
            printResponse(uploadSkinRes);
            return uploadSkinRes.IsSuccess;
        }

        public async Task<bool> TestResetSkin()
        {
            note("This api only works on xbox account");
            MojangAPIResponse resetSkinRes = await mojang.ResetSkin(session.UUID, session.AccessToken);
            printResponse(resetSkinRes);
            return resetSkinRes.IsSuccess;
        }

        public async Task<bool> TestGetStatistics()
        {
            note("This service was closed down by Mojang. It may not work.");
            Statistics stat = await mojang.GetStatistics(StatisticOption.ItemSoldMinecraft, StatisticOption.ItemSoldDungeons);
            printResponse(stat);
            Console.WriteLine($"Total: {stat.Total}, Last24: {stat.Last24h}, Velocity/s: {stat.SaleVelocityPerSeconds}");
            return stat.IsSuccess;
        }

        private bool printResponse(MojangAPIResponse res)
        {
            Console.WriteLine($"IsSuccess: {res.IsSuccess}, StatusCode: {res.StatusCode}");
            if (!res.IsSuccess)
            {
                Console.WriteLine($"Error: {res.Error}, ErrorMessage: {res.ErrorMessage}");
            }
            return res.IsSuccess;
        }

        private void printProfile(PlayerProfile profile)
        {
            if (!profile.IsSuccess)
                return;

            Console.WriteLine($"{profile.UUID}: {profile.Name}, IsLegacy: {profile.IsLegacy}");
            Console.WriteLine($"SKIN: {profile.Skin.Url}, {profile.Skin.Model}");
        }

        private void note(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"NOTE: {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
