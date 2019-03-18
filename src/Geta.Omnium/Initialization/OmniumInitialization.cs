using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Geta.Omnium.Models;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.Omnium.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    internal class OmniumInitialization : IInitializableModule
    {
        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string PurchaseOrderClass = "PurchaseOrder";
        public const string ShipmentClass = "ShipmentEx";

        public void Initialize(InitializationEngine context)
        {
            var mdContext = CatalogContext.MetaDataContext;

            JoinField(mdContext,
                GetOrCreateField(mdContext, OrderNamespace,
                    OrderConstants.MetaFieldOmniumSynchronized,
                    OrderConstants.MetaFieldOmniumSynchronized, MetaDataType.Boolean),
                PurchaseOrderClass);

            JoinField(mdContext,
                GetOrCreateField(mdContext, OrderNamespace,
                    OrderConstants.MetaFieldOmniumSynchronizedDate,
                    OrderConstants.MetaFieldOmniumSynchronizedDate, MetaDataType.DateTime),
                PurchaseOrderClass);
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        private MetaField GetOrCreateField(
            MetaDataContext mdContext,
            string metaNamespace,
            string fieldName,
            string friendlyName,
            MetaDataType metaDataType = MetaDataType.LongString)
        {
            return MetaField.Load(mdContext, fieldName) ?? MetaField.Create(mdContext, metaNamespace,
                       fieldName, friendlyName, string.Empty, metaDataType, Int32.MaxValue, true, false, false, false);
        }

        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        private bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }
}
