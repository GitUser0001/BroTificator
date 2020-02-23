﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NotifierServiece
{
    public class MediaSource
    {
        //danya cheburek
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string HomePageUrl { get; set; }
        public List<MediaContent> ContentList { get; set; }
        public Regex RegexrForName { get; set; }
        public List<Regex> RegExrForDataList { get; set; }

        public bool TryAddMediaContent(MediaContent mediaContent)
        {
            if (mediaContent.PageUrl.StartsWith(HomePageUrl) && ContentList.FindLast(x => x.PageUrl == mediaContent.PageUrl) == null)
            {
                ContentList.Add(mediaContent);
                return true;
            }
            return false;
        }

        public bool TryToDeleteMediaContent(MediaContent mediaContent)
        {
            return ContentList.Remove(mediaContent);
        }

        public MediaContent TryGetMaidaContentById(Guid id)
        {
            return ContentList.FindLast(x => x.Id.Equals(id));
        }

        public MediaSource(string homePageUrl, string name = "")
        {
            Id = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(name) && homePageUrl.Split("//").Length > 1)
                Name = homePageUrl.Split("//")[1].Split(".")[0];
            else
                Name = string.IsNullOrWhiteSpace(name) ? homePageUrl : name;

            if (string.IsNullOrEmpty(homePageUrl))
                throw new ArgumentException("Invalid home page URL");

            HomePageUrl = homePageUrl;
            ContentList = new List<MediaContent>();
            RegExrForDataList = new List<Regex>();
        }
    }
}
