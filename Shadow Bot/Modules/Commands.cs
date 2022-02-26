using Discord.Commands;
using MySql.Data.MySqlClient.Memcached;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Shadow_Bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        #region Help Command
        [Command("help")]
        public async Task Help()
        {
            string reply = "";
            var help = new List<string> { "help - you already know this one", "hello", "bye", "inspire", "bored", "zip {country} {zipcode}", "weather {city}" };
            foreach (var i in help)
            {
                reply += ("s!" + i + "\n");
            }
            await ReplyAsync("```" + "So far we only have: " + "\n\n" + reply + "\n" + "We are working to add more soon!" + "```");
        }
        #endregion

        #region Hello Command
        [Command("hello")]
        public async Task Intro()
        {
            var greetings = new List<string> { "Whats up?", "How you doing?", "Hi! How can I help you today?", "Ayo! What you need?", "Yello! Your wish is my Command", "Sorry! Busy today", "No replies for you" };
            Random rnd = new Random();
            int rand = rnd.Next(greetings.Count);
            await Context.Channel.SendMessageAsync(greetings[rand] + " " + Context.Message.Author.Mention);
        }
        #endregion

        #region Bye Command
        [Command("bye")]
        public async Task Exit()
        {
            var goodbye = new List<string> { "See you soon!", "See You Later Alligator!", "Bye Felicia!", "We are really going to miss trying to avoid you around here", "Thank god you leaving" };
            Random rnd = new Random();
            int rand = rnd.Next(goodbye.Count);
            await Context.Channel.SendMessageAsync(goodbye[rand] + " " + Context.Message.Author.Mention);
        }

        #endregion

        #region Inspire Command 
        [Command("inspire")]
        public async Task InspireQuotes()
        {
            try
            {
                HttpClient client = new HttpClient();
                string json = "https://zenquotes.io/api/random/[your_key]";
                Task<HttpResponseMessage> httpRequest = client.GetAsync(json);
                HttpResponseMessage httpResponse = httpRequest.Result;
                HttpStatusCode statusCode = httpResponse.StatusCode;
                HttpContent responseContent = httpResponse.Content;

                Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                string responseString = stringContentsTask.Result;
                var quoteObject = JsonConvert.DeserializeObject<quotes>(responseString.Substring(1, responseString.Length - 2));
                await ReplyAsync(quoteObject.q + " - " + quoteObject.a);

                //another way
                //var quoteObjectList = JsonConvert.DeserializeObject<List<quotes>>(responseString);
                //var quotesObject = quoteObjectList[0];
            }
            catch (Exception e)
            {
                await ReplyAsync(e.ToString());
            }
        }

        public class quotes
        {
            public string q { get; set; }
            public string a { get; set; }
            public string h { get; set; }
        }
        #endregion

        #region Bored Command
        [Command("bored")]
        public async Task BoredActivity()
        {
            try
            {
                HttpClient client = new HttpClient();
                string json = "http://www.boredapi.com/api/activity/";
                Task<HttpResponseMessage> httpRequest = client.GetAsync(json);
                HttpResponseMessage httpResponse = httpRequest.Result;
                HttpStatusCode statusCode = httpResponse.StatusCode;
                HttpContent responseContent = httpResponse.Content;

                Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                string responseString = stringContentsTask.Result;

                var data = JsonConvert.DeserializeObject<bored>(responseString);

                await Context.Channel.SendMessageAsync(data.activity + " " + Context.Message.Author.Mention);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.StackTrace);
            }
        }

        public class bored
        {
            public string activity { get; set; }
            public decimal accessibility { get; set; }
            public string education { get; set; }
            public int participants { get; set; }
            public decimal price { get; set; }
            public string key { get; set; }
        }
        #endregion

        #region Trivia Command
        [Command("trivia")]
        public async Task Trivia()
        {

            try
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage();
                request.RequestUri = new Uri("http://api.fungenerators.com/fact/random");
                request.Method = HttpMethod.Get;
                request.Headers.Add("X-Fungenerators-Api-Secret", "api_key");
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                var statusCode = response.StatusCode;

                //var data = JsonConvert.DeserializeObject<contents>(responseString);
                await ReplyAsync(responseString);

                //await Context.Channel.SendMessageAsync(data.facts + " " + Context.Message.Author.Mention);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.StackTrace);
            }
        }
        public class contents
        {
            public string facts { get; set; }
            public string id { get; set; }
            public string category { get; set; }
            public string subcategory { get; set; }
        }
        #endregion

        #region Zip Command
        [Command("zip")]
        public async Task Zip(string country, string zip)
        {

            try
            {
                HttpClient client = new HttpClient();
                string json = "https://api.zippopotam.us/" + country + "/" + zip;
                Task<HttpResponseMessage> httpRequest = client.GetAsync(json);
                HttpResponseMessage httpResponse = httpRequest.Result;
                HttpStatusCode statusCode = httpResponse.StatusCode;
                HttpContent responseContent = httpResponse.Content;

                Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                string responseString = stringContentsTask.Result;

                if (responseString != null)
                {
                    var data = JsonConvert.DeserializeObject<zipCode>(responseString);
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + "You requested location is: ");
                    await Context.Channel.SendMessageAsync("Place: " + data.places[0].place_name);
                    await Context.Channel.SendMessageAsync("State: " + data.places[0].state);
                    await Context.Channel.SendMessageAsync("Country: " + data.country);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + ", You're requested details are wrong!");
                }

            }
            catch (Exception e)
            {
                await ReplyAsync(e.StackTrace);
            }
        }
        public class zipCode
        {
            public string postcode { get; set; }
            public string country { get; set; }
            public string countryabbreviation { get; set; }
            public places[] places { get; set; }
        }
        public class places
        {
            [JsonProperty("place name")]
            public string place_name { get; set; }
            public string longitude { get; set; }
            public string state { get; set; }
            public string stateabbreviation { get; set; }
            public string latitude { get; set; }
        }

        #endregion

        #region Weather Command
        [Command("weather")]
        public async Task weather(string city)
        {
            string nextday = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                string json = "https://weatherdbi.herokuapp.com/data/weather/" + city;
                Task<HttpResponseMessage> httpRequest = client.GetAsync(json);
                HttpResponseMessage httpResponse = httpRequest.Result;
                HttpStatusCode statusCode = httpResponse.StatusCode;
                HttpContent responseContent = httpResponse.Content;

                Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                string responseString = stringContentsTask.Result;

                if (responseString != null)
                {
                    var data = JsonConvert.DeserializeObject<todayweather>(responseString);
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + "You requested location is: ");
                    await Context.Channel.SendMessageAsync("Place: " + data.region);
                    await Context.Channel.SendMessageAsync("Time: " + data.currentConditions.dayhour);
                    await Context.Channel.SendMessageAsync("Temperature: " + data.currentConditions.temp.c + " degrees");
                    await Context.Channel.SendMessageAsync("Comment: " + data.currentConditions.comment);
                    await Context.Channel.SendMessageAsync("Getting data for next days. Please wait....");
                    foreach (var i in data.next_days)
                    {
                        nextday += "Day: " + i.day + " Comment: " + i.comment + "\n";
                    }
                    await Context.Channel.SendMessageAsync("```Data for next few days for " + data.region + "\n" + nextday + "```");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + ", You're requested details are wrong!");
                }

            }
            catch (Exception e)
            {
                await ReplyAsync(e.ToString());
            }
        }
        public class todayweather
        {
            public string region { get; set; }
            public currentConditions currentConditions { get; set; }
            public List<nextdays> next_days { get; set; }
        }
        public class currentConditions 
        {
            public string dayhour { get; set; }
            public temp temp { get; set; }
            public string precip { get; set; }
            public string humidity { get; set; }
            public wind wind { get; set; }
            public string iconUrl { get; set; }
            public string comment { get; set; }
        }
        public class nextdays
        {
            public string day { get; set; }
            public string comment { get; set; }
            public temp max_temp { get; set; }
            public temp min_temp { get; set; }
            public string iconUrl { get; set; }
        }
        public class temp
        {
            public float c { get; set; }
            public float f { get; set; }
        }
        public class wind
        {
            public float km { get; set; }
            public float mile { get; set; }
        }
    }
    #endregion
}