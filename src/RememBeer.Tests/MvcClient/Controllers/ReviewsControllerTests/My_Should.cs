﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

using AutoMapper;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.Models.Contracts;
using RememBeer.MvcClient.Controllers;
using RememBeer.MvcClient.Models.Reviews;
using RememBeer.Services.Contracts;
using RememBeer.Tests.MvcClient.Controllers.Ninject;

namespace RememBeer.Tests.MvcClient.Controllers.ReviewsControllerTests
{
    [TestFixture]
    public class My_Should : ReviewsControllerTestBase

    {
        private readonly string expectedUserId = Guid.NewGuid().ToString();

        [TestCase(0, 0, "")]
        [TestCase(1, 50, null)]
        [TestCase(500, 17, "asdjkh12kj3789y6as8791236545sd465as465dasas123423gyuyjuyio")]
        public void Call_GetReviewsForUserOnceWithCorrectParams(int page, int pageSize, string search)
        {
            // Arrange
            var expected = new List<IBeerReview>();
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);

            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetReviewsForUser(this.expectedUserId, page * pageSize, pageSize, search))
                         .Returns(expected);

            // Act
            sut.My(page, pageSize, search);

            // Assert
            reviewService.Verify(s => s.GetReviewsForUser(this.expectedUserId, page * pageSize, pageSize, search), Times.Once);
        }

        [Test]
        public void Call_MapMethodOnceWithCorrectParams()
        {
            // Arrange
            var expected = new List<IBeerReview>();
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var mapper = this.MockingKernel.GetMock<IMapper>();
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.GetReviewsForUser(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                         .Returns(expected);

            // Act
            sut.My();

            // Assert
            mapper.Verify(m => m.Map<IEnumerable<IBeerReview>, IEnumerable<SingleReviewViewModel>>(expected),
                          Times.Once);
        }

        [Test]
        public void Call_CountUserReviewsrOnceWithCorrectParams()
        {
            // Arrange
            var expectedSearch = Guid.NewGuid().ToString();
            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();

            // Act
            sut.My(searchPattern: expectedSearch);

            // Assert
            reviewService.Verify(s => s.CountUserReviews(this.expectedUserId, expectedSearch), Times.Once);
        }

        [Test]
        public void ReturnPartialViewWithCorrectViewModel_WhenRequestIsAjax()
        {
            // Arrange
            var expectedPageSize = 50;
            var expectedPage = 11;

            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var expectedReviews = new List<SingleReviewViewModel>() { new SingleReviewViewModel() };
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IEnumerable<IBeerReview>, IEnumerable<SingleReviewViewModel>>(It.IsAny<IEnumerable<IBeerReview>>()))
                  .Returns(expectedReviews);
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.CountUserReviews(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(expectedReviews.Count);

            // Act
            var result = sut.My(expectedPage, expectedPageSize) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
            var actualViewModel = result.Model as PaginatedReviewsViewModel;
            Assert.IsNotNull(actualViewModel);
            Assert.AreEqual("_ReviewList", result.ViewName);
            Assert.AreEqual(expectedPage, actualViewModel.CurrentPage);
            Assert.AreEqual(expectedPageSize, actualViewModel.PageSize);
            Assert.AreEqual(expectedReviews.Count, actualViewModel.TotalCount);
        }

        [Test]
        public void ReturnViewWithCorrectViewModel_WhenRequestIsNotAjax()
        {
            // Arrange
            var expectedPageSize = 50;
            var expectedPage = 11;

            var sut = this.MockingKernel.Get<ReviewsController>(RegularContextName);
            var expectedReviews = new List<SingleReviewViewModel>() { new SingleReviewViewModel() };
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IEnumerable<IBeerReview>, IEnumerable<SingleReviewViewModel>>(It.IsAny<IEnumerable<IBeerReview>>()))
                  .Returns(expectedReviews);
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.CountUserReviews(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(expectedReviews.Count);

            // Act
            var result = sut.My(expectedPage, expectedPageSize) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var actualViewModel = result.Model as PaginatedReviewsViewModel;
            Assert.IsNotNull(actualViewModel);
            Assert.AreEqual(string.Empty, result.ViewName);
            Assert.AreEqual(expectedPage, actualViewModel.CurrentPage);
            Assert.AreEqual(expectedPageSize, actualViewModel.PageSize);
            Assert.AreEqual(expectedReviews.Count, actualViewModel.TotalCount);
        }

        [Test]
        public void SetViewModelsIsEditPropertyToTrue_WhenAjax()
        {
            // Arrange
            var expectedPageSize = 50;
            var expectedPage = 11;

            var sut = this.MockingKernel.Get<ReviewsController>(AjaxContextName);
            var expectedReviews = new List<SingleReviewViewModel>() { new SingleReviewViewModel(), new SingleReviewViewModel() };
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IEnumerable<IBeerReview>, IEnumerable<SingleReviewViewModel>>(It.IsAny<IEnumerable<IBeerReview>>()))
                  .Returns(expectedReviews);
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.CountUserReviews(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(expectedReviews.Count);

            // Act
            var result = sut.My(expectedPage, expectedPageSize) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
            var actualViewModel = result.Model as PaginatedReviewsViewModel;
            Assert.IsNotNull(actualViewModel);
            foreach (var actual in actualViewModel.Items)
            {
                Assert.IsTrue(actual.IsEdit);
            }
        }

        [Test]
        public void SetViewModelsIsEditPropertyToTrue_WhenRegularRequest()
        {
            // Arrange
            var expectedPageSize = 50;
            var expectedPage = 11;

            var sut = this.MockingKernel.Get<ReviewsController>(RegularContextName);
            var expectedReviews = new List<SingleReviewViewModel>() { new SingleReviewViewModel(), new SingleReviewViewModel() };
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IEnumerable<IBeerReview>, IEnumerable<SingleReviewViewModel>>(It.IsAny<IEnumerable<IBeerReview>>()))
                  .Returns(expectedReviews);
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(r => r.CountUserReviews(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(expectedReviews.Count);

            // Act
            var result = sut.My(expectedPage, expectedPageSize) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var actualViewModel = result.Model as PaginatedReviewsViewModel;
            Assert.IsNotNull(actualViewModel);
            foreach (var actual in actualViewModel.Items)
            {
                Assert.IsTrue(actual.IsEdit);
            }
        }

        public override void Init()
        {
            this.MockingKernel.Bind<HttpContextBase>()
                .ToMethod(ctx =>
                          {
                              var request = new Mock<HttpRequestBase>();
                              request.SetupGet(x => x.Headers).Returns(
                                                                       new System.Net.WebHeaderCollection
                                                                       {
                                                                           { "X-Requested-With", "XMLHttpRequest" }
                                                                       });

                              var identity = new Mock<ClaimsIdentity>();
                              identity.Setup(i => i.FindFirst(It.IsAny<string>()))
                                      .Returns(new Claim("sa", this.expectedUserId));
                              var context = new Mock<HttpContextBase>();
                              context.SetupGet(x => x.Request).Returns(request.Object);
                              context.SetupGet(x => x.User.Identity).Returns(identity.Object);

                              return context.Object;
                          })
                .InSingletonScope()
                .Named(AjaxContextName);

            this.MockingKernel.Bind<HttpContextBase>()
                .ToMethod(ctx =>
                          {
                              var request = new Mock<HttpRequestBase>();
                              request.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection());

                              var identity = new Mock<ClaimsIdentity>();
                              identity.Setup(i => i.FindFirst(It.IsAny<string>()))
                                      .Returns(new Claim("sa", this.expectedUserId));
                              var context = new Mock<HttpContextBase>();
                              context.SetupGet(x => x.Request).Returns(request.Object);
                              context.SetupGet(x => x.User.Identity).Returns(identity.Object);

                              return context.Object;
                          })
                .InSingletonScope()
                .Named(RegularContextName);

            base.Init();
        }
    }
}
