using Microsoft.AspNetCore.Authorization;
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
    public class AccountController : ControllerBase
    {
        private readonly IAccountDao accountDao;
        private readonly ITransferDao transferDao;
        private readonly IUserDao userDao;

        public AccountController(IAccountDao _accountDao, ITransferDao _transferDao, IUserDao _userDao)
        {
            accountDao = _accountDao;
            transferDao = _transferDao;
            userDao = _userDao;
        }
        //todo add auth and user verification to GetAccount
        [HttpGet("{id}")]
        public ActionResult<Account> GetAccount(int id)
        {
            // todo - makesure account belongs to user, make helper
            if (!ContextHelper.UserOwnsAccount(accountDao, id, User))
            {
                return Forbid("The account you're trying to access is not your own.");
            }
            Account account = accountDao.GetAccount(id);
            if(account != null)
            {
                return Ok(account);
            }
            return NotFound("Could not find an account with that id.");
        }

        [HttpGet]
        public ActionResult<List<AccountWithUsername>> GetAccountUsersAndIds()
        {
            IList<AccountWithUsername> users;
            // ask the dao for all the users
            
            users = accountDao.GetAllAccounts(); 
            
            if(users != null)
            {
                return Ok(users);
            }
            return NotFound("No users in the database");    
        }
        [HttpGet("user/{id}")]
        public ActionResult<List<Account>> GetAccountsForUser(int id)
        {
            IList<Account> accounts;
            if (!(Convert.ToInt32(User.FindFirst("sub")?.Value) == id))
            {
                return Forbid();
            }
            try
            {
                accounts= accountDao.GetAllAccountsForUser(id);
            }
            catch
            {
                return StatusCode(500);
            }
            if (accounts.Count < 1)
            {
                return NotFound();
            }
            return Ok(accounts);
        }
        [HttpGet("user")]
        public ActionResult<AccountWithUsername> FindUser(int accountId)
        {
            try
            {
                // select user based on account id
                AccountWithUsername account = accountDao.GetUserByAccount(accountId);
                if (account == null)
                {
                    return NotFound();
                }
                return account;
            }
            catch
            {
                return StatusCode(500);
            }
        }
        
    }
}
