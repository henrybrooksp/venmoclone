using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        Transfer AddTransfer(Transfer transfer);
        bool UpdateTransfer(int transferId, int statusId);
        Transfer GetTransfer(int transferId);

        // I think that at somepoint, we may want to add an additional param for accountId, if
        // if the user were able to create multiple accounts
        IList<Transfer> GetAllTransfersForUserId(int userId); // Any transfers (to or from) associated with userId
        IList<Transfer> GetTransfersForUserByStatus(int userId, int statusId);

        /// <summary>
        /// sender=true will get pending transfers where account_from == userId
        /// sender=false will get pending transfers where account_to == userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        IList<Transfer> GetPendingTransfersForUser(int userId, bool sender); //sender=true will get pending transfers we

    }
}
