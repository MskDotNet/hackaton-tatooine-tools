using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Pathfinder;

namespace MachineheadTetsujin
{
    public class ApiClient
    {
        private readonly string _baseUrl;
        private string _session;

        public ApiClient(string baseUrl = "http://51.15.100.12:5000/raceapi/")
        {
            _baseUrl = baseUrl;
        }

        HttpClient GetClient()
        {
            var cl = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl)
            };
            if(_token != null)
                cl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return cl;
        }
        
        private string _token;

        public T Send<T>(string uri, object data = null, string method = null)
        {
            method = (method ?? (data != null ? "POST" : "GET")).ToUpper();
            using (var cl = GetClient())
            {
                var msg = new HttpRequestMessage(new HttpMethod(method), uri);
                if (data != null)
                    msg.Content = new StringContent(JsonConvert.SerializeObject(data,
                            new JsonSerializerSettings
                            {
                                Converters = {new StringEnumConverter()}
                            }), Encoding.UTF8,
                        "application/json");
                using (var r = cl.SendAsync(msg).Result)
                {
                    var resp = r.Content.ReadAsStringAsync().Result;
                    if (!r.IsSuccessStatusCode)
                        throw new Exception(resp);
                    return JsonConvert.DeserializeObject<T>(resp);
                }
            }
        }
        
        public void Login(string username, string password)
        {
            _token = Send<JObject>("Auth/Login", new {Login = username, Password = password})["Token"].ToString();
        }

        public void Start(string map)
        {
            var wtf = Send<ApiRaceStateDto>("race", new {Map = map}, "POST");
            _session = wtf.SessionId;
        }

        public ApiRaceStateDto GetRaceState()
        {
            return Send<ApiRaceStateDto>("race?sessionId=" + _session);
        }

        public void Turn(Direction direction, int acceleration)
        {
            Send<JObject>("race/" + _session, new
            {
                Direction = direction, Acceleration = acceleration
            }, "PUT");
        }

        public void Register(string username, string password)
        {
            _token =
                Send<JObject>("Auth/Register", new {Login = username, Password = password, team = "tatooin"})["Token"]
                    .ToString();
        }
    }
}