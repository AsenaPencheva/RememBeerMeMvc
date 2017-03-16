﻿using System.Net;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.MvcClient.Areas.Admin.Controllers;
using RememBeer.Services.Contracts;
using RememBeer.Tests.Mvc.Controllers.Ninject;

namespace RememBeer.Tests.Mvc.Controllers.Admin.BreweriesControllerTests
{
    [TestFixture]
    public class DeleteBeer_Should : BreweriesControllerTestBase
    {
        [TestCase(-1)]
        [TestCase(1)]
        [TestCase(991)]
        public void Call_BreweryServiceDeleteBeerMethodOnceWithCorrectParams(int expectedId)
        {
            // Arrange
            var sut = this.Kernel.Get<BreweriesController>();
            var breweryService = this.Kernel.GetMock<IBreweryService>();

            // Act
            sut.DeleteBeer(expectedId);

            // Assert
            breweryService.Verify(s => s.DeleteBeer(expectedId), Times.Once);
        }

        [Test]
        public void Return_CorrectHttpStatusCodeResult()
        {
            // Arrange
            var sut = this.Kernel.Get<BreweriesController>();

            // Act
            var result = sut.DeleteBeer(It.IsAny<int>());

            // Assert
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
            StringAssert.Contains("has been deleted", result.StatusDescription);
        }
    }
}
