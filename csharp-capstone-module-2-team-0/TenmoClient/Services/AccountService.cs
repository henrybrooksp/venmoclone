using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class AccountService
    {
        public AccountService(string _apiBaseUrl, string _userToken)
        {
            API_BASE_URL = _apiBaseUrl;
            client = new RestClient();
            client.Authenticator = new JwtAuthenticator(_userToken);
        }
        private readonly IRestClient client;
        

        private readonly string API_BASE_URL = "https://localhost:44315/";
        
        public Account GetAccount(int accountId)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "account/" + accountId);
            IRestResponse<Account> response = client.Get<Account>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessResponseForErrors(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }
        public Account GetAccountFromUserId(int userId)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "account/user/" + userId);
            IRestResponse<List<Account>> response = client.Get<List<Account>>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessResponseForErrors(response);
            }
            else if(response.Data.Count>0)
            {
                return response.Data[0];
            }
            return null;
        }
        public decimal GetBalance(int accountId)
        {
            return GetAccount(accountId).Balance;   
        }

        public bool AccountCanExecuteTranfer(Transfer transfer)
        {
            return transfer.AmountToTransfer <= GetBalance(transfer.AccountFromId);
        }

        public List<AccountWithUsername> GetAllAccounts(int userId=-1)
        {
            //OPTIONAL - IGNORE FOR NOW SINCE user <-> account is 1:1
            // This is for all accounts from user userId
            ////////////////////////////////////
            if (userId != -1)
            {
                //get specific
            }
            ////////////////////////////////////
            RestRequest request = new RestRequest(API_BASE_URL + "account");
            IRestResponse<List<AccountWithUsername>> response = client.Get<List<AccountWithUsername>>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessResponseForErrors(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }
        private void ProcessResponseForErrors(IRestResponse response)
        {
            if(response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception("Uh oh, unable to reach the server.", response.ErrorException);
            }
            else if(!response.IsSuccessful)
            {
                if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("You are not authorized. Try logging in?");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new Exception("Go away, you're not supposed to be here.");
                }
                throw new Exception("Something went wrong.", response.ErrorException);
            }
        }
        
        public static string GetUsernameFromAccountId(int accountId)
        {
            IRestClient client = new RestClient();
            string API_BASE = "https://localhost:44315/";
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());

            try
            {
                RestRequest request = new RestRequest(API_BASE + "account/user/");
                request.AddParameter("accountId", accountId);
                IRestResponse<AccountWithUsername> response = client.Get<AccountWithUsername>(request);
                if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful || response.Data == null)
                {
                    return "";
                }
                return response.Data.Username;
            }
            catch
            {
                return "";
            }
            
        }
    }
}
