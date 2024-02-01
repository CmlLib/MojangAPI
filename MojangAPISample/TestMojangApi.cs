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
            Console.WriteLine($"UUID: {uuid.UUID}, IsLegacy: {uuid.IsLegacy}, IsDemo: {uuid.IsDemo}");
            return true;
        }

        public async Task<bool> TestGetNameHistories()
        {
            note("This service was closed down by Mojang. It may not work.");
            NameHistory[] histories = await mojang.GetNameHistories(session.UUID);
            Console.WriteLine("Count: " + histories);
            foreach (var item in histories)
            {
                Console.WriteLine($"{item.ChangedTime}: {item.Name}");
            }
            return true;
        }

        public async Task<bool> TestGetProfileUsingUUID()
        {
            PlayerProfile uuidProfile = await mojang.GetProfileUsingUUID(session.UUID);
            printProfile(uuidProfile);
            return true;
        }

        public async Task<bool> TestGetProfileUsingAccessToken()
        {
            note("This api only works on xbox account");
            PlayerProfile atProfile = await mojang.GetProfileUsingAccessToken(session.AccessToken);
            printProfile(atProfile);
            return true;
        }

        public async Task<bool> TestGetPlayerAttributes()
        {
            note("This api only works on xbox account");
            PlayerAttributes attributes = await mojang.GetPlayerAttributes(session.AccessToken);
            Console.WriteLine("Privileges.Telemtry: " + attributes.Privileges?.Telemtry?.Enabled);
            Console.WriteLine("Privileges.MultiplayerServer: " + attributes?.Privileges?.MultiplayerServer?.Enabled);
            Console.WriteLine("Privileges.MultiplayerRealms: " + attributes?.Privileges?.MultiplayerRealms?.Enabled);
            Console.WriteLine("Privileges.OnlineChat: " + attributes?.Privileges?.OnlineChat?.Enabled);
            Console.WriteLine("ProfanityFilterPreferences.ProfanityFilterOn: " + attributes?.ProfanityFilterPreferences?.ProfanityFilterOn);

            Console.Write("BanStatus.BannedScopes: ");

            var bannedScopes = attributes?.BanStatus?.BannedScopes;
            if (bannedScopes == null)
                Console.WriteLine("null");
            else
            {
                Console.WriteLine();
                foreach (var item in bannedScopes)
                {
                    Console.WriteLine(item.Key + ": ");
                    if (item.Value == null)
                        continue;
                    Console.WriteLine("BanId: " + item.Value.BanId);
                    Console.WriteLine("Expires: " + item.Value.Expires ?? "permanent");
                    Console.WriteLine("Reason: " + item.Value.Reason);
                    Console.WriteLine("ReasonMessage: " + item.Value.ReasonMessage);
                }
            }

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
            Console.WriteLine("KeyPair.PrivateKey: " + certificates.KeyPair?.PrivateKey);
            Console.WriteLine("KeyPair.PublicKey: " + certificates.KeyPair?.PublicKey);
            Console.WriteLine("PublicKeySignature: " + certificates.PublicKeySignature);
            Console.WriteLine("ExpiresAt: " + certificates.ExpiresAt);
            Console.WriteLine("RefreshedAfter: " + certificates.RefreshedAfter);
            return true;
        }

        public async Task<bool> TestGetBlockServers()
        {
            string[] servers = await mojang.GetBlockedServer();
            for (int i = 0; i < 5 && i < servers.Length; i++)
            {
                Console.WriteLine(servers[i]);
            }
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
            printProfile(changeNameProfile);
            return true;
        }

        public async Task<bool> TestChangeSkin()
        {
            note("This api only works on xbox account");
            await mojang.ChangeSkin(session.UUID, session.AccessToken, SkinType.Alex, SkinTemplates.AlexUrl);
            return true;
        }

        public async Task<bool> TestUploadSkin()
        {
            note("This api only works on xbox account");
            await mojang.UploadSkin(session.AccessToken, SkinType.Steve, "skin.png");
            return true;
        }

        public async Task<bool> TestResetSkin()
        {
            note("This api only works on xbox account");
            await mojang.ResetSkin(session.UUID, session.AccessToken);
            return true;
        }

        public async Task<bool> TestGetStatistics()
        {
            note("This service was closed down by Mojang. It may not work.");
            Statistics stat = await mojang.GetStatistics(StatisticOption.ItemSoldMinecraft, StatisticOption.ItemSoldDungeons);
            Console.WriteLine($"Total: {stat.Total}, Last24: {stat.Last24h}, Velocity/s: {stat.SaleVelocityPerSeconds}");
            return true;
        }

        public async Task<bool> TestCheckNameAvailability()
        {
            string newName = "NEWNAME123";
            string? result = await mojang.CheckNameAvailability(session.AccessToken, newName);
            Console.WriteLine($"{newName}: {result}");
            return true;
        }

        private void printProfile(PlayerProfile profile)
        {
            Console.WriteLine($"{profile.UUID}: {profile.Name}, IsLegacy: {profile.IsLegacy}");
            Console.WriteLine($"SKIN: {profile.Skin?.Url}, {profile.Skin?.Model}");
        }

        private void note(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"NOTE: {msg}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
