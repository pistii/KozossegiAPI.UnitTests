
using KozoskodoAPI.Controllers;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KozossegiAPI.UnitTests.Controllers
{
    internal class PersonalControllerTests
    {
        public Mock<IPersonalRepository> personalRepository;
        public personalController controller;
        public IQueryable<Personal> GetPersonals()
        {
            List<Personal> personals = new();

            for (int i = 1; i <= 100; i++)
            {
                var person = new Personal()
                {
                    id = i,
                    firstName = "John",
                    lastName = "Doe",
                    PlaceOfResidence = "",
                };
                personals.Add(person);
            }
            return personals.AsQueryable();
        }

        [SetUp]
        public void Setup()
        {
            personalRepository = new();
            controller = new(personalRepository.Object);
        }

        [Test]
        public async Task GetAll_ReturnPersonsAsFilteredAndPaginated()
        {
            var people = GetPersonals();
            personalRepository.Setup(repo => repo.FilterPersons(It.IsAny<int>())).Returns(people);
            var result = await controller.GetAll(1);
            
            Assert.That(result, Is.InstanceOf<ContentDto<Personal>>());
            personalRepository.VerifyAll();
        }

        [Test]
        public async Task Get_ShouldReturnUserById()
        {
            var person = GetPersonals().FirstOrDefault(p => p.id == 1);
            personalRepository.Setup(repo => repo.GetByIdAsync<Personal>(It.IsAny<int>())).ReturnsAsync(person);

            var result = await controller.Get(1);
            Assert.IsInstanceOf<ActionResult<Personal>>(result);
            Assert.That(result.Result, Is.Not.Null);
        }
    }
}
