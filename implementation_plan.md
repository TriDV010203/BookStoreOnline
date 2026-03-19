# QR Payment (Sepay) – Implementation Plan

## Tổng quan

Người dùng chọn phương thức thanh toán khi checkout:
- **COD** (Thanh toán khi nhận hàng) – giữ nguyên luồng hiện tại.
- **QR / Chuyển khoản** – hiển thị mã QR ngân hàng (Sepay), đơn hàng tạo với trạng thái `"AwaitingPayment"`. Sau khi Sepay xác nhận giao dịch qua webhook, trạng thái tự động chuyển sang `"Paid"`.

---

## Proposed Changes

### API

#### [MODIFY] [Order.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.API/Models/Order.cs)
- Thêm trường `PaymentMethod` (string, default `"COD"`).

#### [MODIFY] [Payment.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.API/Models/Payment.cs)
- Thêm `TransactionId` (string, nullable) để lưu mã giao dịch từ Sepay.

#### [MODIFY] [OrdersController.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.API/Controllers/OrdersController.cs)
- Cập nhật `validStatuses` thêm `"AwaitingPayment"` và `"Paid"`.
- Thêm `paymentMethod` vào body khi tạo order.

#### [MODIFY] [PaymentController.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.API/Controllers/PaymentController.cs)
- Thêm endpoint **`POST /api/payment/webhook`** nhận callback từ Sepay (qua ngrok).
  - Parse body JSON từ Sepay: `transferAmount`, `content` (chứa mã đơn hàng).
  - Tìm đơn hàng có trạng thái `"AwaitingPayment"` theo mã trong nội dung chuyển khoản.
  - Ghi bản ghi [Payment](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.API/Models/Payment.cs#6-20) vào DB.
  - Cập nhật `OrderStatus = "Paid"`.
- Thêm endpoint **`GET /api/payment/status/{orderId}`** – trả về `{ isPaid: bool, status: string }` để client polling.
- Thêm config Sepay (`SepayApiKey`) vào [appsettings.json](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/appsettings.json) để xác thực webhook.

#### [MODIFY] [appsettings.json (API)](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.API/appsettings.json)
- Thêm section `"Sepay": { "ApiKey": "..." }`.

---

### MVC (Client)

#### [MODIFY] [CheckoutViewModel.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Models/CheckoutViewModel.cs)
- Thêm `PaymentMethod` (`"COD"` | `"QR"`).

#### [MODIFY] [OrderViewModel.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Models/OrderViewModel.cs)
- Thêm `PaymentMethod` vào [OrderViewModel](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Models/OrderViewModel.cs#3-15) và [ApiOrderDto](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Services/ApiService.cs#510-521).

#### [MODIFY] [IApiService.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Services/IApiService.cs)
- Cập nhật chữ ký [PlaceOrderAsync](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Services/ApiService.cs#275-301) thêm `paymentMethod`.
- Thêm `GetPaymentStatusAsync(int orderId)` → [(bool isPaid, string status)](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Controllers/CartController.cs#40-87).

#### [MODIFY] [ApiService.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Services/ApiService.cs)
- Cập nhật [PlaceOrderAsync](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Services/ApiService.cs#275-301) để gửi `paymentMethod` lên API.
- Implement `GetPaymentStatusAsync`.

#### [MODIFY] [CartController.cs](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Controllers/CartController.cs)
- Sau khi đặt hàng thành công:
  - Nếu `PaymentMethod == "COD"` → redirect bình thường đến [OrderSuccess](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Controllers/CartController.cs#176-181).
  - Nếu `PaymentMethod == "QR"` → redirect đến `Cart/PayByQR?orderId=...&amount=...`.
- Thêm action `PayByQR(int orderId, decimal amount)` (GET) – hiển thị trang QR.
- Thêm action `CheckPaymentStatus(int orderId)` (GET, AJAX) – gọi API và trả JSON `{ isPaid }`.

#### [NEW] [PayByQR.cshtml](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Views/Cart/PayByQR.cshtml)
Trang hiển thị:
- Mã QR VietQR (dùng `img.vietqr.io` – API công khai, không cần key) với số tiền và nội dung đơn hàng.
- Thông tin tài khoản ngân hàng (cấu hình trong appsettings MVC).
- Đồng hồ đếm ngược 10 phút.
- Auto-polling mỗi 5 giây đến `/Cart/CheckPaymentStatus?orderId=X` – khi `isPaid = true` hiển thị thông báo thành công rồi redirect đến [OrderSuccess](file:///b:/Project%20c%C3%A1%20nh%C3%A2n/BookStoreOnline/BookStoreOnline.MVC/Controllers/CartController.cs#176-181).

#### [MODIFY] [Checkout.cshtml](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Views/Cart/Checkout.cshtml)
- Thêm UI chọn phương thức thanh toán: 2 card lựa chọn (COD / QR) style đẹp.
- Câu nút "Xác nhận đặt hàng" thay đổi label theo lựa chọn.

#### [MODIFY] [MyOrders.cshtml](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/Views/Account/MyOrders.cshtml)
- Thêm badge trạng thái `AwaitingPayment` và `Paid`.
- Nếu đơn `AwaitingPayment` → hiển thị nút "Thanh toán QR" dẫn đến trang PayByQR.

#### [MODIFY] [appsettings.json (MVC)](file:///b:/Project%20cá%20nhân/BookStoreOnline/BookStoreOnline.MVC/appsettings.json)
- Thêm `"BankInfo": { "BankCode": "...", "AccountNo": "...", "AccountName": "..." }`.

---

## Luồng hoạt động

```
Người dùng chọn QR → POST /Cart/Checkout (PaymentMethod=QR)
  → API tạo Order (status=AwaitingPayment)
  → Redirect đến /Cart/PayByQR?orderId=X&amount=Y
    → Hiển thị QR (VietQR public API)
    → JS polling /Cart/CheckPaymentStatus
      ← API GET /api/payment/status/{orderId}
    → Người dùng quét & chuyển khoản
    → Sepay gọi webhook → POST /api/payment/webhook (qua ngrok)
      → API cập nhật Order = "Paid", ghi Payment record
    → JS polling nhận isPaid=true → redirect OrderSuccess
```

---

## Verification Plan

### Manual Testing
1. **Build hệ thống**: Chạy cả 2 project (API và MVC). Đảm bảo API ở `http://localhost:5173` và MVC ở `http://localhost:xxxx`.
2. **Migration**: Chạy `Add-Migration AddPaymentMethod` và `Update-Database` trong Package Manager Console (API project) để cập nhật DB.
3. **Test COD**: Thêm sách vào giỏ → Checkout → chọn COD → Đặt hàng → kiểm tra trang OrderSuccess và DB có `PaymentMethod="COD"`.
4. **Test QR**:
   - Chọn QR → Đặt hàng → xem trang PayByQR có hiển thị mã QR không.
   - Kiểm tra DB: đơn hàng có `OrderStatus="AwaitingPayment"`.
   - Gọi test webhook bằng **Postman**: `POST http://localhost:5173/api/payment/webhook` với body Sepay mẫu (xem bên dưới) → DB cập nhật `Paid`.
   - Hoặc dùng ngrok + tài khoản thật Sepay để test toàn bộ luồng.

**Body test webhook (Postman):**
```json
{
  "id": 1,
  "gateway": "MBBank",
  "transactionDate": "2024-01-01 12:00:00",
  "accountNumber": "0123456789",
  "content": "Thanh toan don hang #5",
  "transferType": "in",
  "transferAmount": 150000,
  "accumulated": 150000,
  "referenceCode": "ABC123",
  "description": "Thanh toan don hang #5",
  "apiKey": "your-api-key-here"
}
```

5. **Test MyOrders**: Vào trang đơn hàng → nếu đơn `AwaitingPayment` xuất hiện nút "Thanh toán QR" → click đến trang PayByQR.
