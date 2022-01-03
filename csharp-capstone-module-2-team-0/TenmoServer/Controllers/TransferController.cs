using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;
using TenmoServer.Controllers.Helpers;

namespace TenmoServer.Controllers
{   
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly IAccountDao accountDao;
        private readonly ITransferDao transferDao;
        private readonly IUserDao userDao;

        public TransferController(IAccountDao _accountDao, ITransferDao _transferDao, IUserDao _userDao)
        {
            accountDao = _accountDao;
            transferDao = _transferDao;
            userDao = _userDao;
        }

        [HttpGet]
        public ActionResult<IList<Transfer>> GetTransfersByUser(int userId, bool includeApproved=false,
            bool includePendingFrom=false, bool includePendingTo=false, bool includeRejected=false)
        {
            if (!includePendingFrom && !includeRejected && !includeApproved && !includePendingTo)
            {
                return BadRequest("Either includePendingTo, includePendingFrom, includeRejected, or includeApproved must be true");
            }

           List<Transfer> transfers = new List<Transfer>();

            try
            {
                if (accountDao.GetAllAccountsForUser(userId).Count == 0)
                {
                    return NotFound("No accounts exist for the provided userId");
                }

                // Using  if/else here because single dao access is faster in current implementation with SQL
                // 
                if (includeApproved && includePendingFrom && includePendingTo && includeRejected)
                {
                    transfers = (List<Transfer>)transferDao.GetAllTransfersForUserId(userId);
                } else
                {
                    if (includeApproved)
                    {
                        transfers.AddRange(transferDao.GetTransfersForUserByStatus(userId, (int)TransferStatus.Approved));
                    }
                    if (includeRejected)
                    {
                        transfers.AddRange(transferDao.GetTransfersForUserByStatus(userId, (int)TransferStatus.Rejected));
                    }
                    if (includePendingTo)
                    {
                        transfers.AddRange(transferDao.GetPendingTransfersForUser(userId, false));
                    }
                    if (includePendingFrom)
                    {
                        transfers.AddRange(transferDao.GetPendingTransfersForUser(userId, true));
                    }
                }
            }
            catch
            {
                return StatusCode(500);
            }
            return transfers;
        }
        [HttpGet("{id}")]
        public ActionResult<Transfer> GetTransferByTransferId(int id)
        {
            try
            {
                Transfer transfer = transferDao.GetTransfer(id);
                if (transfer == null)
                {
                    return NotFound();
                }
                return transfer;
            } catch
            {
                return StatusCode(500);
            }
        }
        [HttpPost]
        public ActionResult<Transfer> CreateNewTransfer(Transfer sendTransfer)
        {
            try
            {
                if (!Enum.IsDefined(typeof(TransferType), sendTransfer.TypeId) ||
                    !Enum.IsDefined(typeof(TransferStatus), sendTransfer.StatusId))
                {
                    return BadRequest("ill-formatted post body: invalid typeId and/or statusId");
                }
                if (sendTransfer == null)
                {
                    return BadRequest("Not a valid transfer.");
                }
                // Check auth
                // Check if its a send or a request
                if (sendTransfer.TypeId == (int)TransferType.Send)
                {
                    // If it's a send
                    // make sure sendTransfer is actually a send type

                    // Ensure that account_from userId matches User.FindFirst("sub")?.Value

                    if (!ContextHelper.UserOwnsAccount(accountDao, sendTransfer.AccountFromId, User))
                    {
                        return Forbid("The account you're trying to send from is not your own.");
                    }
                    // make sure it's valid and do-able (make sure theres enough money, and that both accounts exist)
                    if (sendTransfer.AmountToTransfer > accountDao.GetAccount(sendTransfer.AccountFromId).Balance)
                    {
                        return BadRequest("You don't have enough money in your account to make that transfer.");
                    }
                    if (accountDao.GetAccount(sendTransfer.AccountFromId) == null || accountDao.GetAccount(sendTransfer.AccountToId) == null)
                    {
                        return BadRequest("Sorry, an account with that id does not exist.");
                    }
                    // Use ExecuteTransfer
                }
                else if (sendTransfer.TypeId == (int)TransferType.Request)
                {
                    // If it's a request
                    // makesure that account_to userid matches User.FindFirst("sub")?.Value
                    if (!ContextHelper.UserOwnsAccount(accountDao, sendTransfer.AccountToId, User))
                    {
                        return Forbid("The account you're trying to request from is not your own.");
                    }
                    if (accountDao.GetAccount(sendTransfer.AccountToId) == null)
                    {
                        return BadRequest("The account you're trying to request from does not exist.");
                    }
                }

                // Create the request, don't execute
                Transfer createdTransfer = transferDao.AddTransfer(sendTransfer);
                if (createdTransfer == null)
                {
                    return StatusCode(500);
                }
                if (createdTransfer.StatusId == (int)TransferStatus.Approved)
                {
                    ExecuteTransfer(createdTransfer);
                }
                return createdTransfer;
            }
            catch
            {
                return StatusCode(500);
            }
        }
      

