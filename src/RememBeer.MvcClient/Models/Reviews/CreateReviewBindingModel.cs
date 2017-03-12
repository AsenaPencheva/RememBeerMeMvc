﻿using System.ComponentModel.DataAnnotations;
using System.Web;

namespace RememBeer.MvcClient.Models.Reviews
{
    public class CreateReviewBindingModel : EditReviewBindingModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a beer from the dropdown.")]
        public int BeerId { get; set; }

        [FileExtensions(ErrorMessage = "Upload a valid image file.")]
        public HttpPostedFileBase Image { get; set; }
    }
}