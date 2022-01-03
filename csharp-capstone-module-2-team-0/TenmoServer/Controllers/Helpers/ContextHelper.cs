using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;

namespace TenmoServer.Controllers.Helpers
{
    public class ContextHelper
    {
        public static bool UserOwnsAccount(IAccountDao accountDao, int accountId,
            System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            int? userId = int.Parse(claimsPrincipal.FindFirst("sub")?.Value);
            return userId != null && accountDao.GetAccount(accountId).UserId == userId.Value;
        }
    }
}
