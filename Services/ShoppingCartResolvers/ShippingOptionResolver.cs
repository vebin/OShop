﻿using Orchard.Environment.Extensions;
using OShop.Helpers;
using OShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OShop.Services.ShoppingCartResolvers {
    [OrchardFeature("OShop.Shipping")]
    public class ShippingOptionResolver : IShoppingCartResolver {
        private readonly IShippingService _shippingService;

        public ShippingOptionResolver(
            IShippingService shippingService) {
            _shippingService = shippingService;
        }

        public Int32 Priority {
            get { return 10; }
        }

        public void ResolveCart(IShoppingCartService ShoppingCartService, ref ShoppingCart Cart) {
            if (!Cart.IsShippingRequired()) {
                return;
            }

            if (Cart.ShippingZone == null) {
                // Need a shipping zone
                Cart.InvalidCart();
            }

            var suitableProviders = _shippingService.GetSuitableProviderOptions(Cart);

            if (!suitableProviders.Any()) {
                // Need a suitable shipping provider
                Cart.InvalidCart();
                return;
            }

            Int32 selectedProviderId = ShoppingCartService.GetProperty<int>("ShippingProviderId");
            var selectedProvider = suitableProviders.Where(p => p.Provider.Id == selectedProviderId).FirstOrDefault();
            if (selectedProvider != null) {
                // Apply selected provider
                Cart.ShippingOption = selectedProvider;
            }
            else {
                // Set cheapest option
                Cart.ShippingOption = suitableProviders.OrderBy(p => p.Option.Price).FirstOrDefault();
            }

        }
    }
}