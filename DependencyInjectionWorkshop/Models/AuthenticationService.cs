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

            string currentPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                currentPassword = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();

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

            if (hashPassword == currentPassword && otp == otpResp)
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

        //public void Notify(string message)
        //{
        //    var slackClient = new SlackClient("my api token");
        //    slackClient.PostMessage(response => { }, "my channel", "my message", "my bot name");
        //}

        //public string GetPassword(string accountId)
        //{
        //    using (var connection = new SqlConnection("my connection string"))
        //    {
        //        var password = connection.Query<string>("spGetUserPassword", new { Id = accountId },
        //            commandType: CommandType.StoredProcedure).SingleOrDefault();

        //        return password;
        //    }
        //}

        //public string GetHash(string plainText)
        //{
        //    var crypt = new System.Security.Cryptography.SHA256Managed();
        //    var hash = new StringBuilder();
        //    var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        //    foreach (var theByte in crypto)
        //    {
        //        hash.Append(theByte.ToString("x2"));
        //    }
        //    return hash.ToString();
        //}

        //public string GetOtp(string accountId)
        //{
        //    var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
        //    var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        return response.Content.ReadAsAsync<string>().Result;
        //    }
        //    else
        //    {
        //        throw new Exception($"web api error, accountId:{accountId}");
        //    }

        //}
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}