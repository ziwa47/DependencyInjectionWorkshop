using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileDao
    {
        string GetPassword(string account);
    }

    public class ProfileDao : IProfileDao
    {
        public string GetPassword(string account)
        {
            string currentPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                currentPassword = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return currentPassword;
        }
    }
}