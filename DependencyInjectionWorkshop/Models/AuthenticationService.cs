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
    public class ProfileRepo
    {
        public string GetPasswordFromDb(string accountId)
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

    public class Sha256Adapter
    {
        public string GetHashPassword(string password)
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
    }

    public class OptService
    {
        public string GetCurrentOtp(string accountId)
        {
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            string otpResponse;
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
    }

    public class FailedCounter
    {
        public void ResetFailedCounter(string accountId)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string accountId)
        {
            var addResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId)
        {
            var getFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            getFailedCountResponse.EnsureSuccessStatusCode();
            var count = getFailedCountResponse.Content.ReadAsAsync<int>().Result;
            return count;
        }

        public void CheckAccountIsLocked(string accountId)
        {
            var isLockResponse = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockResponse.EnsureSuccessStatusCode();

            if (isLockResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }
    }

    public class SlackAdapter
    {
        public void Notify(string errMsg)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", errMsg, "my bot name");
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OptService _optService = new OptService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profileRepo.GetPasswordFromDb(accountId);

            var hashPassword = _sha256Adapter.GetHashPassword(password);

            var currentOtp = _optService.GetCurrentOtp(accountId);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                _failedCounter.ResetFailedCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                _nLogAdapter.LogFailedCount($"accountId : {accountId}, failedTimes : {failedCount}");
                _slackAdapter.Notify($"accountId :{accountId} verify failed");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}