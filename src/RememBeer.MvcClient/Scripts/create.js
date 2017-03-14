﻿
$("#create-form").submit(function (ev) {
    ev.preventDefault();
    ev.stopPropagation();
    var $form = $(this);
    if ($form.valid()) {
        var form = $('#create-form')[0];
        var formData = new FormData(form);

        $.ajax({
            type: "POST",
            url: '/reviews',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                $('.modal').modal('close');
                $("#content").html(response);
                showSuccess('Review has been created!');
            },
            error: function (error) {
                handleAjaxError(error);
            }
        });
    }
});