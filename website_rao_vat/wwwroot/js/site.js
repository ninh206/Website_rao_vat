$(document).on("click", ".btn-favorite", function (e) {
    e.preventDefault();
    var btn = $(this);

    if (btn.hasClass("disabled")) return;

    btn.addClass("disabled");
    var pId = btn.data("id");

    $.post("/Ads/ToggleFavorite", { productId: pId }, function (data) {
        btn.removeClass("disabled");
        if (data.success) {
            // Logic đổi màu tim
            if (data.isFavorite) {
                btn.find("i").removeClass("bi-heart").addClass("bi-heart-fill text-danger");
            } else {
                btn.find("i").removeClass("bi-heart-fill text-danger").addClass("bi-heart");
            }
        } else {
            alert(data.message);
            window.location.href = "/Account/Login";
        }
        // Trong site.js, cập nhật phần xử lý thành công:
        if (data.success) {
            if (data.isFavorite) {
                btn.find("i").removeClass("bi-heart").addClass("bi-heart-fill text-danger");
            } else {
                btn.find("i").removeClass("bi-heart-fill text-danger").addClass("bi-heart");

                // NẾU ĐANG Ở TRANG CÁ NHÂN: Ẩn luôn cái thẻ đó đi
                btn.closest(".col-12").fadeOut(300);
            }
        }
    });
});