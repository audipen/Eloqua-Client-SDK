using System;
using System.Collections.Generic;
using System.Configuration;
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

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            User = ConfigurationManager.AppSettings[UserName];
            Passwrd = ConfigurationManager.AppSettings[Password];
            Company = ConfigurationManager.AppSettings[CompanyId];
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
    }
}
