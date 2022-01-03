using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDao : IAccountDao
    {
        private readonly string connectionString;
        public AccountSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public Account GetAccount(int accountId)
        {
            Account returnAccount = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT account_id, user_id, balance from accounts WHERE account_id = @accountId", conn);
                cmd.Parameters.AddWithValue("@accountId", accountId);
                SqlDataReader reader = cmd.ExecuteReader();
                if(reader.Read())
                {
                    returnAccount = GetAccountFromReader(reader);
                }

            }
            return returnAccount;
        }

        public IList<AccountWithUsername> GetAllAccounts()
        {
            List<AccountWithUsername> returnAccounts = new List<AccountWithUsername>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select account_id, accounts.user_id, username from accounts join users on accounts.user_id = users.user_id", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    AccountWithUsername aa = GetAccountWithUsernameFromReader(reader);
                    returnAccounts.Add(aa);
                }
            }
            return returnAccounts;
        }

        public IList<Account> GetAllAccountsForUser(int userId)
        {
            List<Account> returnAccounts = new List<Account>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * from accounts WHERE user_id=@userId", conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    Account aa = GetAccountFromReader(reader);
                    returnAccounts.Add(aa);
                }
            }
            return returnAccounts;
        }
        public bool UpdateAccount(Account account)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE accounts SET balance = @balance WHERE account_id = @accountId", conn);
                cmd.Parameters.AddWithValue("@balance", account.Balance);
                cmd.Parameters.AddWithValue("@accountId", account.AccountId);

                int rowsAffected = cmd.ExecuteNonQuery();

                return (rowsAffected > 0);
            }
        }

        /// <summary>
        /// takes in a list of accounts to be updated and stages a transaction to
        /// update all of them simultaneously. if any are unable to be updated, the whole
        /// transaction will be rolled back and no accounts will be affected, and an error will be thrown.
        /// </summary>
        /// <param name="accounts"></param>
        /// <returns></returns>
        public bool UpdateAccountsWithTransaction(IList<Account> accounts)
        {
            SqlTransaction transaction = null;
           
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach(Account account in accounts)
                    {
                        // update that account
                        // if any fail, throw up
                        string sqlUpdateAccount = "UPDATE accounts SET balance=@balance WHERE account_id = @accountId";
                        SqlCommand cmd = new SqlCommand(sqlUpdateAccount, conn, transaction);
                        cmd.Parameters.AddWithValue("@balance", account.Balance);
                        cmd.Parameters.AddWithValue("@accountId", account.AccountId);
                        if (cmd.ExecuteNonQuery() < 1)
                        {
                            throw new Exception("Given account could not be updated. Rolling back transaction.");
                        }
                    }
                    transaction.Commit();
                }
                catch // wanna catch ANYTHING
                {
                    transaction?.Rollback(); // if the transaction was opened and we had an exception, rollback
                    throw;
                }
            }
            return true;
        }

        public AccountWithUsername GetUserByAccount(int accountId)
        {
            AccountWithUsername account = null; ;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlGetUser = "SELECT *, users.username FROM accounts" +
                        " JOIN users ON accounts.user_id=users.user_id" +
                        " WHERE accounts.account_id=@accountId";
                    SqlCommand cmd = new SqlCommand(sqlGetUser, conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        account = GetAccountWithUsernameFromReader(reader);
                    }
                }
            }
            catch
            {
                return null;
            }
            return account;
        }

        private Account GetAccountFromReader(SqlDataReader reader)
        {
            Account acc = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"])
            };
            return acc;
        }
        private AccountWithUsername GetAccountWithUsernameFromReader(SqlDataReader reader)
        {
            AccountWithUsername acc = new AccountWithUsername()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"])
            };
            return acc;
        }


        
    }
}
