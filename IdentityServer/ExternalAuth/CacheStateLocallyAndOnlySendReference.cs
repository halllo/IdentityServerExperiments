using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace IdentityServer.ExternalAuth
{
    public class CacheStateLocallyAndOnlySendReference : ISecureDataFormat<AuthenticationProperties>
    {
        private const string CacheKeyPrefix = "CachedState-";
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDataProtector dataProtector;
        private readonly IDataSerializer<AuthenticationProperties> serializer;

        public CacheStateLocallyAndOnlySendReference(IHttpContextAccessor httpContextAccessor, IDataProtector dataProtector) : this(httpContextAccessor, dataProtector, new PropertiesSerializer())
        {
        }

        public CacheStateLocallyAndOnlySendReference(IHttpContextAccessor httpContextAccessor, IDataProtector dataProtector, IDataSerializer<AuthenticationProperties> serializer)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.dataProtector = dataProtector;
            this.serializer = serializer;
        }

        public string Protect(AuthenticationProperties data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationProperties data, string purpose)
        {
            var key = Guid.NewGuid().ToString();
            var cacheKey = $"{CacheKeyPrefix}{key}";
            var serialized = this.serializer.Serialize(data);

            // Rather than encrypt the full AuthenticationProperties
            // cache the data and encrypt the key that points to the data
            Cache(cacheKey, Convert.ToBase64String(serialized));

            return this.dataProtector.Protect(key);
        }

        public AuthenticationProperties Unprotect(string protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationProperties Unprotect(string protectedText, string purpose)
        {
            // Decrypt the key and retrieve the data from the cache.
            var key = this.dataProtector.Unprotect(protectedText);
            var cacheKey = $"{CacheKeyPrefix}{key}";

            var cached = Uncache(cacheKey);
            if (string.IsNullOrWhiteSpace(cached)) throw new Exception("cannot finde state in cache");
            var serialized = Convert.FromBase64String(cached);

            return this.serializer.Deserialize(serialized);
        }


        private static Dictionary<string, string> _store;
        private void Cache(string key, string value)
        {
            _store[key] = value;
        }

        private string Uncache(string key)
        {
            if (_store.TryGetValue(key, out string value))
            {
                return value;
            }
            else return null;
        }
    }
}
