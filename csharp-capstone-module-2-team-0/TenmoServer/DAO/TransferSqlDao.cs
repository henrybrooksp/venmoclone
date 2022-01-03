using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDao : ITransferDao
    {
        
        private readonly string connectionString;
        public TransferSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public Transfer AddTransfer(Transfer transfer)
        {
            int insertedId;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount)" +
                        " OUTPUT INSERTED.transfer_id" +
                        " VALUES (@typeId, @statusId, @accountFrom, @accountTo, @amount)", conn);

                    cmd.Parameters.AddWithValue("@typeId", transfer.TypeId);
                    cmd.Parameters.AddWithValue("@statusId", transfer.StatusId);
                    cmd.Parameters.AddWithValue("@accountFrom", transfer.AccountFromId);
                    cmd.Parameters.AddWithValue("@accountTo", transfer.AccountToId);
                    cmd.Parameters.AddWithValue("@amount", transfer.AmountToTransfer);

                    insertedId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            } catch (SqlException)
            {
                throw;
            }

            Transfer inserted = GetTransfer(insertedId);

            return inserted;
            
        }

        public Transfer GetTransfer(int transferId)
        {
            Transfer returnTransfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT * FROM transfers" +
                        " WHERE transfer_id=@transferId",conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnTransfer = GetTransferFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return returnTransfer;
        }

        public IList<Transfer> GetAllTransfersForUserId(int userId)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM transfers" +
                        " JOIN accounts ON transfers.account_from=accounts.account_id" +
                        " WHERE accounts.user_id=@userId" +
                        " UNION" +
                        " SELECT * FROM transfers" +
                        " JOIN accounts ON transfers.account_to=accounts.account_id" +
                        " WHERE accounts.user_id=@userId");
            cmd.Parameters.AddWithValue("@userId", userId);

            IList<Transfer> transfers = GetTransfersListHelper(cmd);
            return transfers;

        }
        public IList<Transfer> GetTransfersForUserByStatus(int userId, int statusId)
        {
            string sqlGet = "SELECT * FROM transfers WHERE transfer_id in (SELECT transfer_id FROM TRANSFERS JOIN accounts ON transfers.account_from=accounts.account_id WHERE user_id = @userId UNION SELECT transfer_id FROM transfers JOIN accounts ON transfers.account_to=accounts.account_id WHERE user_id=@userId) and transfer_status_id=@statusId";
            SqlCommand cmd = new SqlCommand(sqlGet);

            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@statusId", statusId);
            IList<Transfer> transfers = GetTransfersListHelper(cmd);
            return transfers;
        }

        public IList<Transfer> GetPendingTransfersForUser(int userId, bool sender)
        {
            // If sender, join on account_from, otherwise, join on account_to
            string accountJoinColumn = sender ? "account_from" : "account_to";
            string sqlSelectQuery = "SELECT * FROM transfers" +
                $" JOIN accounts ON transfers.{accountJoinColumn}=accounts.account_id" +
                " WHERE accounts.user_id=@userId and transfers.transfer_status_id=@statusId";
            SqlCommand cmd = new SqlCommand(sqlSelectQuery);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@statusId", (int)TransferStatus.Pending);

            IList<Transfer> transfers = GetTransfersListHelper(cmd);
            return transfers;
        }


        private IList<Transfer> GetTransfersListHelper(SqlCommand cmd)
        {
            List<Transfer> transfers = new List<Transfer>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    cmd.Connection = conn;

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        transfers.Add(GetTransferFromReader(reader));
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfers;
        }


        public bool UpdateTransfer(int transferId, int statusId)
        {
            if (!Enum.IsDefined(typeof(TransferStatus), statusId))
            {
                return false;
            }
            int rowsAffected = 0;
            try
            {
                Transfer existingTransfer = GetTransfer(transferId);
                if (existingTransfer == null)
                {
                    return false;
                }

                // should only be able to change status... right?
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlUpdateString = "UPDATE transfers" +
                        " SET transfer_status_id=@statusId" +
                        " WHERE transfer_id=@transferId";
                    SqlCommand cmd = new SqlCommand(sqlUpdateString, conn);
                    cmd.Parameters.AddWithValue("@statusId", statusId);
                    cmd.Parameters.AddWithValue("@transferId", transferId);

                    rowsAffected = cmd.ExecuteNonQuery();

                }

            } catch (SqlException)
            {
                throw;
            }
            return rowsAffected > 0;
        }

        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer t = new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TypeId = Convert.ToInt32(reader["transfer_type_id"]),
                StatusId = Convert.ToInt32(reader["transfer_status_id"]),
                AccountFromId = Convert.ToInt32(reader["account_from"]),
                AccountToId = Convert.ToInt32(reader["account_to"]),
                AmountToTransfer = Convert.ToDecimal(reader["amount"])
            };
            return t;
        }
    }
}
