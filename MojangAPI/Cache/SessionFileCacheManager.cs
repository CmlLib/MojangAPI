using MojangAPI.Model;
using System;

namespace MojangAPI.Cache
{
    public class SessionFileCacheManager : JsonFileCacheManager<Session>
    {
        public SessionFileCacheManager(string filepath) : base (filepath)
        {

        }

        public override Session GetDefaultObject()
        {
            return new Session
            {
                ClientToken = Guid.NewGuid().ToString()
            };
        }
    }
}
