﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.MvcClient.Areas.Admin.Controllers;
using RememBeer.Services.Contracts;
using RememBeer.Tests.MvcClient.Controllers.Ninject;

namespace RememBeer.Tests.MvcClient.Controllers.Admin.UserControllerTests
{
    [TestFixture]
    public class RemoveAdmin_Should : UsersControllerTestBase
    {
        [TestCase("")]
        [TestCase(null)]
        [TestCase("user001sdkjasdjklasd789@#&*(23789123789sd")]
        public async Task Call_UserServiceRemoveAdminAsyncOnceWithCorrectParams(string userId)
        {
            // Arrange
            var sut = this.MockingKernel.Get<UsersController>();
            var userService = this.MockingKernel.GetMock<IUserService>();

            // Act
            await sut.RemoveAdmin(userId);

            // Assert
            userService.Verify(s => s.RemoveAdminAsync(userId), Times.Once);
        }

        [Test]
        public async Task Return_CorrectRedirectToActionResult()
        {
            // Arrange
            const int expectedPageSize = 17;
            const string expectedAction = "Index";
            const int expectedPage = 991;
            var expectedSearch = Guid.NewGuid().ToString();
            var sut = this.MockingKernel.Get<UsersController>();

            // Act
            var result = await sut.RemoveAdmin("sa", expectedPage, expectedPageSize, expectedSearch) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual((string)result.RouteValues["action"], expectedAction);
            Assert.AreEqual((int)result.RouteValues["page"], expectedPage);
            Assert.AreEqual((int)result.RouteValues["pageSize"], expectedPageSize);
            Assert.AreEqual((string)result.RouteValues["searchPattern"], expectedSearch);
        }
    }
}
