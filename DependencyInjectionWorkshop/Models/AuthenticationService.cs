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
        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = IsAccountLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var currentPassword = GetCurrentPasswordFromDb(account);
            var hashPassword = GetHashedPassword(password);
            var currentOpt = GetOtpResp(account, httpClient);

            if (hashPassword == currentPassword && otp == currentOpt)
            {
                ResetFaildCount(account, httpClient);
                return true;
            }
            else
            {
                PushMessage();
                AddFailedCount(account, httpClient);
                LogFailedCount(account, httpClient);

                return false;
            }
        }

        private void LogFailedCount(string account, HttpClient httpClient)
        {
            var failedCount = GetFailedCount(account, httpClient);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"account:{account} failed times:{failedCount}");
        }

        private int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private void AddFailedCount(string account, HttpClient httpClient)
        {
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private void PushMessage()
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(msg => { }, "my channel", "my message", "my bot name");
        }

        private bool IsAccountLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private void ResetFaildCount(string account, HttpClient httpClient)
        {
            //成功
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private string GetOtpResp(string account, HttpClient httpClient)
        {
            string otpResp;
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
                otpResp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            return otpResp;
        }

        private string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }

        private string GetCurrentPasswordFromDb(string account)
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

    public class FailedTooManyTimesException : Exception
    {
    }
}