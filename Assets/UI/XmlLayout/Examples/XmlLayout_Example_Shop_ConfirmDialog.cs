using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Examples
{    
    class XmlLayout_Example_Shop_ConfirmDialog : XmlLayoutController
    {
        ExampleProduct product = null;
        Action<ExampleProduct> callback = null;                

        public void Show(ExampleProduct product, Action<ExampleProduct> callback = null)
        {
            xmlLayout.Show();

            this.product = product;
            this.callback = callback;

            // Because this dialog may not have been active yet at this point,
            // we need to wait a frame to make sure that the XmlLayout has finished setting up            
            StartCoroutine(DelayedShow());
        }

        protected IEnumerator DelayedShow()
        {
            while(!xmlLayout.IsReady) yield return null;

            xmlLayout.GetElementById<Image>("productImage").sprite = product.Image;
            xmlLayout.GetElementById<Text>("productQuantity").text = String.Format("x{0}", product.Quantity);
            xmlLayout.GetElementById<Text>("productPrice").text = String.Format("${0}", product.Price);
        }

        void ConfirmPurchase()
        {
            callback(product);

            Hide();
        }        
    }
}
