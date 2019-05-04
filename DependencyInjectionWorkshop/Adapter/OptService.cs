using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Adapter
{
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
}