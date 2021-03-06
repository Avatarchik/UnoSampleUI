﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnoSampleUI.Commons;
using UnoSampleUI.Models;
using UnoSampleUI.ViewModels;
using Windows.Storage;
using Windows.Web.Syndication;

namespace UnoSampleUI.Services
{
    public class RssService
    {
        private const string BAD_URL_MESSAGE = "Hmm... Are you sure this is an RSS URL?";
        private const string NO_REFRESH_MESSAGE = "Sorry. We can't get more articles right now.";

        //public static async Task<IList<ArticleModel>> GetArticles(Uri rssLink, CancellationToken? cancellationToken = null)
        //{
        //    var feed = await new SyndicationClient().RetrieveFeedAsync(rssLink);
        //    if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return null;
        //    var articles = feed.Items.Select(item => new ArticleModel
        //    {
        //        Title = item.Title.Text,
        //        Summary = item.Summary == null 
        //            ? string.Empty 
        //            : item.Summary.Text.RegexRemove("\\&.{0,4}\\;").RegexRemove("<.*?>"),
        //        Author = item.Authors.Select(a => a.NodeValue).FirstOrDefault(),
        //        Link = item.ItemUri ?? item.Links.Select(l => l.Uri).FirstOrDefault(),
        //        PublishedDate = item.PublishedDate
        //    })
        //    .ToList();
        //    return articles;
        //}

        /// <summary>
        /// Retrieves feed data from the server and updates the appropriate FeedViewModel properties.
        /// </summary>
        public static async Task<RSSChannel> GetFeedAsync(Uri rssLink, CancellationToken? cancellationToken = null)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    string result = await hc.GetStringAsync(rssLink);

                    RSSChannel feed = GetChannelFromString(result);

                    if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    {
                        return null;
                    }
                    return feed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (!cancellationToken.HasValue || !cancellationToken.Value.IsCancellationRequested)
                {
                    //feedViewModel.IsInError = true;
                    //feedViewModel.ErrorMessage = feedViewModel.Articles.Count == 0 ? BAD_URL_MESSAGE : NO_REFRESH_MESSAGE;
                }
                return null;
            }
        }

        public static RSSChannel GetChannelFromString(string xml)
        {
            var xdoc = new XmlDocument();
            xdoc.LoadXml(xml);

            var channel = xdoc.SelectSingleNode("rss/channel");
            var returnValue = new RSSChannel
            {
                Title = channel.SelectSingleNode("title").GetString(),
                Link = channel.SelectSingleNode("link").GetString(),
                Description = channel.SelectSingleNode("description").GetString(),
                Pubdate = channel.SelectSingleNode("pubDate").GetDateTime(),
                Items = new List<RSSItem>()
            };

            var items = xdoc.SelectNodes("rss/channel/item");
            foreach (XmlNode item in items)
            {
                returnValue.Items.Add(new RSSItem 
                {
                    Title = item.SelectSingleNode("title").GetString(),
                    ItemLink = item.SelectSingleNode("link").GetString(),
                    Description = Strip(item.SelectSingleNode("description").GetString()),
                    Pubdate = item.SelectSingleNode("pubDate").GetDateTime()
                });
            }
            return returnValue;
        }

        //public static RSSChannel GetFeedFromString(string result)
        //{
        //    XDocument xdoc = XDocument.Parse(result);
        //    RSSChannel feed = (from channel in xdoc.Descendants("channel")
        //                       let hasImage = string.IsNullOrEmpty(channel.Element("image").GetString()) == true ? false : true
        //                       select new RSSChannel()
        //                       {
        //                           Title = channel.Element("title").GetString(),
        //                           Link = channel.Element("link").GetString(),
        //                           Description = channel.Element("description").GetString(),
        //                           Pubdate = channel.Element("pubDate").GetDateTime(),
        //                           Language = channel.Element("language").GetString(),
        //                           Copyright = channel.Element("copyright").GetString(),
        //                           Webmaster = channel.Element("webMaster").GetString(),
        //                           Generator = channel.Element("generator").GetString(),
        //                           Docs = channel.Element("docs").GetString(),
        //                           Ttl = channel.Element("ttl").GetInt(),
        //                           Image = hasImage ? new RSSImage()
        //                           {
        //                               Url = channel.Element("image").Element("url").GetString(),
        //                               Title = channel.Element("image").Element("title").GetString(),
        //                               Link = channel.Element("image").Element("link").GetString(),
        //                           } : null,
        //                           Items = new List<RSSItem>()
        //                       }).FirstOrDefault();

        //    (from item in xdoc.Descendants("item")
        //     let hasImage = string.IsNullOrEmpty(item.Element("image").GetString()) == true ? false : true
        //     select new RSSItem()
        //     {
        //         Title = item.Element("title").GetString(),
        //         ItemLink = item.Element("link").GetString(),
        //         Description = Strip(item.Element("description").GetString()),
        //         Category = item.Element("category").GetString(),
        //         Pubdate = item.Element("pubDate").GetDateTime(),
        //         ImageLink = hasImage ? item.Element("image").Element("link").GetString() : item.Element("link").GetString(),
        //         ImageTitle = hasImage ? item.Element("image").Element("title").GetString() : item.Element("title").GetString(),
        //         ImageUrl = hasImage ? item.Element("image").Element("url").GetString() : null,
        //     })
        //        .ToList()
        //        .ForEach(i => feed.Items.Add(i));
        //    return feed;
        //}

        private static string Strip(string text)
        {
            var returnValue = System.Text.RegularExpressions.Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
            return returnValue;
            //#if NETFX_CORE
            //            return returnValue.Substring(0,50);
            //#else
            //            return returnValue;
            //#endif

        }
    }
}
