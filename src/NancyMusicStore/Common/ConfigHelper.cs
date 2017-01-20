using System.Configuration;

namespace NancyMusicStore.Common
{
    public class ConfigHelper
    {
        public static string GetAppSettingByKey(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();            
        }

        public static string GetConneectionStr()
        {
            return ConfigurationManager.ConnectionStrings["pgsqlConn"].ConnectionString.ToString();
        }
    }
}