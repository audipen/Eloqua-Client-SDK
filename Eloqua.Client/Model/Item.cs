using System;
using System.Linq;
using Newtonsoft.Json;

namespace Eloqua.Client.Model
{
    public abstract class Item
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
    }
}
