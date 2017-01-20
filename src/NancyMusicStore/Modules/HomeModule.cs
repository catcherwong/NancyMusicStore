using Nancy;
using NancyMusicStore.Common;
using NancyMusicStore.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NancyMusicStore.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule() : base("/")
        {
            Get["/"] = _ =>
            {
                var albums = GetTopSellingAlbums(5);
                return View["Index", albums];
            };
        }

        /// <summary>
        /// get top count selling albums 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<Album> GetTopSellingAlbums(int count)
        {
            string sql = "public.get_top_selling_albums";
            var list = DBHelper.Query<Album>(sql, new
            {
                num = count
            }, null, true, null, CommandType.StoredProcedure).ToList();
            return list;
        }
    }
}