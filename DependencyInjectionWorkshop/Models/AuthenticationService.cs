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
    public class ProfileDao
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

    public class Sha256Adapter
    {
        public string Hash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }
    }

    public class OtpService
    {
        public string GetOtpResp(string account)
        {
            string otpResp;
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", account).Result;
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
    }

    public class FailedCounter
    {
        public void ResetFailedCount(string account)
        {
            //成功
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string account)
        {
            //失敗
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class NlogAdapter
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class SlackAdapter
    {
        public void PushMessage(string account)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(msg => { }, "my channel", $"{account}", "my bot name");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NlogAdapter _nlogAdapter = new NlogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = IsAccountLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var currentPassword = _profileDao.GetPassword(account);
            var hashPassword = _sha256Adapter.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(account);
                return true;
            }
            else
            {
                _slackAdapter.PushMessage(account);

                _failedCounter.AddFailedCount(account);

                int failedCount = _failedCounter.GetFailedCount(account);
                _nlogAdapter.Info($"account:{account} failed times:{failedCount}");
                
                return false;
            }
        }

        private bool IsAccountLocked(string account)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}