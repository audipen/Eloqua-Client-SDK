using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eloqua.Client
{
    public class EloquaContext
    {
        private readonly string userName;
        private readonly string password;
        private readonly string companyId;
        private string authenticationToken;
        private List<Contact> objectToAdd = new List<Contact>();
        private List<Contact> objectsToUpdate = new List<Contact>();
        private List<string> objectsToDelete = new List<string>();

        private string restApiUrl;

        public EloquaContext(string userName, string password, string companyId)
        {
            this.userName = userName;
            this.password = password;
            this.companyId = companyId;
        }

        public void Update(Contact contact)
        {
            this.objectsToUpdate.Add(contact);
        }

        public void Delete(string idInsertedInSource)
        {
            this.objectsToDelete.Add(idInsertedInSource);
        }

        private void GetBaseUrl()
        {
            if (this.restApiUrl == null)
            {
                this.authenticationToken = GetAuthenticationToken();
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://login.eloqua.com/");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", this.authenticationToken);
                    var response = client.GetAsync("id").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var resContent = response.Content.ReadAsStringAsync().Result;
                        var jObject = JObject.Parse(resContent);
                        string restApiUrl = jObject["urls"]["apis"]["rest"]["standard"].Value<string>();
                        this.restApiUrl = restApiUrl.Replace("{version}", "1.0");
                    }
                }
            }
        }

        public void SaveChanges()
        {
            GetBaseUrl();
            using (HttpClient contactClient = new HttpClient())
            {
                contactClient.BaseAddress = new Uri(restApiUrl);
                contactClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", this.authenticationToken);
                //contactClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
                for (int i = 0; i < this.objectToAdd.Count; i++)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(this.objectToAdd[i]), Encoding.UTF8, "application/json");
                    var contactPostResponse = contactClient.PostAsync("data/contact", content).Result;
                    if (contactPostResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(contactPostResponse.Content.ReadAsStringAsync().Result);
                    }

                    var contact = JsonConvert.DeserializeAnonymousType(contactPostResponse.Content.ReadAsStringAsync().Result, new Contact());
                    this.objectToAdd[i].Id = contact.Id;
                }
                this.objectToAdd.Clear();

                for (int i = 0; i < this.objectsToUpdate.Count; i++)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(this.objectsToUpdate[i]), Encoding.UTF8, "application/json");
                    var contactPostResponse = contactClient.PutAsync("data/contact/" + this.objectsToUpdate[i].Id, content).Result;
                    if (contactPostResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(contactPostResponse.Content.ReadAsStringAsync().Result);
                    }

                    //var contact = JsonConvert.DeserializeAnonymousType(contactPostResponse.Content.ReadAsStringAsync().Result, new Contact());
                    //this.objectToAdd[i].Id = contact.Id;
                }
                this.objectsToUpdate.Clear();

                for (int i = 0; i < this.objectsToDelete.Count; i++)
                {
                    //var content = new StringContent(JsonConvert.SerializeObject(this.objectToAdd[i]), Encoding.UTF8, "application/json");
                    var contactPostResponse = contactClient.DeleteAsync("data/contact/" + this.objectsToDelete[i]).Result;
                    if (contactPostResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(contactPostResponse.Content.ReadAsStringAsync().Result);
                    }
                }
                this.objectsToDelete.Clear();
            }
        }

        public void Add(Contact contact)
        {
            this.objectToAdd.Add(contact);
        }

        public List<object> GetContacts()
        {
            GetBaseUrl();
            using (HttpClient contactClient = new HttpClient())
            {
                contactClient.BaseAddress = new Uri(restApiUrl);
                contactClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", this.authenticationToken);
                var contactGetResponse = contactClient.GetAsync("data/contacts").Result;
                if (contactGetResponse.IsSuccessStatusCode)
                {
                    var contact = new { elements = new[] { new { name = "", emailAddress = "" } } };
                    var contacts = JsonConvert.DeserializeAnonymousType(contactGetResponse.Content.ReadAsStringAsync().Result, contact);

                    return contacts.elements.Cast<object>().ToList();
                }
            }

            return new List<object>();
        }

        private string GetAuthenticationToken()
        {
            var token = string.Format("{0}\\{1}:{2}", this.companyId, this.userName, this.password);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

    }

    public class Contact
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("businessPhone")]
        public string BusinessPhone { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }
    }
}
