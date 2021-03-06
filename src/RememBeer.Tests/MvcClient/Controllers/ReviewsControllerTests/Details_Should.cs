﻿using System.Web.Mvc;

using AutoMapper;

using Moq;

using Ninject;

using NUnit.Framework;

using RememBeer.Models.Contracts;
using RememBeer.MvcClient.Controllers;
using RememBeer.MvcClient.Models.Reviews;
using RememBeer.Services.Contracts;
using RememBeer.Tests.MvcClient.Controllers.Ninject;
using RememBeer.Tests.Utils;

namespace RememBeer.Tests.MvcClient.Controllers.ReviewsControllerTests
{
    [TestFixture]
    public class Details_Should : ReviewsControllerTestBase
    {
        [Test]
        public void HaveAllowAnonymousAttribute()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();

            // Act
            var hasAttribute = AttributeTester.MethodHasAttribute(() => sut.Details(default(int)), typeof(AllowAnonymousAttribute));

            // Assert
            Assert.IsTrue(hasAttribute);
        }

        [TestCase(1)]
        [TestCase(49979687)]
        public void Call_ReviewServiceGetByIdMethodOnceWithCorrectParams(int expectedId)
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();

            // Act
            sut.Details(expectedId);

            // Assert
            reviewService.Verify(s => s.GetById(expectedId), Times.Once);
        }

        [TestCase(1)]
        [TestCase(49979687)]
        public void ReturnNotFoundView_WhenReviewIsNotFound(int expectedId)
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(s => s.GetById(expectedId))
                         .Returns((IBeerReview)null);
            // Act
            var result = sut.Details(expectedId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("NotFound", result.ViewName);
        }

        [Test]
        public void Call_IMapperMapMethodOnceWithCorrectParams_WhenReviewIsFound()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();
            var expectedBeerReview = new Mock<IBeerReview>();
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(s => s.GetById(It.IsAny<int>()))
                         .Returns(expectedBeerReview.Object);
            var mapper = this.MockingKernel.GetMock<IMapper>();

            // Act
            sut.Details(It.IsAny<int>());

            // Assert
            mapper.Verify(m => m.Map<IBeerReview, SingleReviewViewModel>(expectedBeerReview.Object), Times.Once);
        }

        [Test]
        public void ReturnValueFrom_IMapperMapMethod_WhenReviewIsFound()
        {
            // Arrange
            var sut = this.MockingKernel.Get<ReviewsController>();
            var beerReview = new Mock<IBeerReview>();
            var expectedViewModel = new SingleReviewViewModel();
            var reviewService = this.MockingKernel.GetMock<IBeerReviewService>();
            reviewService.Setup(s => s.GetById(It.IsAny<int>()))
                         .Returns(beerReview.Object);
            var mapper = this.MockingKernel.GetMock<IMapper>();
            mapper.Setup(m => m.Map<IBeerReview, SingleReviewViewModel>(beerReview.Object))
                  .Returns(expectedViewModel);

            // Act
            var result = sut.Details(It.IsAny<int>());

            // Assert
            Assert.IsNotNull(result);
            var actualViewModel = result.Model as SingleReviewViewModel;
            Assert.AreSame(expectedViewModel, actualViewModel);
        }
    }
}
