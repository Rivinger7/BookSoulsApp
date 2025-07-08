using Net.payOS.Types;

namespace BookSoulsApp.Application.ThirdPartyServiceInterfaces.Payment
{
    public interface IPayosService
    {
        Task<string> ConfirmWebhook(string url);
        Task<string> CreatePaymentLinkRequest(string orderId);
        Task<PaymentLinkInformation> GetPaymentLinkInfo(long paymentCode);
        Task HandleWebhook(WebhookType webhookType);
    }
}
