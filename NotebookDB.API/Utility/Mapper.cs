using NotebookDB.Database;
using NotebookDB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NotebookDB.Server.Utility
{
    static public class Mapper
    {
        // static public Common.Account ToSharedAccount(this ApplicationUser user, IList<string> roles)
        // {

        //     var account = new Common.Account()
        //     {
        //         Id = user.Id,
        //         UserName = user.UserName,
        //         IsAdministrator = roles.Contains("Administrator"),
        //         AllowFolderAdministration = roles.Contains("FolderAdministrator")
        //     };

        //     return account;
        // }

        static public T Convert<T>(this IMapper source)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(source, settings);
            T result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }
    }
}
