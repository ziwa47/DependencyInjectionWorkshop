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
            CheckAccountIsLocked(accountId, httpClient);

            var passwordFromDb = GetPasswordFromDb(accountId);

            var hashPassword = GetHashPassoword(password);

            var currentOtp = GetCurrentOtp(accountId, httpClient);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                ResetFaildCounter(accountId, httpClient);

                return true;
            }
            else
            {
                AddFailedCount(accountId, httpClient);

                var failedCount = GetFailedCount(accountId, httpClient);
                LogFailedCount($"accountId : {accountId}, failedTimes : {failedCount}");
                Notify($"accountId :{accountId} verify failed");

                return false;
            }

        }

        private static void Notify(string errMsg)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", errMsg, "my bot name");
        }

        private static void LogFailedCount(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var getFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var count = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return count;
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFaildCounter(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId, HttpClient httpClient)
        {
            string otpResponse;
            httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.dev/")};
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                otpResponse = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return otpResponse;
        }

        private static string GetHashPassoword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var inputPasswordHash = hash.ToString();
            return inputPasswordHash;
        }

        private static string GetPasswordFromDb(string accountId)
        {
            string dbHashPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                dbHashPassword = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbHashPassword;
        }

        private static void CheckAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockResponse.EnsureSuccessStatusCode();

            if (isLockResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}