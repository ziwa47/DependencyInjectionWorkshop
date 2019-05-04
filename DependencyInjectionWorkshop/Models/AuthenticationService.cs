using Dapper;

using SlackAPI;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var passwordFromDb = GetPasswordFromDb(accountId);

            var hashPassword = GetHashPassword(password);

            var currentOtp = GetCurrentOtp(accountId);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                AddFailedCount(accountId);

                var failedCount = GetFailedCount(accountId);
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

        private static int GetFailedCount(string accountId)
        {
            var getFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var count = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return count;
        }

        private static void AddFailedCount(string accountId)
        {
            var addResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCounter(string accountId)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId)
        {
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
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

            return otpResponse;
        }

        private static string GetHashPassword(string password)
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
                dbHashPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return dbHashPassword;
        }

        private static void CheckAccountIsLocked(string accountId)
        {
            var isLockResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
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