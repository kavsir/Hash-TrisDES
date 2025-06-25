# Hash-TrisDES
Ứng dụng SHA và Triple DES để bảo vệ mật khẩu người dùng trong cơ sở dữ liệu
• Mô tả: Sinh viên sẽ xây dựng một hệ thống xác thực người dùng an toàn bằng cách:
      – Sử dụng SHA và Triple DES để xử lý mật khẩu an toàn.
      – Dùng Salt ngẫu nhiên riêng cho từng người dùng.
      – Kết hợp băm mật khẩu và tên đăng nhập để tăng tính duy nhất.
      – Các chức năng: đăng ký, đăng nhập, đổi mật khẩu.
      – Tự động khóa tài khoản sau nhiều lần nhập sai.
      – Giao diện quản trị: quản lý tài khoản, mở khóa, xem lịch sử đăng nhập


• Tính năng yêu cầu:
1. Chức năng người dùng:

    – Đăng ký tài khoản
    
    – Đăng nhập tài khoản
    
    – Đổi mật khẩu (sau khi đăng nhập thành công)
    
    – Cảnh báo nếu nhập sai mật khẩu
    
    – Tự động khóa tài khoản sau 5 lần đăng nhập sai liên tiếp

3. Bảo mật:

      – Băm mật khẩu bằng SHA-256
      
      – Băm tên người dùng bằng SHA-256
      
      – Kết hợp hai giá trị hash, sau đó băm lại
      
      – Mã hóa kết quả cuối bằng Triple DES (3DES)
      
      – Dùng Salt ngẫu nhiên cho mỗi người dùng
  
5. Đối với quản trị viên:

      – Giao diện quản trị riêng tại /admin
       
      – Xem danh sách người dùng (ẩn mật khẩu)
      
      – Xóa tài khoản
        
      – Mở khóa tài khoản bị khóa
        
      – Xem lịch sử đăng nhập (log)

**• Hướng dẫn:**
– Đăng ký tài khoản:
1. Người dùng nhập username và password
2. Tạo Salt ngẫu nhiên
3. Băm password + salt
4. Băm username
5. Kết hợp và băm lại
6. Mã hóa bằng Triple DES
7. Lưu vào cơ sở dữ liệu:
*username
*salt
*encrypted_password
*fail_attempts = 0
*is_locked = FALSE

**– Đăng nhập:**
1. Nhận username và password nhập vào
2. Truy xuất salt, encrypted_password, fail_attempts, is_locked từ CSDL
3. Nếu is_locked = TRUE ⇒ trả về: ”Tài khoản bị khóa”
4. Xử lý mật khẩu như khi đăng ký
5. So sánh kết quả mã hóa:
  * Nếu đúng:
    · Cho đăng nhập
    · fail_attempts = 0
  * Nếu sai:
    · fail_attempts ++
    · Nếu fail_attempts >= 5 ⇒ is_locked = TRUE
    · Thông báo: ”Sai mật khẩu. Bạn còn X lần thử”
6. Ghi log vào bảng login_logs

**– Đổi mật khẩu:**
1. Người dùng đăng nhập thành công
2. Nhập mật khẩu cũ và mật khẩu mới
3. Kiểm tra mật khẩu cũ
4. Nếu đúng:
* Tạo Salt mới
* Hash và mã hóa lại mật khẩu mới
* Cập nhật vào cơ sở dữ liệu\


**– Quản trị viên:**
1. Giao diện admin hiển thị:
* Danh sách người dùng
* Tên đăng nhập
* Trạng thái (Hoạt động / Bị khóa)
* Ngày tạo
* Nút: Xóa, Mở khóa
* Chức năng mở khóa: admin nhấn nút

**– Ghi log hoạt động đăng nhập:**
  * Log mỗi lần đăng nhập
  * Ghi rõ: Thành công / thất bại
  * Thời gian thực hiện

