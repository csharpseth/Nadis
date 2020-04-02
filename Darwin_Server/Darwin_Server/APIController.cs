using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.Newtonsoft;

namespace Darwin_Server
{
    public class APIController
    {
        public static void UpdateServer()
        {
            string RestURL = "http://45.79.9.19:3000/";
            var client = new RestClient(RestURL);
            var request = new RestRequest("servers/registerserver", Method.POST, DataFormat.Json);
            request.AddJsonBody(NetworkConfig.Data);

            var response = client.Post(request);
        }
    }
}
