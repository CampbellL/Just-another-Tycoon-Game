using Economy;
using Game;
using UI.Xml;
using UnityEngine.UI;
using Zenject;

internal class GameUiController : XmlLayoutController
{
    private XmlElementReference<Text> _moneyLabelReference;
    private XmlElementReference<XmlElement> _workerPanelReference;
    private XmlElementReference<XmlElement> _specsPanelReference;
    private XmlElementReference<XmlElement> _workerContentHolderReference;
    private EconomyController _economyController;
    private PlayerStateService _playerStateService;
    private int _workerCarouselPosition = 0;
    private XmlElementReference<XmlElement> _activePanel;

    [Inject]
    private void PopulateDependencies(EconomyController economyController, PlayerStateService playerStateService)
    {
        this._economyController = economyController;
        this._playerStateService = playerStateService;
        this._economyController.OnMoneyChanged += () =>
        {
            if (this._moneyLabelReference != null)
                this._moneyLabelReference.element.text = this._economyController.Money.ToString("0.00");
        };
    }

    private void PopulateWorkerPanel()
    {
        if (this._playerStateService == null) return;
        XmlElement workerPanel = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerAvatarPanel");
        XmlElement costField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerCost");
        XmlElement nameField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerName");
        XmlElement workerBodyField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerBody");
        XmlElement workerFaceField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerFace");
        XmlElement workerHairField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerHair");
        XmlElement workerKitField = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerKit");
        var stars = _workerPanelReference.element.GetElementByInternalId<XmlElement>("workerEfficiency").childElements;
        Worker.Worker worker = this._playerStateService.GetWorkers()[_workerCarouselPosition];
        nameField.SetAttribute("text", worker.Name);
        costField.SetAttribute("text", worker.Cost.ToString("0.00"));
        workerBodyField.SetAttribute("image", $"Faces/Bodies/Body_{worker.BodyType}");
        workerFaceField.SetAttribute("image", $"Faces/Faces/Face_{worker.FaceType}");
        workerHairField.SetAttribute("image", $"Faces/Hairs/Hair_{worker.HairType}");
        workerKitField.SetAttribute("image", $"Faces/Kits/Kit_{worker.KitType}");
        workerPanel.Show();
        for (var index = 1; index < stars.Count; index++)
        {
            XmlElement image = stars[index];
            image.SetAttribute("active", "true");
            if (index >= worker.Efficiency)
            {
                image.SetAttribute("active", "false");
            }
            image.ApplyAttributes();
        }
        nameField.ApplyAttributes();
        costField.ApplyAttributes();
        workerBodyField.ApplyAttributes();
        workerFaceField.ApplyAttributes();
        workerHairField.ApplyAttributes();
        workerKitField.ApplyAttributes();
    }

    public void WorkerCarouselLeft()
    {
        this._workerCarouselPosition--;
        if (this._workerCarouselPosition < 0)
        {
            this._workerCarouselPosition = this._playerStateService.GetWorkers().Count - 1;
        }
        PopulateWorkerPanel();
    }

    public void WorkerCarouselRight()
    {
        this._workerCarouselPosition++;
        if (this._workerCarouselPosition >= this._playerStateService.GetWorkers().Count)
        {
            this._workerCarouselPosition = 0;
        }
        PopulateWorkerPanel();
    }
    
    public override void LayoutRebuilt(ParseXmlResult parseResult)
    {
        _moneyLabelReference = _moneyLabelReference ?? this.XmlElementReference<Text>("moneyLabel");
        _workerPanelReference = _workerPanelReference ?? this.XmlElementReference<XmlElement>("workerPanel");
        _specsPanelReference = _specsPanelReference ?? this.XmlElementReference<XmlElement>("specsPanel");
        PopulateWorkerPanel();
        _workerPanelReference.element.Hide();
        _specsPanelReference.element.Hide();
    }

    public void ShowWorkers()
    {
        _activePanel?.element.Hide();
        this._workerPanelReference.element.Show();
        this._activePanel = this._workerPanelReference;
    }
    
    public void ShowSpecs()
    {
        this._activePanel?.element.Hide();
        this._specsPanelReference.element.Show();
        this._activePanel = this._workerPanelReference;
    }
}