        [HttpPut("{id}")]
        public ActionResult<Transfer> UpdateTransfer(Transfer updatedTransfer, int id)
        {
            try
            {
                // In terms of user authorization
                // if you are the user calling this method
                bool isUpdated = false;

                if (updatedTransfer.TransferId != id)
                {
                    return BadRequest("transfer_id in url does not match transfer_id in PUT body.");
                }
                // then you should be the owner of updatedTransfer.accountFromId
                if (!ContextHelper.UserOwnsAccount(accountDao, updatedTransfer.AccountFromId, User))
                {
                    return Forbid("The transfer that you're trying to access is for an account that is not own you may update");
                }
                // exististing transfer should have status 1 (pending)
                if (transferDao.GetTransfer(id) == null)
                {
                    return BadRequest("A transfer with that id does not exist");
                }
                // updatedTranfer.transferStatusId == 2 (approved/complete) or 3 (rejected)
                if (transferDao.GetTransfer(id).StatusId == (int)TransferStatus.Pending)
                {
                    if (updatedTransfer.StatusId == (int)TransferStatus.Approved || updatedTransfer.StatusId == (int)TransferStatus.Rejected)
                    {
                        // makesure funds available
                        if (TransferFromHasSufficientBalance(updatedTransfer))
                        {
                            isUpdated = transferDao.UpdateTransfer(updatedTransfer.TransferId, updatedTransfer.StatusId);
                        }
                    }
                }



                // check all validity
                if (isUpdated)
                {
                    Transfer transfer = transferDao.GetTransfer(id);
                    if (transfer.StatusId == (int)TransferStatus.Approved)
                    {
                        // was approved
                        ExecuteTransfer(transfer);
                    }
                    return transfer;
                } else
                {
                    return StatusCode(418); // idk what went wrong...
                }
            } catch {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Returns true if the account sending money has sufficient funds for transactions
        /// Throws exception if accountFromId does not correspond to real account
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        private bool TransferFromHasSufficientBalance(Transfer transfer)
        {
            if (transfer == null) return false;

            try
            {
                Account accountFrom = accountDao.GetAccount(transfer.AccountFromId);
                return transfer.AmountToTransfer <= accountFrom.Balance;
            } catch
            {
                throw;
            }
        }
        private bool ExecuteTransfer(Transfer transfer)
        {
            if (transfer == null)
            {
                return false;
            }
            try
            {
                Account accountFrom = accountDao.GetAccount(transfer.AccountFromId);
                Account accountTo = accountDao.GetAccount(transfer.AccountToId);

                if (accountTo == null || accountFrom == null)
                {
                    return false;
                }
                if (transfer.AmountToTransfer > accountFrom.Balance)
                {
                    return false;
                }
                accountFrom.Balance -= transfer.AmountToTransfer;
                accountTo.Balance += transfer.AmountToTransfer;

                List<Account> accountsToUpdate = new List<Account>();
                accountsToUpdate.Add(accountTo);
                accountsToUpdate.Add(accountFrom);

                return accountDao.UpdateAccountsWithTransaction(accountsToUpdate);
            }
            catch (Exception)
            {

                throw;
            }
        }       
        
    }
}
