﻿using BiliDuang.UI;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BiliDuang
{
    class Video
    {
        public int Type;
        public List<VideoClass.AV> av = new List<VideoClass.AV>();
        public VideoClass.SS ss;

        public Video(string vlink)
        {
            //第一步,bilibili网址转换
            //注意垃圾spm!
            //例如 https://www.bilibili.com/bangumi/play/ss28615/?spm=3.0212
            try
            {
                Uri uri = new Uri(vlink);
                if (uri.Host != "space.bilibili.com")
                {
                    vlink = uri.AbsolutePath;
                    vlink = vlink.TrimEnd('/');
                    vlink = vlink.Substring(vlink.LastIndexOf("/") + 1, vlink.Length - vlink.LastIndexOf("/") - 1);
                }
                else
                {
                    vlink = uri.AbsolutePath;
                    vlink = vlink.TrimEnd('/');
                    vlink = vlink.Substring(vlink.LastIndexOf("/") + 1, vlink.Length - vlink.LastIndexOf("/") - 1);
                    int uid = 0;
                    if (int.TryParse(vlink, out uid))
                    {
                        ProcessUid(vlink);
                        return;
                    }
                    else
                    {
                        Dialog.Show("用户ID错误,请检查链接格式!");
                        return;
                    }

                }

            }
            catch (UriFormatException e)
            {
                if (vlink.Contains("/"))
                {
                    Dialog.Show("链接格式错误!请复制完整的链接!");
                    return;
                }
            }


            //第二步判断格式
            if (vlink.Contains("BV"))
            {
                Type = VideoType.AV;
                vlink = "av" + Video.ProcessBV(vlink);
            }
            vlink = vlink.ToLower();
            if (vlink.Contains("av"))
            {
                Type = VideoType.AV;
                ProcessAV(vlink.Replace("av", ""));
            }
            else if (vlink.Contains("ep"))
            {
                Type = VideoType.SS;
                ProcessEP(vlink.Replace("ep", ""));
            }
            else if (vlink.Contains("ss"))
            {
                Type = VideoType.SS;
                ProcessSS(vlink.Replace("ss", ""));
            }
            else if (vlink.Contains("md"))
            {
                ProcessMD(vlink.Replace("md", ""));
            }
            else if (vlink.Contains("ml"))
            {
                ProcessML(vlink.Replace("ml", ""));
            }
            else if (vlink.Contains("uid"))
            {
                ProcessUid(vlink.Replace("uid", ""));
            }
            else
            {
                Dialog.Show("不是可以获取的格式,请检查格式是否正确");
            }
        }

        public static string ProcessBV(string v)
        {
            //https://api.bilibili.com/x/web-interface/view?bvid=BV187411m7eL
            WebClient wc = new WebClient();
            string ret = Encoding.UTF8.GetString(wc.DownloadData("https://api.bilibili.com/x/web-interface/view?bvid=" + v));
            JSONCallback.AV.AV data = Newtonsoft.Json.JsonConvert.DeserializeObject<JSONCallback.AV.AV>(ret);
            if (data.code == 0)
                return data.data.aid;
            else
                return "0";
        }

        private void ProcessML(string v)
        {
            LikeSelect likeSelect = new LikeSelect(v);
            likeSelect.ShowDialog();
        }

        private void ProcessUid(string uid)
        {
            UserInfoForm uif = new UserInfoForm(uid);
            uif.ShowDialog();
        }

        private bool ProcessAV(string avid)
        {
            VideoClass.AV nav = new VideoClass.AV(avid);
            if (nav.isbangumi)
            {
                string vlink = nav.bangumiurl;
                vlink = Other.TextGetCenter("play/", "/", vlink, vlink);
                if (vlink.Contains("ep"))
                {
                    Type = VideoType.SS;
                    ProcessEP(vlink.Replace("ep", ""));
                    return true;
                }
            }
            av.Add(nav);
            return true;
        }

        private bool ProcessSS(string vlink)
        {
            VideoClass.SS nss = new VideoClass.SS(vlink);
            ss = nss;
            return true;
        }

        private bool ProcessEP(string vlink)
        {
            VideoClass.EP ep = new VideoClass.EP(vlink);
            VideoClass.SS nss = new VideoClass.SS(ep.ssid);
            ss = nss;
            return true;
        }

        private bool ProcessMD(string vlink)
        {
            string callback = Other.GetHtml("https://www.bilibili.com/bangumi/media/md" + vlink);
            string ssid = Other.TextGetCenter("\"season_id\":", ",", callback);
            Type = VideoType.SS;
            ProcessSS(ssid);
            return true;
        }



    }
}
