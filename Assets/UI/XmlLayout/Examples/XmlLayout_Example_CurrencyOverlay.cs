using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Examples
{    
    class XmlLayout_Example_CurrencyOverlay : XmlLayoutController
    {
        [SerializeField]
        public List<ExampleProduct> CurrencyQuantities = new List<ExampleProduct>()
        {
            new ExampleProduct() { Name = "Coins", Quantity = 0 },
            new ExampleProduct() { Name = "Green Gems", Quantity = 0 },
            new ExampleProduct() { Name = "Blue Gems", Quantity = 0 },
            new ExampleProduct() { Name = "Red Gems", Quantity = 0 }
        };        

        public void AddCurrency(ExampleProduct productPurchased)
        {
            var currency = CurrencyQuantities.First(c => c.Name == productPurchased.Name);

            currency.Quantity += productPurchased.Quantity;

            UpdateDisplay();
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            for (var x = 0; x < CurrencyQuantities.Count; x++)
            {
                var text = xmlLayout.GetElementById<Text>(x.ToString());

                text.text = String.Format("x{0}", CurrencyQuantities[x].Quantity);
            }
        }        
    }
}
