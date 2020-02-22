using System;
using System.Collections.Generic;
using System.Text;

namespace NotifierServiece
{
    public enum UpdateFrequency { Low = 5, Medium = 3, Height = 1 }
    public class MediaContent
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string PageUrl { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool WasUpdated { get; set; }
        public UpdateFrequency UpdateFrequency { get; set; }
        public string Status { get; set; }
        public MediaContent(string pageUrl, string name = "...", UpdateFrequency updateFrequency = UpdateFrequency.Height)
        {
            Id = Guid.NewGuid();
            PageUrl = pageUrl;
            Name = name;
            LastUpdated = DateTime.MinValue;
            WasUpdated = false;
            UpdateFrequency = updateFrequency;
            Status = "No Data";
        }
        public int CompareTo(MediaContent mediaContent)
        {
            return LastUpdated.CompareTo(mediaContent.LastUpdated);
        }
    }
}
