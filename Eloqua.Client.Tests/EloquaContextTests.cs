using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Eloqua.Client.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eloqua.Client.Tests
{
    [TestClass]
    public class EloquaContextTests
    {
        private const string UserName = "UserName";
        private const string Password = "Password";
        private const string CompanyId = "CompanyId";

        private static string User;
        private static string Passwrd;
        private static string Company;

        EloquaContext context;
        readonly List<Item> itemsAdded = new List<Item>();
        private static int InitialItemCount;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            User = ConfigurationManager.AppSettings[UserName];
            Passwrd = ConfigurationManager.AppSettings[Password];
            Company = ConfigurationManager.AppSettings[CompanyId];
            
            var context = new EloquaContext(User, Passwrd, Company);
            InitialItemCount = context.GetAll<Contact>().Count();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            context = new EloquaContext(User, Passwrd, Company);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var item in this.itemsAdded)
            {
                context.Delete(item.Id);
            }
            context.SaveChanges();
            this.itemsAdded.Clear();
        }

        [TestMethod]
        public void Add()
        {
            Contact contact = new Contact { FirstName = "Frodo", EmailAddress = "frodo@test.com", BusinessPhone = "666-666" };
            context.Add(contact);
            context.SaveChanges();
            
            this.itemsAdded.Add(contact);

            //Id should be assigned on successful creation of contact
            Assert.IsFalse(string.IsNullOrEmpty(contact.Id));


        }

        [TestMethod]
        public void GetAllContacts()
        {
            List<Contact> contacts = new List<Contact>();
            for (int i = 0; i < 10; i++)
            {
                Contact contact = new Contact { FirstName = "Frodo", EmailAddress = string.Format("frodo{0}@test.com",i), BusinessPhone = "666-666" };
                context.Add(contact);
                contacts.Add(contact);
            }
            
            context.SaveChanges();
            this.itemsAdded.AddRange(contacts);

            var readContacts = context.GetAll<Contact>();

            Assert.AreEqual(10 + InitialItemCount, readContacts.Count());
            readContacts = readContacts.Where(c => string.IsNullOrEmpty(c.FirstName) == false && c.FirstName.Equals("Frodo")).ToList();
            Assert.AreEqual(10, readContacts.Count());
            Assert.IsFalse(readContacts.Any(i => string.IsNullOrEmpty(i.Id)));
            Assert.IsTrue(readContacts.All(c => c.BusinessPhone.Equals("666-666")));
            Assert.IsTrue(readContacts.All(c => c.FirstName.Equals("Frodo")));
        }
    }
}
