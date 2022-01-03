using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDao
    {
        Account GetAccount(int accountId);
        IList<Account> GetAllAccountsForUser(int userId);
        bool UpdateAccount(Account account);
        bool UpdateAccountsWithTransaction(IList<Account> accounts);

        // TODO add add account so that pre-existing user can have multiple accounts (optional)

        public AccountWithUsername GetUserByAccount(int accountId);

        IList<AccountWithUsername> GetAllAccounts();
    }
}
