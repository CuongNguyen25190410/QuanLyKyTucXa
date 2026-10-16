// Script dùng chung. Ví dụ: tự đóng thông báo thành công sau 5 giây.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-success').forEach(function (el) {
        setTimeout(function () {
            if (window.bootstrap) { bootstrap.Alert.getOrCreateInstance(el).close(); }
        }, 5000);
    });
});
