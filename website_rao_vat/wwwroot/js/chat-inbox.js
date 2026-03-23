// wwwroot/js/chat-inbox.js

// 1. Khởi tạo kết nối SignalR
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

// 2. Lắng nghe tin nhắn từ Server
connection.on("ReceiveMessage", function (senderId, message, time) {
    var myId = $("#currentUserId").val();
    var activeReceiverId = $("#sendButton").data("receiver");

    // Nếu tin nhắn liên quan đến cuộc hội thoại đang mở, thêm trực tiếp vào giao diện
    if (senderId == activeReceiverId || senderId == myId) {
        var isMe = senderId.toString() === myId.toString();
        var html = `
            <div class="msg ${isMe ? 'msg-me' : 'msg-them'}">
                ${message}
                <div style="font-size: 0.6rem; opacity: 0.7; text-align: ${isMe ? 'right' : 'left'}">
                    ${time}
                </div>
            </div>`;

        $("#messagesList").append(html);
        scrollToBottom();
    } else {
        // Nếu là tin nhắn từ người khác, có thể reload để cập nhật danh sách bên trái
        location.reload();
    }
});

// 3. Bắt đầu kết nối
connection.start().then(function () {
    console.log("SignalR Connected to Inbox");
}).catch(function (err) {
    return console.error(err.toString());
});

// 4. Các hàm bổ trợ và sự kiện
function scrollToBottom() {
    var list = document.getElementById("messagesList");
    if (list) {
        list.scrollTop = list.scrollHeight;
    }
}

$(document).ready(function () {
    // Cuộn xuống đáy khi mới load trang
    scrollToBottom();

    // Xử lý nút Gửi
    $("#sendButton").click(function () {
        var msg = $("#messageInput").val();
        var rId = $(this).data("receiver");

        if (msg.trim() !== "") {
            connection.invoke("SendMessage", parseInt(rId), msg)
                .catch(function (err) {
                    return console.error(err.toString());
                });
            $("#messageInput").val("").focus();
        }
    });

    // Nhấn Enter để gửi cho nhanh
    $("#messageInput").keypress(function (e) {
        if (e.which == 13) {
            $("#sendButton").click();
            return false;
        }
    });
});