using System;
using System.Linq;
using Newtonsoft.Json;

namespace Eloqua.Client.Model
{
    public class Contact : Item
    {
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("businessPhone")]
        public string BusinessPhone { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }
    }
}
