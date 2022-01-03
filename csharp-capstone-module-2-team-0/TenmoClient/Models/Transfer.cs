using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TypeId { get; set; } // FK to transfer_types
        public int StatusId { get; set; } // FK to transfer_status
        public int AccountFromId { get; set; } //FK to accounts
        public int AccountToId { get; set; } // FK to accounts
        public decimal AmountToTransfer { get; set; }
    }
    public static class TransferStatus
    {
        public const int PENDING = 1;
        public const int APPROVED = 2;
        public const int REJECTED = 3;
    }

    public static class TransferType
    {
        public const int REQUEST = 1;
        public const int SEND = 2;
    }
}
