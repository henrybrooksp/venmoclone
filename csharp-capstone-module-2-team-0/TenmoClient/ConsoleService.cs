using System;
using System.Collections.Generic;
using TenmoClient.Models;
using TenmoClient.Services;
namespace TenmoClient
{
    public class ConsoleService
    {
        /// <summary>
        /// Prompts for transfer ID to view, approve, or reject
        /// </summary>
        /// <param name="action">String to print in prompt. Expected values are "Approve" or "Reject" or "View"</param>
        /// <returns>ID of transfers to view, approve, or reject</returns>
        public int PromptForTransferID(string action)
        {
            Console.WriteLine("");
            Console.Write("Please enter transfer ID to " + action + " (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int auctionId))
            {
                Console.WriteLine("Invalid input. Only input a number.");
                return 0;
            }
            else
            {
                return auctionId;
            }
        }

        public LoginUser PromptForLogin()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            string password = GetPasswordFromConsole("Password: ");

            LoginUser loginUser = new LoginUser
            {
                Username = username,
                Password = password
            };
            return loginUser;
        }

        private string GetPasswordFromConsole(string displayMessage)
        {
            string pass = "";
            Console.Write(displayMessage);
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine("");
            return pass;
        }
        public void PrintAccount(Account account)
        {
            Console.WriteLine("User ID: " + account.UserId);
            Console.WriteLine("Account ID: "+ account.AccountId);
            Console.WriteLine("Your current balance is: " + account.Balance.ToString("C2"));
        }
        public void PrintTransfers(List<Transfer> transfers, int accountId)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Transfers");
            Console.WriteLine("ID".PadRight(10, ' ')+"From/To".PadRight(15,' ')+"Amount");
            Console.WriteLine("-------------------------------------------");
            foreach(Transfer transfer in  transfers)
            {
                if (transfer.AccountFromId != accountId)
                {
                    Console.Write(transfer.TransferId.ToString().PadRight(10, ' '));
                    Console.Write("From: ".PadRight(5, ' ')+ AccountService.GetUsernameFromAccountId(transfer.AccountFromId).PadRight(15,' '));
                    Console.WriteLine($"{transfer.AmountToTransfer:C2}");
                }
                else
                {
                    Console.Write(transfer.TransferId.ToString().PadRight(10, ' '));
                    Console.Write("To: ".PadRight(5, ' ') + AccountService.GetUsernameFromAccountId(transfer.AccountToId).PadRight(15, ' '));
                    Console.WriteLine($"{transfer.AmountToTransfer:C2}");
                }
            }
        }
        public void PrintTransferDetails(Transfer transfer,AccountService accountService)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Transfer Details:");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Id: "+ transfer.TransferId);
            Console.WriteLine("From: " + AccountService.GetUsernameFromAccountId(transfer.AccountFromId));
            Console.WriteLine("To: "+ AccountService.GetUsernameFromAccountId(transfer.AccountToId));
            Console.WriteLine("Type: "+ GetTransferType(transfer.TypeId));
            Console.WriteLine("Status: " + GetTransferStatus(transfer.StatusId));
            Console.WriteLine($"Amount: {transfer.AmountToTransfer:C2}");
        }

        private string GetTransferType(int typeId)
        {
            if(typeId==1)
            {
                return "Send";
            }
            else if(typeId == 2)
            {
                return "Request";
            }
            else
            {
                return "";
            }
        }

        private string GetTransferStatus(int statusId)
        {
            if (statusId == 1)
            {
                return "Pending";
            }
            else if (statusId == 2)
            {
                return "Complete";
            }
            else if (statusId == 3)
            {
                return "Rejected";
            }
            else
            {
                return "";
            }
        }

        internal int PromptForUserSelection(string action)
        {
            Console.WriteLine("");
            Console.WriteLine($"Input an account id to {action}.");
            if (!int.TryParse(Console.ReadLine(), out int accountId))
            {
                Console.WriteLine("Invalid input. Only input a number.");
                return 0;
            }
            else
            {
                return accountId;
            }

        }

        internal Transfer PromptForNewTransfer(Account userAccount, int selectedAccountId, bool isSend)
        {
            Transfer newTransfer = new Transfer();
            if (isSend)
            {
                decimal amount = -1M;
                while (amount <= 0)
                {
                    Console.WriteLine("");
                    Console.Write("Enter amount to send: ");
                    if (decimal.TryParse(Console.ReadLine(), out amount))
                    { 
                        if (amount > userAccount.Balance)
                        {
                            Console.WriteLine("You can't send more money than you have in your account");
                            amount = -1M;
                        }
                    };
                }
                newTransfer.AccountFromId = userAccount.AccountId;
                newTransfer.AccountToId = selectedAccountId;
                newTransfer.StatusId = TransferStatus.APPROVED;
                newTransfer.TypeId = TransferType.SEND;
                newTransfer.AmountToTransfer = amount;
            }
            else
            {
                decimal amount = -1M;
                while (amount <= 0)
                {
                    Console.WriteLine("");
                    Console.Write("Enter amount to send: ");
                    decimal.TryParse(Console.ReadLine(), out amount);
                
                }
                newTransfer.AccountToId = userAccount.AccountId;
                newTransfer.AccountFromId = selectedAccountId;
                newTransfer.StatusId = TransferStatus.PENDING;
                newTransfer.TypeId = TransferType.REQUEST;
                newTransfer.AmountToTransfer = amount;
            }
            return newTransfer;

        }

        internal void PrintUsersExceptCurrent(List<AccountWithUsername> users)
        {
            Console.WriteLine("");
            Console.WriteLine("Existing Accounts and Users");
            Console.WriteLine("--------------------------------------------");
            Console.Write("ACCOUNT ID".PadRight(10));
            Console.Write("USERNAME".PadRight(10));
            Console.WriteLine("USER ID");
            Console.WriteLine("--------------------------------------------");
            foreach (AccountWithUsername account in users)
            {
                if (account.UserId != UserService.GetUserId())
                {
                    PrintAccountWithUser(account);
                }
            }
            Console.WriteLine("");
        }

        private void PrintAccountWithUser(AccountWithUsername account)
        {
            Console.Write(account.AccountId.ToString().PadRight(10));
            Console.Write(account.Username.ToString().PadRight(10));
            Console.WriteLine(account.UserId);
        }

        internal void ProcessClosedTransfer(Transfer closedTransfer)
        {
            if (closedTransfer != null)
            {
                string status = "";
                
                switch(closedTransfer.StatusId)
                {
                    case 1:
                        status = "pending";
                        break;
                    case 2:
                        status = "complete";
                        break;
                    case 3:
                        status = "rejected";
                        break;
                }
                Console.WriteLine($"Transfer {closedTransfer.TransferId} is {status}.");
            }
            else
            {
                Console.WriteLine("The requested action could not be completed.");
            }
        }

        internal void PrintPendingTransfers(List<Transfer> transfers, bool userIsSender)
        {
            string toFrom = userIsSender ? "To" : "From";
            
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Users");
            Console.WriteLine("ID".PadRight(10) + toFrom.PadRight(15)+"Amount");
            Console.WriteLine("--------------------------------------------");
            foreach (Transfer tran in transfers)
            {
                int accountToShow;
                if (userIsSender)
                {
                    accountToShow = tran.AccountToId;
                }
                else
                {
                    accountToShow = tran.AccountFromId;
                }
                Console.Write(tran.TransferId.ToString().PadRight(10));
                Console.Write(AccountService.GetUsernameFromAccountId(accountToShow).PadRight(15));
                Console.WriteLine($"{tran.AmountToTransfer:C2}");
            }
        }

        internal void GetCurrentBalance(Account account)
        {
            Console.WriteLine($"Your current balance is {account.Balance:C2}");
        }

        internal int PromptForCompleteOrReject()
        {
            Console.WriteLine("Please select an option: ");
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Reject");
            Console.WriteLine("[Any other]: Don't approve or reject");
            if (!int.TryParse(Console.ReadLine(), out int userChoice) || (userChoice != 1 && userChoice != 2))
            {
                Console.WriteLine("Going back...");
                return 0;
            }
            else
            {
                if (userChoice == 1)
                {
                    return TransferStatus.APPROVED;
                }
                else
                {
                    return TransferStatus.REJECTED;
                }
            }
        }
    }
}
