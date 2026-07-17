# HomeServicePlatform — Backend (.NET, Clean Architecture)

## ⚙️ Cấu hình bí mật (secrets) — BẮT BUỘC trước khi chạy

Các giá trị nhạy cảm **không còn nằm trong `appsettings.json`** (đã gỡ để tránh lộ khi commit).
Bạn phải nạp chúng qua **User Secrets** (khi chạy local) hoặc **biến môi trường** (trên server).

### Chạy local (dev) — dùng User Secrets

Project `HomeServicePlatform.Api` đã bật User Secrets sẵn. Từ thư mục
`src/HomeServicePlatform.Api`, chạy:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "JwtSettings:Secret" "<chuỗi bí mật ngẫu nhiên, tối thiểu 32 ký tự>"

# (tùy chọn) upload ảnh qua Cloudinary
dotnet user-secrets set "Cloudinary:CloudName" "..."
dotnet user-secrets set "Cloudinary:ApiKey"   "..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."
```

User Secrets lưu ngoài thư mục project (không bị commit).

### Trên server (production) — dùng biến môi trường

.NET tự map biến môi trường theo cú pháp `Section__Key` (hai dấu gạch dưới):

| Cấu hình                              | Biến môi trường                          |
| ------------------------------------- | ---------------------------------------- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection`   |
| `JwtSettings:Secret`                  | `JwtSettings__Secret`                     |
| `Cloudinary:ApiSecret`                | `Cloudinary__ApiSecret`                   |

> Ứng dụng **fail-fast** khi khởi động nếu `JwtSettings:Secret` trống hoặc ngắn hơn
> 32 ký tự — đây là chủ đích để không chạy nhầm với secret rỗng.

## ⚠️ Nếu bạn vừa clone từ lịch sử cũ

Secret cũ (mật khẩu DB, JWT secret) đã từng bị commit trong lịch sử Git → xem như **đã lộ**.
Hãy **đổi (rotate)** chúng:

- Đổi mật khẩu database trên Neon và cập nhật lại chuỗi kết nối.
- Sinh JWT secret mới (đổi secret sẽ vô hiệu hóa mọi token đang đăng nhập — bình thường).
- (Tùy chọn) gột secret khỏi lịch sử Git bằng `git filter-repo` hoặc BFG.
