using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
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
}