﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repository
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileRepo : IProfile
    {
        public string GetPassword(string accountId)
        {
            string dbHashPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbHashPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbHashPassword;
        }
    }
}