﻿using System;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

using AutoMapper;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.Common.Constants;
using RememBeer.Data.Repositories;
using RememBeer.Models.Contracts;
using RememBeer.MvcClient.Controllers;
using RememBeer.MvcClient.Filters;
using RememBeer.MvcClient.Models.Reviews;
using RememBeer.Services.Contracts;
using RememBeer.Tests.MvcClient.Controllers.Ninject;
using RememBeer.Tests.Utils;
using RememBeer.Tests.Utils.TestExtensions;

namespace RememBeer.Tests.MvcClient.Controllers.ReviewsControllerTests
{
    [TestFixture]
    public class Index_Put_Should : ReviewsControllerTestBase
    {
        private readonly string expectedUserId = Guid.NewGuid().ToString();

        [Test]
        public void HaveValidateAntiForgeryTokenAttribute()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();

            // Act
            var hasAttribute = AttributeTester.MethodHasAttribute(() => sut.Index(default(EditReviewBindingModel)), typeof(ValidateAntiForgeryTokenAttribute));

            // Assert
            Assert.IsTrue(hasAttribute);
        }

        [Test]
        public void HaveAjaxOnlyAttribute()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();

            // Act
            var hasAttribute = AttributeTester.MethodHasAttribute(() => sut.Index(default(EditReviewBindingModel)), typeof(AjaxOnlyAttribute));

