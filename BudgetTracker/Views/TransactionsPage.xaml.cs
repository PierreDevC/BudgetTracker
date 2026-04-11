namespace BudgetTracker.Views;

public partial class TransactionsPage : ContentPage
{
    double _lastScrollY = 0;
    bool _isFabExpanded = true;
    double _expandedWidth = 0;

    public TransactionsPage(ViewModels.TransactionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as ViewModels.TransactionsViewModel)?.Refresh();
    }

    private void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        bool scrollingDown = e.VerticalOffset > _lastScrollY + 2;
        bool scrollingUp = e.VerticalOffset < _lastScrollY - 2;
        _lastScrollY = e.VerticalOffset;

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
        FabButton.Text = "+ Transaction";

        var anim = new Animation(v => FabButton.WidthRequest = v, 56, _expandedWidth);
        anim.Commit(this, "FabExpand", 16, 200, Easing.CubicInOut,
            finished: (_, __) => FabButton.WidthRequest = -1);
    }
}