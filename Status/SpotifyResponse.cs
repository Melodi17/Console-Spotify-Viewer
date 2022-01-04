using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Status
{
    public class SpotifyExternalUrls
    {
        public string spotify { get; set; }
    }

    public class SpotifyArtist
    {
        public SpotifyExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class SpotifyImage
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class SpotifyAlbum
    {
        public string album_type { get; set; }
        public List<SpotifyArtist> artists { get; set; }
        public List<string> available_markets { get; set; }
        public SpotifyExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<SpotifyImage> images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public int total_tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class SpotifyExternalIds
    {
        public string isrc { get; set; }
    }

    public class SpotifyItem
    {
        public SpotifyAlbum album { get; set; }
        public List<SpotifyArtist> artists { get; set; }
        public List<string> available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool @explicit { get; set; }
        public SpotifyExternalIds external_ids { get; set; }
        public SpotifyExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class SpotifyDisallows
    {
        public bool pausing { get; set; }
    }

    public class SpotifyActions
    {
        public SpotifyDisallows disallows { get; set; }
    }

    public class SpotifyRoot
    {
        public long timestamp { get; set; }
        public object context { get; set; }
        public int progress_ms { get; set; }
        public SpotifyItem item { get; set; }
        public string currently_playing_type { get; set; }
        public SpotifyActions actions { get; set; }
        public bool is_playing { get; set; }

        public static SpotifyRoot Parse(string str)
        {
            return JsonConvert.DeserializeObject<SpotifyRoot>(str);
        }
    }


}
