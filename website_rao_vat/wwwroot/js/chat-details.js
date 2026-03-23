// 1. Thiết lập kết nối
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect() // Tự động kết nối lại nếu rớt mạng
    .build();

// 2. Lắng nghe tin nhắn từ Server
connection.on("ReceiveMessage", function (senderId, message, time) {
    // Lấy ID của chính mình từ một thẻ ẩn (Tôi sẽ hướng dẫn ông thêm thẻ này ở dưới)
    var myId = $("#currentUserId").val();

    // Kiểm tra xem tin nhắn này là của mình gửi hay người khác gửi
    var isMe = senderId.toString() === myId.toString();

    var msgHtml = `
        <div class="d-flex ${isMe ? 'justify-content-end' : 'justify-content-start'} mb-3">
            <div class="p-2 rounded shadow-sm ${isMe ? 'bg-primary text-white' : 'bg-white border'}" 
                 style="max-width: 80%; min-width: 50px;">
                <div class="small ${isMe ? 'text-light' : 'text-muted'}" style="font-size: 0.7rem;">
                    ${isMe ? 'Bạn' : 'Người bán'} • ${time}
                </div>
                <div>${message}</div>
            </div>
        </div>`;

    var $list = $("#messagesList");
    $list.append(msgHtml);

    // Cuộn xuống tin nhắn mới nhất
    $list.animate({ scrollTop: $list[0].scrollHeight }, 300);
});

// 3. Khởi động kết nối (Chỉ giữ lại 1 block này thôi Ninh nhé)
connection.start().then(function () {
    console.log("SignalR Connected!");
}).catch(function (err) {
    console.error("SignalR Connection Error: ", err.toString());
});

// 4. Xử lý sự kiện Gửi tin nhắn
$(document).ready(function () {
    // Hàm xử lý gửi tin
    function sendMessage() {
        var receiverId = $("#sendButton").data("receiver");
        var message = $("#messageInput").val();

        if (message.trim() !== "") {
            connection.invoke("SendMessage", parseInt(receiverId), message)
                .catch(function (err) {
                    console.error("Gửi tin thất bại: " + err.toString());
                    alert("Không thể gửi tin nhắn. Vui lòng đăng nhập lại!");
                });
            $("#messageInput").val("").focus();
        }
    }

    // Click nút Gửi
    $("#sendButton").click(function () {
        sendMessage();
    });

    // Nhấn Enter để gửi (Tính năng này cực kỳ nên có!)
    $("#messageInput").keypress(function (e) {
        if (e.which == 13) { // 13 là mã phím Enter
            sendMessage();
            return false;
        }
    });
});
function openChat() {
    var receiverId = $("#sendButton").data("receiver");
    var myId = $("#currentUserId").val();

    // 1. Kiểm tra xem khung chat có tồn tại không (Phòng hờ lỗi render)
    if ($("#chatBox").length === 0) return;

    // 2. Tải toàn bộ lịch sử chat cũ
    $.get("/Chat/GetChatHistory", { receiverId: receiverId }, function (data) {
        $("#messagesList").empty();

        data.forEach(function (msg) {
            var isMe = msg.senderId.toString() === myId.toString();
            var msgHtml = `
                <div class="d-flex ${isMe ? 'justify-content-end' : 'justify-content-start'} mb-3">
                    <div class="p-2 rounded shadow-sm ${isMe ? 'bg-primary text-white' : 'bg-white border'}" style="max-width: 80%;">
                        <div>${msg.content}</div>
                        <small style="font-size: 0.6rem; opacity: 0.7;">${msg.time}</small>
                    </div>
                </div>`;
            $("#messagesList").append(msgHtml);
        });

        $("#chatBox").fadeIn(300);
        $("#messagesList").scrollTop($("#messagesList")[0].scrollHeight);
    });
}

function closeChat() { $("#chatBox").fadeOut(300); }