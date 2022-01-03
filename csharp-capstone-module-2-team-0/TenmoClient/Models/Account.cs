using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TenmoClient.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
    }
    public class AccountWithUsername
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
    }
}
