using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NotifierServiece
{
    public delegate void EventHandler(List<MediaContent> mediaContents);
    public delegate void EventHandlerMediaContent(MediaContent mediaContent);
    public class UpdateManager
    {
        public event EventHandler onCollectionChaned;
        public event EventHandlerMediaContent onMediaContentChanged;

        string mediaDBFileName;
        CancellationTokenSource watcherCancellationToken;
        List<MediaSource> mediaDB;
        object _fileLock;

        List<MediaContent> mediaContentsUI
        {
            get
            {
                var mediaContentsUI = new List<MediaContent>();

                mediaDB.ForEach(mediaSource => mediaSource.ContentList.ForEach(mediaContent => mediaContentsUI.Add(mediaContent)));

                mediaContentsUI.Sort((a, b) => a.CompareTo(b));

                return mediaContentsUI;
            }
        }

        static UpdateManager updateManager;
        static public UpdateManager GetUpdateManager
        {
            get
            {
                if (updateManager == null)
                    updateManager = new UpdateManager();
                return updateManager;
            }
        }

        public MediaContent GetMediaContent(Guid id)
        {
            MediaContent mediaContent = null;
            foreach (var mediaSource in mediaDB)
            {
                mediaContent = mediaSource.TryGetMaidaContentById(id);
                if (mediaContent != null) break;
            }
            return mediaContent;
        }

        public void StartWathcer()
        {
            watcherCancellationToken = new CancellationTokenSource();
            CancellationToken ct = watcherCancellationToken.Token;

            Task.Factory.StartNew(() =>
            {
                var counter = 0;

                while (!watcherCancellationToken.IsCancellationRequested)
                {
                    Update(mediaDB, counter);
                    counter++;
                    onCollectionChaned(mediaContentsUI);
                    Task.Delay(TimeSpan.FromMinutes(15)).Wait();
                }

            }, ct);

            onCollectionChaned(mediaContentsUI);
        }

        public void StopWatcher()
        {
            if (watcherCancellationToken != null)
                watcherCancellationToken.Cancel();

            watcherCancellationToken = null;
        }

        public bool TryToAddUrl(string url)
        {
            var result = false;
            var mediaSource = mediaDB.FindLast(x => url.StartsWith(x.HomePageUrl));

            if (mediaSource != null)
            {
                using (WebClient client = new WebClient())
                {
                    string htmlCode = client.DownloadString(url);

                    var regExpResult = mediaSource.RegexrForName.Match(htmlCode);
                    if (regExpResult.Success && regExpResult.Groups.Count > 0)
                    {
                        var name = regExpResult.Groups[1].Value;
                        var mediaContent = new MediaContent(url, name);
                        if (mediaSource.TryAddMediaContent(mediaContent))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                UpdateMediaContent(mediaSource, mediaContent);
                                onCollectionChaned(mediaContentsUI);
                            });

                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        public void UpdateUserCollection(List<MediaContent> mediaContents)
        {
            mediaContents.Sort((a, b) => b.CompareTo(a));
        }

        public bool TryToRemoveMediaContent(MediaContent mediaContent)
        {
            foreach (var mediaSource in mediaDB)
            {
                if (mediaSource.TryToDeleteMediaContent(mediaContent))
                {
                    Task.Factory.StartNew(() => 
                    { 
                        onCollectionChaned(mediaContentsUI);
                    });
                    return true;
                }
            }
            return false;
        }

        private UpdateManager(string fileName = "UserMediaDB.json")
        {
            _fileLock = new object();

            onCollectionChaned += UpdateUserCollection;
            onCollectionChaned += (x) => { SaveMediaDB(mediaDB); };

            mediaDBFileName = fileName;

            if (!File.Exists(mediaDBFileName))
            {
                SaveMediaDB(GenerateDefaultMediaSource());
            }

            mediaDB = JsonConvert.DeserializeObject<List<MediaSource>>(File.ReadAllText(mediaDBFileName));

            updateMediaSources(mediaDB);
        }

        void Update(IEnumerable<MediaSource> mediaDB, int counter)
        {
            foreach (var mediaSource in mediaDB)
            {
                foreach (var mediaContent in mediaSource.ContentList)
                {
                    if (counter % (int)mediaContent.UpdateFrequency != 0 && counter != 0)
                        continue;
                    
                    UpdateMediaContent(mediaSource, mediaContent);

                    Thread.Sleep(1000);
                }
            }
        }

        void UpdateMediaContent(MediaSource mediaSource, MediaContent mediaContent)
        {
            using (WebClient client = new WebClient() { Encoding = System.Text.Encoding.UTF8 })
            {
                string htmlCode = client.DownloadString(mediaContent.PageUrl);
                string lastResult = null;

                foreach (var regex in mediaSource.RegExrForDataList)
                {
                    var regExpResult = regex.Match(lastResult ?? htmlCode);

                    if (regExpResult.Success && regExpResult.Groups.Count > 0)
                    {
                        lastResult = regExpResult.Groups[1].Value;
                    }
                    else
                    {
                        Console.WriteLine("Problem...");
                        break;
                    }
                }

                if (mediaContent.Status != lastResult)
                {
                    mediaContent.Status = lastResult;
                    mediaContent.LastUpdated = DateTime.Now;
                    mediaContent.WasUpdated = true;

                    onMediaContentChanged(mediaContent);
                }
                else
                {
                    //Console.WriteLine("Nothing :(");
                }
            }
        }

        void SaveMediaDB(IEnumerable<MediaSource> mediaDB)
        {
            lock (_fileLock)
            {
                using (StreamWriter writer = File.CreateText(mediaDBFileName))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(mediaDB));
                }
            }
        }

        List<MediaSource> GenerateDefaultMediaSource()
        {
            MediaSource defaultSource1 = new MediaSource("https://animevost.org");

            var regExpListForSource1 = new List<Regex>() {
                new Regex(@"<title>([^<]*)<\/title>"),
                new Regex(@".*?\d+[-| ](\d+)")
            };
            defaultSource1.RegExrForDataList.AddRange(regExpListForSource1);

            defaultSource1.RegexrForName = new Regex(@"<title>([^\/]*)");


            MediaSource defaultSource2 = new MediaSource("https://anistar.org");

            var regExpListForSource2 = new List<Regex>() {
                new Regex(@"<div class=""descripts\"">[^<]*<p[^<]*?(\d+)")
            };
            defaultSource2.RegExrForDataList.AddRange(regExpListForSource2);

            defaultSource2.RegexrForName = new Regex(@"<title>[^\w]*([\w\s\]\[]*)");


            return new List<MediaSource>() { defaultSource1, defaultSource2 };
        }

        void updateMediaSources(List<MediaSource> mediaSources)
        {
            GenerateDefaultMediaSource().ForEach(defaultMediaSource =>
            {
                var mediaSource = mediaDB.FindLast(mediaSource => mediaSource.HomePageUrl.Equals(defaultMediaSource.HomePageUrl));
                if (mediaSource != null)
                {
                    mediaSource.RegExrForDataList = defaultMediaSource.RegExrForDataList;
                    mediaSource.RegexrForName = defaultMediaSource.RegexrForName;
                }
                else
                {
                    mediaSources.Add(defaultMediaSource);
                }
            });
        }
    }
}
