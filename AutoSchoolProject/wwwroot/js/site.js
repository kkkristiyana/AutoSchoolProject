// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Български file input-и
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".js-bulgarian-file-input").forEach(function (input) {
        const wrapper = input.closest(".file-picker");
        const fileNameElement = wrapper ? wrapper.querySelector(".js-file-name") : null;

        const updateFileName = function () {
            if (!fileNameElement) {
                return;
            }

            if (input.files && input.files.length > 0) {
                fileNameElement.textContent = input.files[0].name;
            } else {
                fileNameElement.textContent = "Няма избран файл";
            }
        };

        input.addEventListener("change", updateFileName);
        updateFileName();
    });
});
