using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using iTunesSearch.Library;

namespace Avalonia.MusicStore.Models;

public class Album
{
    private static HttpClient _httpClient = new();
    private static iTunesSearchManager _searchManager = new();


    public Album(string title, string artist, string coverUrl)
    {
        Title = title;
        Artist = artist;
        CoverUrl = coverUrl;
    }

    public string Artist { get; set; }
    public string Title { get; set; }
    public string CoverUrl { get; set; }

    private static string _cacheDirectory = "./Cache";
    private static string BoughtAlbumsFile => $"{_cacheDirectory}/___BOUGHT__ALBUMS___";

    private string CachePath => $"{_cacheDirectory}/{Artist}_{Title}";
    private string BmpPath => CachePath + ".bmp";


    public async Task<Stream> LoadCoverBitmapAsync()
    {
        if (File.Exists(BmpPath))
        {
            return File.OpenRead(BmpPath);
        }
        else
        {
            var data = await _httpClient.GetByteArrayAsync(CoverUrl);
            return new MemoryStream(data);
        }
    }

    public async Task SaveAsync()
    {
        CreateDirectoryIfNotExists();

        using (var fs = File.OpenWrite(CachePath))
        {
            await SaveToStreamAsync(this, fs);
        }
    }

    public Stream SaveCoverBitmapStream()
    {
        return File.OpenWrite(BmpPath);
    }

    private static async Task SaveToStreamAsync(Album album, Stream stream)
    {
        await JsonSerializer.SerializeAsync(stream, album).ConfigureAwait(false);
    }

    public static async Task<Album> LoadFromStream(Stream stream)
    {
        return (await JsonSerializer.DeserializeAsync<Album>(stream).ConfigureAwait(false))!;
    }


    public static async Task<IEnumerable<Album>> LoadCachedAsync()
    {
        CreateDirectoryIfNotExists();

        var results = new List<Album>();

        foreach (var file in Directory.EnumerateFiles(_cacheDirectory))
        {
            if (!string.IsNullOrWhiteSpace(new DirectoryInfo(file).Extension)) continue;

            await using var fs = File.OpenRead(file);
            results.Add(await Album.LoadFromStream(fs).ConfigureAwait(false));
        }

        return results;
    }

    public static async Task<IEnumerable<Album>> SearchAsync(string searchTerm)
    {
        var query = await _searchManager.GetAlbumsAsync(searchTerm).ConfigureAwait(false);

        return query.Albums.Select(x =>
            new Album(x.ArtistName, x.CollectionName, x.ArtworkUrl100.Replace("100x100bb", "600x600bb")));
    }


    private static void CreateDirectoryIfNotExists()
    {
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

}