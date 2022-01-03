using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoServer.Models
{
    public enum TransferStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
    public enum TransferType
    {
        Request = 1,
        Send = 2
    }

    public class Transfer
    {
        public int TransferId { get; set; }
        public int TypeId { get; set; } // FK to transfer_types
        public int StatusId { get; set; } // FK to transfer_status
        public int AccountFromId { get; set; } //FK to accounts
        public int AccountToId { get; set; } // FK to accounts
        public decimal AmountToTransfer { get; set; }
        public Transfer() // TODO build constructor with everything but PK
        {

        }
    }
}
