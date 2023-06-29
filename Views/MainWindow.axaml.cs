using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.MusicStore.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Avalonia.MusicStore.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(InteractionContext<MusicStoreViewModel, AlbumViewModel?> interactionContext)
    {
        var dialog = new MusicStoreWindow
        {
            DataContext = interactionContext.Input
        };

        var result = await dialog.ShowDialog<AlbumViewModel?>(this);

        interactionContext.SetOutput(result);
    }
}