
using System;
using System.Net;
using Newtonsoft.Json;
using RestSharp;


namespace Api
{
    internal sealed class ApiHandler
    {
        private RestClient _client = new RestClient("http://192.168.132.209/");
        public string Login(LoginDto loginDto)
        {
            this._client.Timeout = -1;
            RestRequest request = new RestRequest("/api/auth/login",Method.POST);
            var body = JsonConvert.SerializeObject(loginDto);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", body,  ParameterType.RequestBody);
            IRestResponse response = this._client.Execute(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            } 
            return response.Content;
        }

        public void Register(RegisterDto registerDto)
        {
            this._client.Timeout = -1;
            RestRequest request = new RestRequest("/api/auth/register",Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(registerDto);
            request.AddParameter("application/json", body,  ParameterType.RequestBody);
            IRestResponse response = this._client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new UnauthorizedAccessException();
            } 
        }
        
    }
}