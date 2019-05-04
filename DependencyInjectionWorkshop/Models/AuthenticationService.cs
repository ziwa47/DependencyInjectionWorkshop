using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var isLockResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockResponse.EnsureSuccessStatusCode();

            if(isLockResponse.Content.ReadAsAsync<bool>().Result)
                throw new FailedTooManyTimesException();



            string dbHashPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbHashPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            var inputPasswordHash = hash.ToString();

            string otpResponse;
            httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                otpResponse = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            if (string.Equals(otpResponse, otp) && string.Equals(dbHashPassword, inputPasswordHash))
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {
                var addResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
                addResponse.EnsureSuccessStatusCode();

                var errMsg = $"accountId :{accountId} verify failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", errMsg, "my bot name");


                var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
                getFailedCountResponse.EnsureSuccessStatusCode();
                var count = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
                NLog.LogManager.GetCurrentClassLogger().Info($"accountId : {accountId}, failedTimes : {count}");
                
                return false;
            }

        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}