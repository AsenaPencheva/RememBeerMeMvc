﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.Models;
using RememBeer.Models.Identity.Contracts;
using RememBeer.MvcClient.Controllers;
using RememBeer.MvcClient.Models.AccountModels;
using RememBeer.Tests.MvcClient.Controllers.Ninject;
using RememBeer.Tests.Utils;
using RememBeer.Tests.Utils.TestExtensions;

namespace RememBeer.Tests.MvcClient.Controllers.AccountControllerTests
{
    [TestFixture]
    public class Register_Post_Should : AccountControllerTestBase
    {
        [TestCase(typeof(AllowAnonymousAttribute))]
        [TestCase(typeof(HttpPostAttribute))]
        [TestCase(typeof(ValidateAntiForgeryTokenAttribute))]
        public void Have_RequiredAttributes(Type attrType)
        {
            // Arrange
            var sut = this.Kernel.Get<AccountController>();

            // Act
            var hasAttribute = AttributeTester.MethodHasAttribute(() => sut.Register(default(RegisterViewModel)), attrType);

            // Assert
            Assert.IsTrue(hasAttribute);
        }

        [Test]
        public async Task Return_CorrectView_WhenModelStateIsInvalid()
        {
            // Arrange
            var sut = this.Kernel.Get<AccountController>();
            var expected = new RegisterViewModel();

            // Act
            sut.ValidateViewModel(expected);
            var result = await sut.Register(expected) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.ViewName);
            var actual = result.Model as RegisterViewModel;
            Assert.AreSame(expected, actual);
        }

        [Test]
        public async Task Call_UserManagerCreateAsyncMethodOnceWithCorrectParams_WhenModelIsValid()
        {
            // Arrange
            const string expectedEmail = "asdkdaskjhasjklhaskld@asd.dsa";
            const string expectedPass = "asdkdaskjhasjklha5646sk4ld@asd.dsa";
            var sut = this.Kernel.Get<AccountController>();
            var expected = new RegisterViewModel()
                           {
                               Email = expectedEmail,
                               Password = expectedPass
                           };
            var userManager = this.Kernel.GetMock<IApplicationUserManager>();
            userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                       .Returns(Task.FromResult(IdentityResult.Failed("")));
            // Act
            await sut.Register(expected);

            // Assert
            userManager.Verify(
                               m => m.CreateAsync(
                                                  It.Is<ApplicationUser>(u => u.UserName == expectedEmail
                                                                              && u.Email == expectedEmail)
                                                  , expectedPass),
                               Times.Once);
        }

        [Test]
        public async Task Call_SignInManagerSignInAsynccMethodOnceWithCorrectParams_WhenResultSucceeds()
        {
            // Arrange
            const string expectedEmail = "asdkdaskjhasjklhaskld@asd.dsa";
            const string expectedPass = "asdkdaskjhasjklha5646sk4ld@asd.dsa";
            var sut = this.Kernel.Get<AccountController>();
            var expected = new RegisterViewModel()
            {
                Email = expectedEmail,
                Password = expectedPass
            };

            var userManager = this.Kernel.GetMock<IApplicationUserManager>();
            userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                       .Returns(Task.FromResult(IdentityResult.Success));
            var signInManager = this.Kernel.GetMock<IApplicationSignInManager>();

            // Act
            await sut.Register(expected);

            // Assert
            signInManager.Verify(
                               m => m.SignInAsync(
                                                  It.Is<ApplicationUser>(u => u.UserName == expectedEmail
                                                                              && u.Email == expectedEmail)
                                                  , false , false),
                               Times.Once);
        }

        [Test]
        public async Task Return_CorrectRedirectResult_WhenResultSucceeds()
        {
            // Arrange
            const string expectedEmail = "asdkdaskjhasjklhaskld@asd.dsa";
            const string expectedPass = "asdkdaskjhasjklha5646sk4ld@asd.dsa";
            var sut = this.Kernel.Get<AccountController>();
            var expected = new RegisterViewModel()
            {
                Email = expectedEmail,
                Password = expectedPass
            };

            var userManager = this.Kernel.GetMock<IApplicationUserManager>();
            userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                       .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await sut.Register(expected) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Home", result.RouteValues["controller"]);
        }
    }
}