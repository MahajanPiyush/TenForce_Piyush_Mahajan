using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    public class PlanetDto
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }
        public ICollection<MoonDto> Moons { get; set; }
    }


    public class PlanetsMoonobject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string englishName { get; set; }
        public bool isPlanet { get; set; }
        public decimal gravity { get; set; }
    }

   

}