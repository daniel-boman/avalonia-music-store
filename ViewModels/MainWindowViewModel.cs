using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using Avalonia.MusicStore.Models;
using ReactiveUI;

namespace Avalonia.MusicStore.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();

        BuyMusicCommand = ReactiveCommand.Create(async () =>
        {
            var store = new MusicStoreViewModel();

            var result = await ShowDialog.Handle(store);

            if (result != null)
            {
                Albums.Add(result);

                await result.SaveToDiskAsync();
            }
        });


        this.WhenAnyValue(x => x.Albums.Count).Subscribe(x => CollectionEmpty = x == 0);

        RxApp.MainThreadScheduler.Schedule(LoadAlbums);
    }


    private bool _collectionEmpty;

    public bool CollectionEmpty
    {
        get => _collectionEmpty;
        set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
    }

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    public ICommand BuyMusicCommand { get; }

    public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }


    private async void LoadAlbums()
    {
        var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

        foreach (var album in albums)
        {
            Albums.Add(album);
        }

        foreach (var album in Albums.ToList())
        {
            await album.LoadCover();
        }
    }
}