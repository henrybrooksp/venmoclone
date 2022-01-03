using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TransferService
    {

        private readonly string API_BASE_URL;
        private readonly string API_RESOURCE = "transfer/";
        private readonly IRestClient client;
        public TransferService(string _apiBaseUrl, string _userToken)
        {
            API_BASE_URL = _apiBaseUrl;
            client = new RestClient();
            client.Authenticator = new JwtAuthenticator(_userToken);
        }
        public List<Transfer> GetPastTransfers(bool includeApproved=true, bool includeRejected = true)
        {
            RestRequest request = new RestRequest(API_BASE_URL + API_RESOURCE);
            request.AddParameter("userId", UserService.GetUserId());
            request.AddParameter("includeApproved", includeApproved);
            request.AddParameter("includeRejected", includeRejected);

            IRestResponse<IList<Transfer>> response = client.Get<IList<Transfer>>(request);

            try
            {
                ProcessResponse(response);
            } catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }
            return (List<Transfer>)response.Data;


        }

        private void ProcessResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(response.ErrorMessage, response.ErrorException);
            }
            else if (!response.IsSuccessful)
            {
                throw new Exception($"Error - Response Code: {response.StatusCode}");
            }
        }

        public List<Transfer> GetPendingTransfers(bool includePendingFrom=true, bool includePendingTo = true)
        {
            RestRequest request = new RestRequest(API_BASE_URL + API_RESOURCE);
            request.AddParameter("userId", UserService.GetUserId());
            request.AddParameter("includePendingFrom", includePendingFrom);
            request.AddParameter("includePendingTo", includePendingTo);

            IRestResponse<IList<Transfer>> response = client.Get<IList<Transfer>>(request);

            try
            {
                ProcessResponse(response);
            } catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }
            if (response.Data == null)
            {
                return new List<Transfer>();
            }
            
            return (List<Transfer>)response.Data;
        }

        /// <summary>
        /// Returns updated Transfer
        /// </summary>
        /// <param name="transferId">id for transfer in DB</param>
        /// <param name="transferStatus">uses Transfer.TransferStatus codes</param>
        /// <returns></returns>
        public Transfer PayOrRejectPendingTransfer(int transferId, int newTransferStatus)
        {
            // use request to GET old transfer

            RestRequest request = new RestRequest(API_BASE_URL + API_RESOURCE + $"{transferId}");
            IRestResponse<Transfer> getResponse = client.Get<Transfer>(request);
            try
            {
                ProcessResponse(getResponse);
            } catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }

            Transfer existingTransfer = getResponse.Data;
            if (existingTransfer == null)
            {
                return null;
            }

            // modify with new status, add jsonbody to request
            existingTransfer.StatusId = newTransferStatus;
            // PUT request
            request.AddJsonBody(existingTransfer);
            IRestResponse<Transfer> putResponse = client.Put<Transfer>(request);
            // check resposne
            try
            {
                ProcessResponse(putResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }
            return putResponse?.Data;

        }

        public Transfer CreateNewTransfer(Transfer newTransfer)
        {
            RestRequest request = new RestRequest(API_BASE_URL + API_RESOURCE);
            request.AddJsonBody(newTransfer);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            try
            {
                ProcessResponse(response);
            } catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }
            if (response.Data == null)
            {
                Console.WriteLine("Something went wrong; Transfer not created");
                return null;
            }
            return response.Data;
        }
        public Transfer GetTransferByTransferId(int transferId)
        {
            RestRequest request = new RestRequest(API_BASE_URL + API_RESOURCE + transferId);
            IRestResponse<Transfer> response = client.Get<Transfer>(request);
            try
            {
                ProcessResponse(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while communicating with the server");
                Console.WriteLine(e.Message);
                return null;
            }
            return response.Data;
        }


    }
}
