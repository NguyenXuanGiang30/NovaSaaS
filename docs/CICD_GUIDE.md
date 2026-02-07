# Hướng dẫn Thiết lập CI/CD cho NovaSaaS

Tài liệu này hướng dẫn cách sử dụng và cấu hình quy trình CI/CD (Continuous Integration / Continuous Deployment) đã được tích hợp sẵn trong dự án.

## 1. Tổng quan

Hệ thống CI/CD sử dụng **GitHub Actions** để tự động hóa:
-   **CI (`ci.yml`)**: Chạy mỗi khi có Push hoặc Pull Request vào nhánh `main`. Quy trình này sẽ:
    -   Restore dependencies.
    -   Build code (.NET 10).
    -   Chạy Unit Tests.
-   **CD (`cd.yml`)**: Chạy khi bạn Push một **Tag phiên bản** (ví dụ: `v1.0.0`) hoặc kích hoạt thủ công. Quy trình này sẽ:
    -   Build Docker Image từ `Dockerfile`.
    -   Push image lên **GitHub Container Registry (GHCR)**.

## 2. Dockerfile

File `Dockerfile` nằm ở thư mục gốc (`d:\NovaSaaS\Dockerfile`), sử dụng Multi-stage build để tối ưu dung lượng:
-   **Base Image**: `mcr.microsoft.com/dotnet/aspnet:10.0`
-   **Build SDK**: `mcr.microsoft.com/dotnet/sdk:10.0`

## 3. Cấu hình GitHub Repository

Để CD hoạt động (Push Docker Image), bạn cần đảm bảo quyền ghi vào GitHub Packages:

1.  Vào Repository trên GitHub.
2.  Đi tới **Settings** -> **Actions** -> **General**.
3.  Kéo xuống phần **Workflow permissions**.
4.  Chọn **Read and write permissions**.
5.  Nhấn **Save**.

## 4. Cách kích hoạt Deploy (CD)

Để build và publish phiên bản mới, bạn chỉ cần tạo Tag:

```bash
# Tạo tag v1.0.0
git tag v1.0.0

# Push tag lên GitHub -> Sẽ kích hoạt CD workflow
git push origin v1.0.0
```

Hoặc vào tab **Actions** trên GitHub -> Chọn **NovaSaaS CD** -> Nhấn **Run workflow**.

## 5. Sử dụng Docker Image

Sau khi CD chạy xong, Image của bạn sẽ có tại đường dẫn (ví dụ):
`ghcr.io/nguyenxuangiang30/novasaas:v1.0.0`

Bạn có thể chạy thử trên server/local:

```bash
docker run -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="YourRealConnectionString" \
  -e JwtSettings__SecretKey="YourSecretKey" \
  ghcr.io/nguyenxuangiang30/novasaas:v1.0.0
```

## 6. Thiết lập Tự động Deploy (Gợi ý)

Hiện tại CD mới chỉ **đóng gói** (Push Image). Để **tự động chạy** phiên bản mới trên server, bạn có 2 cách phổ biến:

### Cách 1: Sử dụng Watchtower (Khuyên dùng - Đơn giản nhất)
Bạn chạy thêm một container tên là **Watchtower** trên server của mình. Nó sẽ tự động kiểm tra xem có Image mới không, nếu có nó sẽ tự pull về và restart app của bạn.

```bash
# Chạy Watchtower trên server
docker run -d \
  --name watchtower \
  -v /var/run/docker.sock:/var/run/docker.sock \
  containrrr/watchtower \
  --interval 300 # Kiểm tra mỗi 5 phút
```

### Cách 2: Sử dụng SSH Action (Chủ động)
Cập nhật file `.github/workflows/cd.yml` để SSH vào server và chạy lệnh.

**Yêu cầu:**
- Cần cung cấp IP Server, Username, và Private Key cho GitHub Secrets.
- Thêm step vào cuối file `cd.yml`:

```yaml
    - name: Deploy to Server via SSH
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.SERVER_IP }}
        username: ${{ secrets.SERVER_USER }}
        key: ${{ secrets.SSH_PRIVATE_KEY }}
        script: |
          docker pull ghcr.io/nguyenxuangiang30/novasaas:${{ github.ref_name }}
          docker stop novasaas || true
          docker rm novasaas || true
          docker run -d --name novasaas -p 8080:80 ghcr.io/nguyenxuangiang30/novasaas:${{ github.ref_name }}
```
