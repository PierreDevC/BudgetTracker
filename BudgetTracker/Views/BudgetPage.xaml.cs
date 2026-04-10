namespace BudgetTracker.Views;

public partial class BudgetPage : ContentPage
{
    // FAB state
    double _lastScrollY = 0;
    bool _isFabExpanded = true;
    double _expandedWidth = 0;

    // Sheet state
    double _sheetDragStartY = 0;

    public BudgetPage(ViewModels.BudgetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    // ── FAB scroll behaviour ──────────────────────────────────────────────────

    private void OnScrolled(object sender, ScrolledEventArgs e)
    {
        bool scrollingDown = e.ScrollY > _lastScrollY + 2;
        bool scrollingUp   = e.ScrollY < _lastScrollY - 2;
        _lastScrollY = e.ScrollY;

        if (scrollingDown && _isFabExpanded)
        {
            _isFabExpanded = false;
            CollapseFab();
        }
        else if (scrollingUp && !_isFabExpanded)
        {
            _isFabExpanded = true;
            ExpandFab();
        }
    }

    private void CollapseFab()
    {
        _expandedWidth = FabButton.Width;
        FabButton.Text = "+";

        var anim = new Animation(v => FabButton.WidthRequest = v, _expandedWidth, 56);
        anim.Commit(this, "FabCollapse", 16, 200, Easing.CubicInOut);
    }

    private void ExpandFab()
    {
        FabButton.Text = "+ Catégorie";

        var anim = new Animation(v => FabButton.WidthRequest = v, 56, _expandedWidth);
        anim.Commit(this, "FabExpand", 16, 200, Easing.CubicInOut,
            finished: (_, __) => FabButton.WidthRequest = -1);
    }

    // ── Sheet show / hide ─────────────────────────────────────────────────────

    private void OnFabClicked(object sender, EventArgs e) => ShowSheet();

    private void ShowSheet()
    {
        double startY = SheetPanel.Height > 0 ? SheetPanel.Height : 600;
        SheetPanel.TranslationY = startY;
        SheetOverlay.Opacity = 0;
        SheetOverlay.IsVisible = true;

        var combined = new Animation
        {
            { 0, 1, new Animation(v => SheetOverlay.Opacity    = v, 0, 1) },
            { 0, 1, new Animation(v => SheetPanel.TranslationY = v, startY, 0) },
        };
        combined.Commit(this, "ShowSheet", 16, 320, Easing.CubicOut);
    }

    private void HideSheet()
    {
        double endY = SheetPanel.Height > 0 ? SheetPanel.Height : 600;

        var combined = new Animation
        {
            { 0, 1, new Animation(v => SheetPanel.TranslationY = v, SheetPanel.TranslationY, endY) },
            { 0, 1, new Animation(v => SheetOverlay.Opacity    = v, SheetOverlay.Opacity, 0) },
        };
        combined.Commit(this, "HideSheet", 16, 260, Easing.CubicIn,
            finished: (_, __) =>
            {
                SheetOverlay.IsVisible = false;
                SheetPanel.TranslationY = 0;
                NameEntry.Text = "";
                AmountEntry.Text = "";
            });
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e) => HideSheet();
    private void OnCancelSheet(object sender, EventArgs e) => HideSheet();

    private void OnConfirmAdd(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!decimal.TryParse(AmountEntry.Text?.Trim(), out var amount) || amount <= 0) return;

        ((ViewModels.BudgetViewModel)BindingContext).AddCategory(name, amount);
        HideSheet();
    }

    // ── Drag to dismiss ───────────────────────────────────────────────────────

    private void OnSheetPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _sheetDragStartY = SheetPanel.TranslationY;
                break;

            case GestureStatus.Running:
                double newY = _sheetDragStartY + e.TotalY;
                if (newY >= 0)
                    SheetPanel.TranslationY = newY;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (SheetPanel.TranslationY > 120)
                {
                    HideSheet();
                }
                else
                {
                    var snap = new Animation(v => SheetPanel.TranslationY = v, SheetPanel.TranslationY, 0);
                    snap.Commit(this, "SnapBack", 16, 200, Easing.SpringOut);
                }
                break;
        }
    }
}
