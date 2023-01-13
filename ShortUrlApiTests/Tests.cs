using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShortURLsPostmanRequests
{
    public class Tests 
    {
        private const int InitiallyShortUrls = 3;
        private const string ActualShortCode = "seldev";
        private const string ActualUrl = "https://selenium.dev";
        private readonly string _uniqueUrl = GenerateUniqueUrl();
        private readonly string _differentUrl = GenerateUniqueUrl();
        private readonly string _uniqueShortCode = GenerateUniqueShortCode();
        private readonly string _differentShortCode = GenerateUniqueShortCode();


        private RestClient restClient;

        [SetUp]
        public void Setup()
        {
            var baseURL = new Uri("http://localhost:8080");

            restClient = new RestClient(baseURL);
        }

        [Order(1)]
        [Test]
        public void GetListOfShortURLs()
        {
            var request = new RestRequest("/api/urls", Method.Get);

            var response = restClient.Execute(request);
            var listOfShortUrls = JsonConvert.DeserializeObject<List<UrlEntry>>(response.Content);

            Assert.That((int)response.StatusCode == 200, "Status Code is different");
            Assert.IsTrue(listOfShortUrls.Count >= InitiallyShortUrls, 
                          "Number of initial Short URLs are different.");
        }

        [Order(2)]
        [Test]
        public void GetShortUrlByGivenShortCode()
        {
            var request = new RestRequest($"/api/urls/{ActualShortCode}", Method.Get);

            var response = restClient.Execute(request);
            var urlEntry = JsonConvert.DeserializeObject<UrlEntry>(response.Content);

            Assert.That((int)response.StatusCode == 200, "Expected Status Code is different.");
            Assert.That(urlEntry.ShortCode == ActualShortCode, "Short Code is different.");
            Assert.That(urlEntry.Url == ActualUrl, "Original Url is different.");

        }

        [Order(3)]
        [Test]
        public void PostNewShortUrlWithCorrectUrlAndCorrectShortCode()
        {
            var createShortUrlRequest = new RestRequest("/api/urls", Method.Post);

            var body = new { url = _uniqueUrl, shortCode = _uniqueShortCode };
            createShortUrlRequest.AddJsonBody(body);

            var response = restClient.Execute(createShortUrlRequest);

            Assert.That((int)response.StatusCode == 200, "Status Code is different.");

            var responseContent = JsonConvert.DeserializeObject<Message>(response.Content);

            Assert.AreEqual("Short code added.", responseContent.Msg, "Short Code is not added.");

            var getShortUrlRequest = new RestRequest($"/api/urls/{_uniqueShortCode}", Method.Get);
            var getShortUrlResponse = restClient.Execute(getShortUrlRequest);

            Assert.That(getShortUrlResponse.Content.Contains(_uniqueUrl),"Response does not contain original url.");
        }

        [Order(4)]
        [Test]
        public void TryToCreateNewShortUrlWithSameUrlAndShortCode()
        {
            var request = new RestRequest("/api/urls", Method.Post);

            var body = new { url = _uniqueUrl, shortCode = _uniqueShortCode };
            request.AddJsonBody(body);

            var response = restClient.Execute(request);

            Assert.That((int)response.StatusCode == 400, "Short Url added.");
            Assert.That(response.Content.Contains("Short code already exists!"),
                        "Error message is different.");
        }

        [Order(5)]
        [Test]
        public void TryToCreateNewShortUrlWithSameUrlAndDifferentShortCode()
        {
            var createShortUrlRequest = new RestRequest("/api/urls", Method.Post);

            var body = new { url = _uniqueUrl, shortCode = _differentShortCode };
            createShortUrlRequest.AddJsonBody(body);

            var createShortUrlResponse = restClient.Execute(createShortUrlRequest);

            Assert.That((int)createShortUrlResponse.StatusCode == 200, "Short code is not added.");
            
            var responseContent = JsonConvert.DeserializeObject<Message>(createShortUrlResponse.Content);

            Assert.AreEqual("Short code added.", responseContent.Msg, "Short Code is not added.");

            var getShortUrlRequest = new RestRequest($"/api/urls/{_differentShortCode}", Method.Get);
            var getShortUrlResponse = restClient.Execute(getShortUrlRequest);

            Assert.That(getShortUrlResponse.Content.Contains(_differentShortCode), 
                        "Response does not contain original url.");
        }

        [Order(6)]
        [Test]
        public void TryToCreateNewShortUrlWithDifferentUrlAndSameShortCode()
        {
            var request = new RestRequest("/api/urls", Method.Post);

            var body = new { url = _differentUrl, shortCode = ActualShortCode };
            request.AddJsonBody(body);

            var response = restClient.Execute(request);

            Assert.That((int)response.StatusCode == 400, "Status code is different.");
            Assert.That(response.Content.Contains("Short code already exists!"), 
                        "Error message is different.");
        }

        [Order(7)]
        [Test]
        public void DeleteShortUrlByGivenShortCode()
        {
            var request = new RestRequest($"/api/urls/{_uniqueShortCode}", Method.Delete);

            var response = restClient.Execute(request);

            Assert.That((int)response.StatusCode == 200, "Expected status code is different.");

            var responseMessage = JsonConvert.DeserializeObject<ErrorMessage>(response.Content);

            Assert.That(responseMessage.Msg.Contains(_uniqueShortCode), "Deleted Short Code is different.");
        }

        [Order(8)]
        [Test]
        public void PostNewVisitsByGivenShortCode()
        {
            var getRequest = new RestRequest("/api/urls", Method.Get);

            var getResponse = restClient.Execute(getRequest);
            var listOfShortUrls = JsonConvert.DeserializeObject<List<UrlEntry>>(getResponse.Content);

            var shortUrl = listOfShortUrls.Where(u => u.ShortURL.Contains(ActualShortCode))
                                       .FirstOrDefault();
            int initiallyVisits = int.Parse(shortUrl.Visits);
            
            var newVisitRequest = new RestRequest($"/api/urls/visit/{ActualShortCode}", Method.Post);
            var newVisitResponse = restClient.Execute(newVisitRequest);

            var getShortUrlRequest = new RestRequest($"/api/urls/{ActualShortCode}", Method.Get);
            var getShortUrlResponse = restClient.Execute(getShortUrlRequest);
            var shortUrlContent = JsonConvert.DeserializeObject<UrlEntry>(getShortUrlResponse.Content);
            int actualVisits = int.Parse(shortUrlContent.Visits);

            Assert.That(initiallyVisits < actualVisits, "Expected visits are different.");
        }

        public static string GenerateUniqueUrl()
        {
            var result = new StringBuilder("https://shami");

            var random = new Random();
            int number = random.Next();
            result.Append(number);

            result.Append(".com");

            return result.ToString();
        }

        public static string GenerateUniqueShortCode()
        {
            var result = new StringBuilder("shami");

            var random = new Random();
            int number = random.Next();
            result.Append(number);

            return result.ToString();
        }
    }

}