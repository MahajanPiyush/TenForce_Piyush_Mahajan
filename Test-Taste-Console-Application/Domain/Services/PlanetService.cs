using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            var allPlanetsWithTheirMoons = new Collection<Planet>();

            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            //The JSON converter can return a null object. 
            if (results == null) return allPlanetsWithTheirMoons;

            //If the planet doesn't have any moons, then it isn't added to the collection.
            foreach (var planet in results.Bodies)
            {
                if(planet.Moons != null)
                {
                    var newMoonsCollection = new Collection<MoonDto>();
                    foreach (var moon in planet.Moons)
                    {
                        var moonResponse = _httpClientService.Client
                            .GetAsync(UriPath.GetMoonByIdQueryParameters + moon.URLId)
                            .Result;
                        var moonContent = moonResponse.Content.ReadAsStringAsync().Result;
                        newMoonsCollection.Add(JsonConvert.DeserializeObject<MoonDto>(moonContent));
                    }
                    planet.Moons = newMoonsCollection;

                }
                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Method to Calculate Avergae Moon Gravity of Planet
        /// </summary>
        /// <param name="Moons"></param>
        /// <returns>decimal</returns>
        public decimal GetAverageMoonGravityForPlanet(ICollection<Moon> Moons)
        {
            decimal avergaeOfMoonGravityOfPlanet = 0;
            decimal sumOfMoonGravityOfPlanet = 0;
            try
            {
                //Loop through all the moons of planet to get gravity for each moon
                foreach (var obj_moon in Moons)
                {
                    //calling api 
                    var response = _httpClientService.Client
                        .GetAsync(UriPath.GetAvergaeMoonGravity + obj_moon.Id)
                        .Result;

                    //If the status code isn't 200-299, then the function returns an empty collection.
                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                    }
                    else
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
                        var results = JsonConvert.DeserializeObject<PlanetsMoonobject>(content);

                        if (results != null)
                        {
                            //Suming up all the moon gravity of Planet
                            sumOfMoonGravityOfPlanet += results.gravity;
                        }
                    }


                }

                //Average is sum of gravity upon total moons
                avergaeOfMoonGravityOfPlanet = Math.Round((sumOfMoonGravityOfPlanet / Moons.Count()),4);
            }
            catch(Exception ex)
            {
                Logger.Instance.Warn($"{"Exception Occured in GetAverageMoonGravityForPlanet Method"}{ex.Message}");
                sumOfMoonGravityOfPlanet = 0;
            }
            return avergaeOfMoonGravityOfPlanet;
        }
    }
}