            // Assert
            Assert.IsTrue(hasAttribute);
        }

        [Test]
        public void HaveHttpPutAttribute()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();

            // Act
            var hasAttribute = AttributeTester.MethodHasAttribute(() => sut.Index(default(EditReviewBindingModel)), typeof(HttpPutAttribute));

            // Assert
            Assert.IsTrue(hasAttribute);
        }

        [Test]
        public void Return_CorrectHttpStatusCodeResult_WhenModelValidationFails()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();
            var invalidModel = new EditReviewBindingModel();
            sut.InvalidateViewModel();

            // Act
            var result = sut.Index(invalidModel) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            StringAssert.Contains("validation failed", result.StatusDescription);
        }

        [Test]
        public void Call_ReviewServiceGetByIdMethodOnceWithCorrectParams()
        {
            // Arrange
            var expectedId = 10;
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel()
                               {
                                   Id = expectedId
                               };
            var beerReview = new Mock<IBeerReview>();

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(expectedId))
                         .Returns(beerReview.Object);
            // Act
            sut.Index(bindingModel);

            // Assert
            reviewService.Verify(s => s.GetById(expectedId), Times.Once);
        }

        [Test]
        public void Return_CorrectHttpStatusCodeResult_WhenUserIsNotTheOwnerOfTheFoundReviewAndIsNotAdmin()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            // Act
            var result = sut.Index(bindingModel) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, result.StatusCode);
            StringAssert.Contains("edit other people", result.StatusDescription);
        }

        [Test]
        public void Call_IMapperMapMethodOnceWithCorrectParams_WhenUserIsTheOwnerOfTheFoundReview()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();
            beerReview.Setup(r => r.ApplicationUserId)
                      .Returns(this.expectedUserId);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            reviewService.Setup(s => s.UpdateReview(It.IsAny<IBeerReview>()))
                         .Returns(new Mock<IDataModifiedResult>().Object);

            var mapper = this.MockingKernel.GetMock<IMapper>();
            // Act
            sut.Index(bindingModel);

            // Assert
            mapper.Verify(m => m.Map(bindingModel, beerReview.Object), Times.Once);
        }

        [Test]
        public void Call_ReviewServiceUpdateReviewMethodOnceWithCorrectParams_WhenUserIsTheOwnerOfTheFoundReview()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();
            beerReview.Setup(r => r.ApplicationUserId)
                      .Returns(this.expectedUserId);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            reviewService.Setup(s => s.UpdateReview(It.IsAny<IBeerReview>()))
                         .Returns(new Mock<IDataModifiedResult>().Object);
            // Act
            sut.Index(bindingModel);

            // Assert
            reviewService.Verify(s => s.UpdateReview(beerReview.Object), Times.Once);
        }

        [Test]
        public void Return_CorrectHttpStatusCodeResult_WhenReviewUpdateFails()
        {
            // Arrange
            var expectedErrors = new[] { "error1", "error2" };
            var updateResult = new Mock<IDataModifiedResult>();
            updateResult.Setup(r => r.Errors).Returns(expectedErrors);

            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();
            beerReview.Setup(r => r.ApplicationUserId)
                      .Returns(this.expectedUserId);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            reviewService.Setup(s => s.UpdateReview(It.IsAny<IBeerReview>()))
                         .Returns(updateResult.Object);
            // Act
            var result = sut.Index(bindingModel) as HttpStatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            StringAssert.Contains("error1, error2", result.StatusDescription);
        }

        [Test]
        public void Call_IMapperMapMethodOnceWithCorrectParams_WhenReviewUpdateIsSuccessful()
        {
            // Arrange
            var updateResult = new Mock<IDataModifiedResult>();
            updateResult.Setup(r => r.Successful).Returns(true);

            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();
            beerReview.Setup(r => r.ApplicationUserId)
                      .Returns(this.expectedUserId);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            reviewService.Setup(s => s.UpdateReview(It.IsAny<IBeerReview>()))
                         .Returns(updateResult.Object);

            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IBeerReview, SingleReviewViewModel>(beerReview.Object))
                  .Returns(new SingleReviewViewModel());

            // Act
            sut.Index(bindingModel);

            // Assert
            mapper.Verify(m => m.Map<IBeerReview, SingleReviewViewModel>(beerReview.Object), Times.Once);
        }

        [Test]
        public void Return_CorrectPartialViewWithCorrectParams_WhenUpdateIsSuccessful()
        {
            // Arrange
            var updateResult = new Mock<IDataModifiedResult>();
            updateResult.Setup(r => r.Successful).Returns(true);

            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var bindingModel = new EditReviewBindingModel();
            var beerReview = new Mock<IBeerReview>();
            beerReview.Setup(r => r.ApplicationUserId)
                      .Returns(this.expectedUserId);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            reviewService.Setup(s => s.UpdateReview(It.IsAny<IBeerReview>()))
                         .Returns(updateResult.Object);

            var expectedViewModel = new SingleReviewViewModel();
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IBeerReview, SingleReviewViewModel>(beerReview.Object))
                  .Returns(expectedViewModel);

            // Act
            var result = sut.Index(bindingModel) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("_SingleReview", result.ViewName);
            var actualViewModel = result.Model as SingleReviewViewModel;
            Assert.IsNotNull(actualViewModel);
            Assert.AreSame(expectedViewModel, actualViewModel);
        }

        public override void Init()
        {
            this.MockingKernel.Bind<HttpContextBase>()
                .ToMethod(ctx =>
                          {
                              var request = new Mock<HttpRequestBase>();
                              request.SetupGet(x => x.Headers).Returns(
                                                                       new WebHeaderCollection
                                                                       {
                                                                           { "X-Requested-With", "XMLHttpRequest" }
                                                                       });
                              var identity = new Mock<ClaimsIdentity>();
                              identity.Setup(i => i.FindFirst(It.IsAny<string>()))
                                      .Returns(new Claim("sa", this.expectedUserId));

                              var mockedUser = new Mock<IPrincipal>();
                              mockedUser.Setup(u => u.Identity).Returns(identity.Object);
                              mockedUser.Setup(u => u.IsInRole(Constants.AdminRole))
                                        .Returns(false);
                              var context = new Mock<HttpContextBase>();
                              context.SetupGet(x => x.Request).Returns(request.Object);
                              context.SetupGet(x => x.User).Returns(mockedUser.Object);

                              return context.Object;
                          })
                .InSingletonScope()
                .Named(AjaxContextName);

            base.Init();
        }
    }
}
