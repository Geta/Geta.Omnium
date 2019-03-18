using System;
using Mediachase.Commerce.Orders.Dto;

namespace Geta.Omnium
{
    public interface IPaymentManagerFacade
    {
        PaymentMethodDto GetPaymentMethod(Guid paymentMethodId);
        PaymentMethodDto GetPaymentMethodBySystemName(string name, string languageId);
        PaymentMethodDto GetPaymentMethodsByMarket(string marketId);
        void SavePaymentMethod(PaymentMethodDto dto);
    }
}