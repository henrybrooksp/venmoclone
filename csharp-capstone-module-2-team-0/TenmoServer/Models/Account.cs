using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoServer.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public Account()
        {

        }
        public Account(int userId, decimal balance)
        {
            UserId = userId;
            Balance = balance;
        }
        
    }

    public class AccountWithUsername
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
    }
}
