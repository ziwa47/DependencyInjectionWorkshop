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
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            var currentPassword = GetCurrentPasswordFromDb(account);
            var hashPassword = GetHashedPassword(password);
            var currentOpt = GetOtpResp(account, httpClient);

            if (hashPassword == currentPassword && otp == currentOpt)
            {
                //成功
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

                resetResponse.EnsureSuccessStatusCode();
                return true;
            }
            else
            {
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(msg => { }, "my channel", "my message", "my bot name");
                //失敗
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
                addFailedCountResponse.EnsureSuccessStatusCode();

                var failedCountResponse =
                    httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

                failedCountResponse.EnsureSuccessStatusCode();

                var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"account:{account} failed times:{failedCount}");

                return false;
            }
        }

        private static string GetOtpResp(string account, HttpClient httpClient)
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

        private static string GetHashedPassword(string password)
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

        private static string GetCurrentPasswordFromDb(string account)
        {
            string currentPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                currentPassword = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return currentPassword;
        }
    }

    public class FailedTooManyTimesException : Exception
    {

    }
}