/**
 * File quản lý Logic cho Tin đăng (Rao vặt)
 * Không dùng JQuery để tối ưu tốc độ load trang
 */

document.addEventListener('DOMContentLoaded', function () {

    // --- 1. LOGIC PREVIEW ẢNH (Dùng cho cả Post và Edit) ---
    const fileInput = document.getElementById('file-input');
    const previewContainer = document.getElementById('preview-container');
    const uploadInstruction = document.getElementById('upload-instruction'); // Phần icon đám mây

    if (fileInput && previewContainer) {
        fileInput.addEventListener('change', function (e) {
            previewContainer.innerHTML = ''; // Xóa preview cũ

            if (e.target.files.length > 0) {
                // Ẩn icon hướng dẫn nếu có chọn ảnh
                if (uploadInstruction) uploadInstruction.style.display = 'none';

                [...e.target.files].forEach(file => {
                    const reader = new FileReader();
                    reader.onload = function (event) {
                        const div = document.createElement('div');
                        div.className = 'position-relative';
                        div.innerHTML = `
                            <img src="${event.target.result}" 
                                 style="width: 80px; height: 80px; object-fit: cover;" 
                                 class="rounded-3 border shadow-sm">
                        `;
                        previewContainer.appendChild(div);
                    };
                    reader.readAsDataURL(file);
                });
            } else {
                // Hiện lại hướng dẫn nếu không chọn ảnh nào
                if (uploadInstruction) uploadInstruction.style.display = 'block';
            }
        });
    }

    // --- 2. LOGIC THẢ TIM (FAVORITE) ---
    document.addEventListener('click', function (e) {
        const btnFav = e.target.closest('.btn-favorite');

        // Lấy UserId từ thuộc tính data-user-id gắn ở thẻ <body> trong _Layout
        const userId = document.body.getAttribute('data-user-id');

        if (btnFav) {
            e.preventDefault(); // Ngăn chặn nhảy trang nếu bọc trong thẻ <a>

            if (!userId || userId === "") {
                alert("Vui lòng đăng nhập để lưu tin này!");
                window.location.href = "/Account/Login";
                return;
            }

            const productId = btnFav.getAttribute('data-id');
            handleToggleFavorite(btnFav, productId);
        }
    });
});

/**
 * Hàm xử lý Thả tim qua API
 */
function handleToggleFavorite(btn, pId) {
    // Lấy Token bảo mật (RequestVerificationToken) từ form có sẵn trên trang
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    fetch('/Ads/ToggleFavorite', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token // Bắt buộc phải có để qua cửa bảo mật
        },
        body: `productId=${pId}`
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const icon = btn.querySelector('i');
                // Đổi trạng thái Icon
                if (data.isFavorite) {
                    icon.className = 'bi bi-heart-fill text-danger';
                } else {
                    icon.className = 'bi bi-heart text-secondary';
                }

                // Hiệu ứng nảy nhẹ cho icon cho "sang"
                icon.style.transform = 'scale(1.3)';
                setTimeout(() => icon.style.transform = 'scale(1)', 200);
            } else {
                alert(data.message || "Có lỗi xảy ra, thử lại sau!");
            }
        })
        .catch(err => {
            console.error("Error:", err);
        });
}