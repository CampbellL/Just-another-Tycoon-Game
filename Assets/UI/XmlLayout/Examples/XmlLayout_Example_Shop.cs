using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UI.Tables;

namespace UI.Xml.Examples
{    
    class XmlLayout_Example_Shop : XmlLayoutController
    {
        public XmlLayout_Example_Shop_ConfirmDialog ConfirmPurchaseDialog = null;
        public XmlLayout_Example_CurrencyOverlay CurrencyOverlay = null;

        [SerializeField]
        public List<ExampleProduct> Products = new List<ExampleProduct>();        

        void OnEnable()
        {
            CurrencyOverlay.Show();            
        }

        void OnDisable()
        {
            CurrencyOverlay.Hide();
        }

        void OnValidate()
        {
            if (!this.gameObject.activeInHierarchy) return;

            if (Application.isPlaying)
            {
                StartCoroutine(Rebuild());
            }            
        }

        public IEnumerator Rebuild()
        {
            yield return new WaitForEndOfFrame();
            xmlLayout.RebuildLayout();
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {            
            if (parseResult != ParseXmlResult.Changed || Products == null || !Products.Any()) return;

            var shopContent = xmlLayout.GetElementById<TableLayout>("shopContent");
            var itemTemplate = xmlLayout.GetElementById("productTemplate");

            var columnCount = 4;
            var column = 0;
            var rows = shopContent.Rows.ToList();
            var productCount = Products.Count;

            var rowHeight = rows.First().preferredHeight;

            TableRow row;
            if (rows.Any())
            {
                row = rows.Last();
            }
            else
            {
                row = shopContent.AddRow(0);
                row.preferredHeight = rowHeight;
            }            
            
            for(var x = 0; x < productCount; x++)
            {
                var product = Products[x];

                var item = GameObject.Instantiate(itemTemplate);                
                item.Initialise(xmlLayout, (RectTransform)item.transform, itemTemplate.tagHandler);

                item.gameObject.SetActive(true); // the template is inactive so as not to show up, so we need to activate our new object

                // Add a new cell to the row (containing our new product)
                row.AddCell(item.rectTransform);                

                HandleProduct(product, item, x);

                // increment column count
                column++;

                // move to the next row, if necessary
                if (column == columnCount && (x + 1) < productCount)
                {
                    column = 0;
                    row = shopContent.AddRow(0);
                    row.preferredHeight = rowHeight;
                }
            }            
        }

        void HandleProduct(ExampleProduct product, XmlElement item, int productId)
        {
            var image = item.GetElementByInternalId<Image>("productImage");            
            if (product.Image != null)
            {
                image.sprite = product.Image;
            }
            image.color = Color.white;

            var button = item.GetElementByInternalId<Button>("productBuyButton");         
            button.GetComponentInChildren<Text>().text = String.Format("${0}", product.Price);
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { PurchaseButtonClicked(productId); }));            

            var productQuantity = item.GetElementByInternalId<Text>("productQuantity");
            productQuantity.text = String.Format("x{0}", product.Quantity);

            if (product.IsBestDeal)
            {
                var ribbon = item.GetElementByInternalId<Image>("productBestDeal");
                ribbon.gameObject.SetActive(true);
            }
        }

        void PurchaseButtonClicked(int productId)
        {
            var product = Products[productId];

            ConfirmPurchaseDialog.Show(product, PurchaseConfirmed);
        }

        public void PurchaseConfirmed(ExampleProduct product)
        {
            CurrencyOverlay.AddCurrency(product);
        }        
    }    

    [Serializable]
    public class ExampleProduct
    {
        public float Price = 0;
        public string Name = "";
        public int Quantity;
        public Sprite Image;
        public bool IsBestDeal = false;
    }
}
