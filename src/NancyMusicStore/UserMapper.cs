using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using NancyMusicStore.Common;
using NancyMusicStore.Models;
using System;
using System.Data;

namespace NancyMusicStore
{
    internal class UserMapper : IUserMapper
    {
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            string cmd = "public.get_user_by_userid";
            var user = DBHelper.QueryFirstOrDefault<SysUser>(cmd, new
            {
                uid = identifier.ToString()
            }, null, null, CommandType.StoredProcedure);

            return user == null
                ? null
                : new UserIdentity
                {
                    UserName = user.SysUserName,
                    Claims = new[] { "SystemUser" }
                };           
        }
    }
}