using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Payment;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;
using BookSoulsApp.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Net.payOS;
using Net.payOS.Types;

namespace BookSoulsApp.Infrastructure.ThirdPartyServices.Payment
{
    public class PayosService : IPayosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly PayOS _payOs;
        private readonly string _cancelUrl;
        private readonly string _returnUrl;
        public PayosService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            string clientId = config["PAYOS_CLIENT_ID"]
                ?? throw new NotFoundCustomException("PAYOS_CLIENT_ID chưa được config.");
            string apiKey = config["PAYOS_API_KEY"]
                ?? throw new NotFoundCustomException("PAYOS_API_KEY chưa được config.");
            string checksumKey = config["PAYOS_CHECKSUM_KEY"]
                ?? throw new NotFoundCustomException("PAYOS_CHECKSUM_KEY chưa được config.");
            _payOs = new PayOS(clientId, apiKey, checksumKey);
            _cancelUrl = config["PAYOS_CANCEL_URL"]
                ?? throw new NotFoundCustomException("PAYOS_CANCEL_URL chưa được config.");
            _returnUrl = config["PAYOS_RETURN_URL"]
                ?? throw new NotFoundCustomException("PAYOS_RETURN_URL chưa được config.");
        }
        public async Task<string> CreatePaymentLinkRequest(string orderId)
        {
            Order order = await _unitOfWork.GetCollection<Order>().Find(o => o.Id == orderId && o.PaymentStatus == PaymentStatus.None).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Order Not Found or Paid");
            long paymentCode = GenerateOrderCode();
            long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds();
            List<ItemData> items = new List<ItemData>();
            foreach (OrderBooks ob in order.OrderBooks)
            {
                items.Add(new ItemData(ob.BookTitle, ob.Quantity, (int)ob.BookPrice));
            }
            PaymentData paymentData = new PaymentData(
                paymentCode,
                (int)order.TotalPrice,
                "Thanh toán đơn hàng",
                items,
                cancelUrl: _cancelUrl + paymentCode,
                returnUrl: _returnUrl,
                expiredAt: expiredAt); // hạn link thanh toán 15 phút
            // tạo link thanh toán
            CreatePaymentResult createPayment = await _payOs.createPaymentLink(paymentData);
            // lưu PaymentCode vào Order
            UpdateResult updateResult = await _unitOfWork.GetCollection<Order>()
                .UpdateOneAsync(c => c.Id == orderId, Builders<Order>.Update.Set(c => c.PaymentCode, paymentCode));

            if (updateResult.ModifiedCount == 0)
            {
                throw new NotFoundCustomException("Save PaymentCode failed");
            }
            // tạo thông tin payment
            return createPayment.checkoutUrl;
        }
        public async Task<PaymentLinkInformation> GetPaymentLinkInfo(long paymentCode)
        {
            // lấy link QR từ paymentCode (payOS quản lý)
            return await _payOs.getPaymentLinkInformation(paymentCode);
        }
        public async Task HandleWebhook(WebhookType webhookType)
        {
            // Xác minh dữ liệu
            WebhookData webhookData = _payOs.verifyPaymentWebhookData(webhookType);

            if (webhookData.code == "00") ///Thanh toán thành công
            {
                // Xử lý Order với webhookData.description (nơi lưu orderId)
                Console.WriteLine($"Đơn thanh toán thành công: {webhookData.desc}");

                // update order.PaymentStatus 
                Order order = await _unitOfWork.GetCollection<Order>().Find(o => o.PaymentCode == webhookData.orderCode).FirstOrDefaultAsync()
                ?? throw new NotFoundCustomException("Order Not Found or Paid");
                UpdateResult updateResult = await _unitOfWork.GetCollection<Order>()
                    .UpdateOneAsync(c => c.Id == order.Id, Builders<Order>.Update.Set(c => c.PaymentStatus, PaymentStatus.Paid));

                if (updateResult.ModifiedCount == 0)
                {
                    throw new NotFoundCustomException("Update Payment Status failed");
                }
            }
            else
            {
                Console.WriteLine($"Thanh toán KHÔNG thành công: {webhookData.desc}");
            }
        }
        public async Task<string> ConfirmWebhook(string url)
        {
            try
            {
                return await _payOs.confirmWebhook(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI gọi confirmWebhook: " + ex.Message);
                throw;
            }
        }
        private static long GenerateOrderCode()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // 13 chữ số
            int randomPart = Random.Shared.Next(0, 1000);                    // 3 chữ số
            string combined = timestamp.ToString() + randomPart.ToString("D3"); // nối thành 16 chữ số
            return long.Parse(combined); // ép thành long
        }
    }
}
