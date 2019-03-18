using System;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Omnium
{
    [ServiceConfiguration(typeof(IPaymentManagerFacade))]
    public class PaymentManagerFacade : IPaymentManagerFacade
    {
        public PaymentMethodDto GetPaymentMethod(Guid paymentMethodId)
        {
            return PaymentManager.GetPaymentMethod(paymentMethodId);
        }

        public PaymentMethodDto GetPaymentMethodBySystemName(string name, string languageId)
        {
            return PaymentManager.GetPaymentMethodBySystemName(name, languageId);
        }

        public PaymentMethodDto GetPaymentMethodsByMarket(string marketId)
        {
            return PaymentManager.GetPaymentMethodsByMarket(marketId);
        }

        public void SavePaymentMethod(PaymentMethodDto dto)
        {
            PaymentManager.SavePayment(dto);
        }
    }
}
