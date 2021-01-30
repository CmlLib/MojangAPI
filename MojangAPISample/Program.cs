using System;
using System.Threading.Tasks;
using System.Net.Http;
using MojangAPI;

namespace MojangAPISample
{
    class Program
    {
        static HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Mojang mojang = new Mojang(httpClient);
            
        }
    }
}
