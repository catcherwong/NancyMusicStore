using Nancy;
using NancyMusicStore.Common;
using NancyMusicStore.Models;
using NancyMusicStore.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NancyMusicStore.Modules
{
    public class StoreModule : NancyModule
    {
        public StoreModule() : base("/store")
        {
            Get["/"] = _ =>
            {
                return View["Index", GetGenreList()];
            };

            Get["/genremenu"] = _ =>
            {
                return Response.AsJson(GetGenreList());
            };

            Get["details/{id:int}"] = _ =>
            {
                int id = 0;
                if (int.TryParse(_.id, out id))
                {
                    string cmd = "public.get_album_details_by_aid";
                    var album = DBHelper.QueryFirstOrDefault<AlbumDetailsViewModel>(cmd, new
                    {
                        aid = id
                    }, null, null, CommandType.StoredProcedure);
                    if (album != null)
                    {
                        return View["Details", album];
                    }
                }
                return View["Shared/Error"];
            };

            Get["browse/{genre}"] = _ =>
            {
                string genre = _.genre;
                ViewBag.Genre = genre;

                string cmd = "public.get_album_list_by_gname";
                var albumList = DBHelper.Query<AlbumListViewModel>(cmd, new
                {
                    gname = genre
                }, null, true, null, CommandType.StoredProcedure).ToList();
                return View["Browse", albumList];
            };
        }

        private IList<Genre> GetGenreList()
        {
            string cmd = "public.get_all_genres";
            return DBHelper.Query<Genre>(cmd, null, null, true, null, CommandType.StoredProcedure);
        }
    }
}