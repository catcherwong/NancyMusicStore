using Nancy.Security;
using System.Collections.Generic;

namespace NancyMusicStore
{
    internal class UserIdentity : IUserIdentity
    {
        public IEnumerable<string> Claims { get; set; }
        public string UserName { get; set; }       
    }
}