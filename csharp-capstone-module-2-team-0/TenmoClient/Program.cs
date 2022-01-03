using System;
using System.Collections.Generic;
using System.Net;
using TenmoClient.Models;
using TenmoClient.Services;

namespace TenmoClient
{
    class Program
    {
        private readonly static string API_BASE_URL = "https://localhost:44315/";
        private static readonly ConsoleService consoleService = new ConsoleService();
        private static readonly AuthService authService = new AuthService(API_BASE_URL);
        private static AccountService accountService;
        private static TransferService transferService;
        

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            Run();
        }

        private static void Run()
        {
            int loginRegister = -1;
            while (loginRegister != 1 && loginRegister != 2)
            {
                Console.WriteLine("Welcome to TEnmo!");
                Console.WriteLine("1: Login");
                Console.WriteLine("2: Register");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out loginRegister))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (loginRegister == 1)
                {
                    while (!UserService.IsLoggedIn()) //will keep looping until user is logged in
                    {
                        LoginUser loginUser = consoleService.PromptForLogin();
                        ApiUser user = authService.Login(loginUser);
                        if (user != null)
                        {
                            UserService.SetLogin(user);
                            accountService = new AccountService(API_BASE_URL, UserService.GetToken());
                            transferService = new TransferService(API_BASE_URL, UserService.GetToken());
    }
                    }
                }
                else if (loginRegister == 2)
                {
                    bool isRegistered = false;
                    while (!isRegistered) //will keep looping until user is registered
                    {
                        LoginUser registerUser = consoleService.PromptForLogin();
                        isRegistered = authService.Register(registerUser);
                        if (isRegistered)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Registration successful. You can now log in.");
                            loginRegister = -1; //reset outer loop to allow choice for login
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }

            MenuSelection();
        }

        private static void MenuSelection()
        {
            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    //maybe later print out list of account id and balance for user
                    int userId = UserService.GetUserId();
                    Account account = accountService.GetAccountFromUserId(userId);
                    consoleService.PrintAccount(account);
                }
                else if (menuSelection == 2)
                {
                    int userId = UserService.GetUserId();
                    int accountId = accountService.GetAccountFromUserId(userId).AccountId;
                    List<Transfer> pastTransfers = transferService.GetPastTransfers();
                    consoleService.PrintTransfers(pastTransfers, accountId);
                    int transferId = consoleService.PromptForTransferID("view");
                    if (transferId == 0)
                    {
                        Console.WriteLine("");
                        continue;
                    }
                    Transfer transfer = transferService.GetTransferByTransferId(transferId);

                    if (transfer == null)
                    {
                        Console.WriteLine("");
                        continue;
                    }
                    consoleService.PrintTransferDetails(transfer, accountService);

                }
                else if (menuSelection == 3)
                {
                    PendingMenu();                   
                }
                else if (menuSelection == 4)
                {
                    List<AccountWithUsername> users = accountService.GetAllAccounts();
                    consoleService.PrintUsersExceptCurrent(users);
                    int selectedAccountId = consoleService.PromptForUserSelection("send funds to");//action
                    if (selectedAccountId == 0)
                    {
                        continue;
                    }
                    if (AccountService.GetUsernameFromAccountId(selectedAccountId) == "")
                    {
                        Console.WriteLine("Invalid Account Id");
                        continue;
                    }
                    int userId = UserService.GetUserId();
                    Account userAccount = accountService.GetAccountFromUserId(userId);
                    Transfer newTransfer = consoleService.PromptForNewTransfer(userAccount, selectedAccountId, true); //currentUser, recipient, isSend
                    Transfer createdTransfer = transferService.CreateNewTransfer(newTransfer);
                    consoleService.ProcessClosedTransfer(createdTransfer);
                }
                else if (menuSelection == 5)
                {
                    List<AccountWithUsername> users = accountService.GetAllAccounts();
                    consoleService.PrintUsersExceptCurrent(users);
                    int selectedAccountId = consoleService.PromptForUserSelection("request funds from");
                    if (selectedAccountId == 0)
                    {
                        continue;
                    }
                    if (AccountService.GetUsernameFromAccountId(selectedAccountId) == "")
                    {
                        Console.WriteLine("Invalid Account Id");
                        continue;
                    }
                    int userId = UserService.GetUserId();
                    Account userAccount = accountService.GetAccountFromUserId(userId);
                    Transfer newTransfer = consoleService.PromptForNewTransfer(userAccount, selectedAccountId, false); // isSend = false
                    Transfer createdTransfer = transferService.CreateNewTransfer(newTransfer);
                    consoleService.ProcessClosedTransfer(createdTransfer);
                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new ApiUser()); //wipe out previous login info
                    Run(); //return to entry point
                }
                else
                {
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
        private static void PendingMenu()
        {
            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Select outbound or incoming pending transfers:");
                Console.WriteLine("1: Outbound Requests");
                Console.WriteLine("2: Incoming Requests");
                Console.WriteLine("0: Go back");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    Console.WriteLine("");
                    List<Transfer> transfers = transferService.GetPendingTransfers(false, true);
                    if (transfers.Count == 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("No incoming pending transfers for user.");
                        Console.WriteLine("");
                        continue;
                    }
                    consoleService.PrintPendingTransfers(transfers, false);
                    Console.WriteLine("");
                }
                else if (menuSelection == 2)
                {
                    Console.WriteLine("");
                    List<Transfer> transfers = transferService.GetPendingTransfers(true, false);
                    if (transfers.Count == 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("No incoming pending transfers for user.");
                        Console.WriteLine("");
                        continue;
                    }
                    consoleService.PrintPendingTransfers(transfers, true);
                    Console.WriteLine("");
                    int transferId = consoleService.PromptForTransferID("complete/reject");
                    if (transferId == 0)
                    {
                        continue;
                    }
                    if (transferService.GetTransferByTransferId(transferId) == null) // will return null if either DNE or is not users to access
                    {
                        Console.WriteLine("Invalid tranfer id entered");
                    }
                    //todo - makesure transfer id is actually a transfer
                    int userId = UserService.GetUserId();
                    Account account = accountService.GetAccountFromUserId(userId);
                    consoleService.GetCurrentBalance(account);
                    int statusId = consoleService.PromptForCompleteOrReject();
                    if (statusId == 0)
                    {
                        Console.WriteLine("");
                        return;
                    }
                    Transfer closedTransfer = transferService.PayOrRejectPendingTransfer(transferId, statusId);
                    consoleService.ProcessClosedTransfer(closedTransfer);


                }
            }
        }
    }
}
