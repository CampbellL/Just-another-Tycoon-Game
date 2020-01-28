using Economy;
using Game;
using UI.Xml;
using UnityEngine.UI;
using Worker;
using Zenject;

internal class GameUiController : XmlLayoutController
{
    #region WorkerPanelReferences

    private XmlElementReference<Text> _workerMoneyLabelReference;
    private XmlElementReference<XmlElement> _workerPanelReference;
    private XmlElementReference<XmlElement> _workerContentHolderReference;

    #endregion

    #region SpecPanelReferences

    private XmlElementReference<XmlElement> _moneyUpgradeButtonReference;
    private XmlElementReference<XmlElement> _specsPanelReference;

    #endregion

    private EconomyController _economyController;
    private PlayerStateService _playerStateService;
    private int _workerCarouselPosition = 0;
    private XmlElementReference<XmlElement> _activePanel;

    private XmlElementReference<XmlElement> _shopPanelReference;
    private XmlElementReference<XmlElement> _workerUnlockPanelReference;
    private XmlElementReference<XmlElement> _messageBoxReference;

    private Worker.Worker _unlockedWorkerCache;

    [Inject]
    private void PopulateDependencies(EconomyController economyController, PlayerStateService playerStateService)
    {
        this._economyController = economyController;
        this._playerStateService = playerStateService;
        this._economyController.OnMoneyChanged += () =>
        {
            if (this._workerMoneyLabelReference != null)
                this._workerMoneyLabelReference.element.text = this._economyController.Money.ToString("0.00");
            if (this._moneyUpgradeButtonReference == null) return;
            if (this._economyController.CanUpgradePercentageSkill()) return;
            this._moneyUpgradeButtonReference.element.SetAttribute("color", "red");
            this._moneyUpgradeButtonReference.element.ApplyAttributes();
        };
    }

    public override void LayoutRebuilt(ParseXmlResult parseResult)
    {
        _workerMoneyLabelReference = _workerMoneyLabelReference ?? this.XmlElementReference<Text>("moneyLabel");
        _workerPanelReference = _workerPanelReference ?? this.XmlElementReference<XmlElement>("workerPanel");
        _specsPanelReference = _specsPanelReference ?? this.XmlElementReference<XmlElement>("specsPanel");
        _shopPanelReference = _shopPanelReference ?? this.XmlElementReference<XmlElement>("shopPanel");
        _messageBoxReference = _messageBoxReference ?? this.XmlElementReference<XmlElement>("messageBox");
        _workerUnlockPanelReference = _workerUnlockPanelReference ?? this.XmlElementReference<XmlElement>("workerUnlockPanel");
        _moneyUpgradeButtonReference = _moneyUpgradeButtonReference ??
                                       this.XmlElementReference<XmlElement>("UpgradeMoneyPercentage");
    }

    #region WorkerPanel

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

    #endregion

    public void UpgradeMoneyPercentageSkillLevel()
    {
        if (this._economyController.CanUpgradePercentageSkill())
        {
            this._economyController.PurchasePercentageSkillUpgrade();
            this.SpecPanelUpdateInterface();
        }
        this._messageBoxReference.element.Show();
    }

    private void SpecPanelUpdateInterface()
    {
        this._moneyUpgradeButtonReference.element.SetAttribute("text",
            this._economyController.GetPercentageSkillUpgradeCost().ToString());
        this._moneyUpgradeButtonReference.element.ApplyAttributes();
    }

    public void ShowWorkers()
    {
        PopulateWorkerPanel();
        _activePanel?.element.Hide();
        this._workerPanelReference.element.Show();
        this._activePanel = this._workerPanelReference;
    }

    public void ShowSpecs()
    {
        this._activePanel?.element.Hide();
        this._specsPanelReference.element.Show();
        this.SpecPanelUpdateInterface();
        this._activePanel = this._specsPanelReference;
    }

    public void ShowShop()
    {
        this._activePanel?.element.Hide();
        this._shopPanelReference.element.Show();
        this._activePanel = this._shopPanelReference;
    }

    public void PopulateWorkerUnlockPanel(Worker.Worker worker)
    {
        if (this._playerStateService == null) return;
        XmlElement workerPanel = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerAvatarPanel");
        XmlElement costField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerCost");
        XmlElement nameField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerName");
        XmlElement workerBodyField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerBody");
        XmlElement workerFaceField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerFace");
        XmlElement workerHairField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerHair");
        XmlElement workerKitField = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerKit");
        var stars = _workerUnlockPanelReference.element.GetElementByInternalId<XmlElement>("workerEfficiency").childElements;
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

    public void OpenWorkerLootBox()
    {
        if (this._economyController.Money > 1000)
        {
            this._economyController.Purchase(1000);
            this._unlockedWorkerCache = WorkerController.GenerateRandomWorker();
            this.PopulateWorkerUnlockPanel(this._unlockedWorkerCache);
            this._workerUnlockPanelReference.element.Show();
        }
        this._messageBoxReference.element.Show();
    }

    public void DiscardWorker()
    {
        this._unlockedWorkerCache = null;
        this._workerUnlockPanelReference.element.Hide();
    }
    
    public void HireWorker()
    {
        this._playerStateService.HireWorker(this._unlockedWorkerCache);
        this._workerUnlockPanelReference.element.Hide();
    }

    public void CloseMessageBox()
    {
        this._messageBoxReference.element.Hide();
    }
}