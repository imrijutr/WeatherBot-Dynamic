// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using OpenWeatherMap;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    // This bot will respond to the user's input with an Adaptive Card.
    // Adaptive Cards are a way for developers to exchange card content
    // in a common and consistent way. A simple open card format enables
    // an ecosystem of shared tooling, seamless integration between apps,
    // and native cross-platform performance on any device.
    // For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    // This is a Transient lifetime service. Transient lifetime services are created
    // each time they're requested. For each Activity received, a new instance of this
    // class is created. Objects that are expensive to construct, or have a lifetime
    // beyond the single turn, should be carefully managed.

    public class AdaptiveCardsBot : ActivityHandler 
    {
        private const string WelcomeText = @"Ask me about weather update. Please send your location !";

        // This array contains the file location of our adaptive cards
        private readonly string[] _cards =
        {
            Path.Combine(".", "Resources", "LargeWeatherCard.json"),
        };

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await SendWelcomeMessageAsync(turnContext, cancellationToken);
        }
        // protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        // {
        //     Random r = new Random();
        //     var cardAttachment = CreateAdaptiveCardAttachment(_cards[r.Next(_cards.Length)]);

        //     turnContext.Activity.Attachments = new List<Attachment>() { cardAttachment };
        //     await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        //     await turnContext.SendActivityAsync(MessageFactory.Text("Please enter another city name"), cancellationToken);
        // }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to weatherBot. {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }
        private static JObject readFileforUpdate_jobj(string filepath)
        {
            var json = File.ReadAllText(filepath);
            var jobj = JsonConvert.DeserializeObject(json);
            JObject Jobj_card = JObject.FromObject(jobj) as JObject;
            return Jobj_card;
        }
        private static Attachment UpdateAdaptivecardAttachment(JObject updateAttch)
        {
            
            var adaptiveCardAttch = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(updateAttch.ToString()),
            };
            return adaptiveCardAttch;
        }
        public static string ImageToBase64(string filePath)
        {
            Byte[] bytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(bytes);
            return "data:image/jpg;base64," + base64String;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var client = new OpenWeatherMapClient("bcf5adf0f7e061d9d1c47e9b12fcffde");
            var CloudImage = "http://messagecardplayground.azurewebsites.net/assets/Mostly%20Cloudy-Square.png";
            var rainImage  = "https://raw.githubusercontent.com/imrijutr/WeatherBot-Dynamic/master/Resources/rain.png";
            var stormImage = "https://raw.githubusercontent.com/imrijutr/WeatherBot-Dynamic/master/Resources/storm.png";
            var sunImage = "https://raw.githubusercontent.com/imrijutr/WeatherBot-Dynamic/master/Resources/sun.png";
            var currentWeather = await client.CurrentWeather.GetByName(turnContext.Activity.Text);
            var search =await client.Search.GetByName("Chennai");
            var forcast  = await client.Forecast.GetByName("Chennai");
            var curtTemp = currentWeather.Temperature.Value - 273.15;
            // var MaxTemp  = currentWeather.Temperature.Max -273.15;
            // var MinTemp  = currentWeather.Temperature.Min -273.15;
            var updateCard = readFileforUpdate_jobj(_cards[0]);
            JToken cityName = updateCard.SelectToken("body[0].columns[1].items[2].text");
            JToken tdyDate = updateCard.SelectToken("body[0].columns[1].items[0].text");
            JToken curTemp = updateCard.SelectToken("body[0].columns[1].items[1].text");
            // JToken maxTem = updateCard.SelectToken("body[2].columns[3].items[0].text");
            // JToken minTem = updateCard.SelectToken("body[2].columns[3].items[1].text");
            JToken weatherImageUrl = updateCard.SelectToken("body[0].columns[0].items[0].url");

 
            cityName.Replace(currentWeather.City.Name);
            curTemp.Replace(curtTemp.ToString("N0\u00B0C"));
            tdyDate.Replace(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            // maxTem.Replace("Max" +" "+MaxTemp.ToString("N0"));
            // minTem.Replace("Min" + " "+MinTemp.ToString("N0"));
            var n = currentWeather.Clouds.Name;
           
            if(n=="overcast clouds")
            {
                weatherImageUrl.Replace(rainImage);
            }
            else if (n.Contains("clouds"))
            {
                weatherImageUrl.Replace(CloudImage);
            }
            else if (n.Contains("sky"))
            {
                weatherImageUrl.Replace(sunImage);
            }
            else if (n.Contains("rai"))
            {
             weatherImageUrl.Replace(rainImage);
            }
            else if(n.Contains("storm") || n.Contains("thunder"))
            {
             weatherImageUrl.Replace(stormImage);
            }           

            var updateWeatherTem = UpdateAdaptivecardAttachment(updateCard);
          
            await turnContext.SendActivityAsync(MessageFactory.Attachment(updateWeatherTem), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text("Please enter another city name"));
            
        }
        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}






