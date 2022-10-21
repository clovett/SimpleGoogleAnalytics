using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GoogleAnalytics
{
    public class HttpProtocol
    {
        const string baseUrl = "https://www.google-analytics.com/mp/collect";
        const string debugBaseUrl = "https://www.google-analytics.com/debug/mp/collect";

        public static async Task PostMeasurements(Analytics a)
        {
            const string guide = "\r\nSee https://developers.google.com/analytics/devguides/collection/protocol/ga4";

            if (a.Events.Count > 25)
            {
                throw new Exception("A maximum of 25 events can be specified per request." + guide);
            }

            string query = a.ToQueryString();
            string url = baseUrl + "?" + query;

            if (string.IsNullOrEmpty(a.UserId))
            {
                a.UserId = a.ClientId;
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "GoogleAnalyticsDotNetClient");

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(a);

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            if (bytes.Length > 130000)
            {
                throw new Exception("The total size of analytics payloads cannot be greater than 130kb bytes" + guide);
            }

            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<ValidationResponse> ValidateMeasurements(Analytics a)
        {
            const string guide = "\r\nSee https://developers.google.com/analytics/devguides/collection/protocol/ga4";

            if (a.Events.Count > 25)
            {
                throw new Exception("A maximum of 25 events can be specified per request." + guide);
            }

            string query = a.ToQueryString();
            string url = debugBaseUrl + "?" + query;

            HttpClient client = new HttpClient();

            string platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "OSX");
            var arch = RuntimeInformation.OSArchitecture.ToString();
            client.DefaultRequestHeaders.Add("User-Agent", string.Format("Mozilla/5.0 ({0}; {1})", platform, arch));
            client.DefaultRequestHeaders.Add("Accept-Language", CultureInfo.CurrentCulture.Name);
            a.UserProperties = new UserProperties()
            {
                FrameworkVersion = new UserPropertyValue(RuntimeInformation.FrameworkDescription),
                Platform = new UserPropertyValue(platform),
                PlatformVersion = new UserPropertyValue(RuntimeInformation.OSDescription),
                Language = new UserPropertyValue(CultureInfo.CurrentCulture.Name)
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(a);

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            if (bytes.Length > 130000)
            {
                throw new Exception("The total size of analytics payloads cannot be greater than 130kb bytes" + guide);
            }

            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            if (response.Content != null)
            {
                var message = await response.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ValidationResponse>(message);
            }
            throw new Exception("No validation response");
        }
    }

    [DataContract]
    public class ValidationResponse
    {
        [DataMember(Name = "validationMessages")]
        public ValidationMessage[] ValidationMessages;
    }


    [DataContract]
    public class ValidationMessage
    {
        [DataMember(Name = "description")]
        public string Description;
        [DataMember(Name = "fieldPath")]
        public string InvalidFieldPath;
        [DataMember(Name = "validationCode")]
        public string ValidationCode;
    }
}
