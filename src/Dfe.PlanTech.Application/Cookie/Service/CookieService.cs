﻿using Dfe.PlanTech.Application.Cookie.Interfaces;
using Dfe.PlanTech.Domain.Cookie;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Dfe.PlanTech.Application.Cookie.Service
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _context;

        public CookieService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public void SetVisibility(bool visibility)
        {
            var currentCookie = GetCookie();
            DeleteCookie();
            CreateCookie("cookies_preferences_set", currentCookie.HasApproved, visibility, currentCookie.IsRejected);
        }

        public void RejectCookies()
        {
            DeleteCookie();
            CreateCookie("cookies_preferences_set", false, true, true);
        }

        public void SetPreference(bool userPreference)
        {
            CreateCookie("cookies_preferences_set", userPreference);
        }

        public DfeCookie GetCookie()
        {
            var cookie = _context.HttpContext.Request.Cookies["cookies_preferences_set"];
            if (cookie is null) 
            { 
                return new DfeCookie(); 
            }
            else
            {
                var dfeCookie = JsonSerializer.Deserialize<DfeCookie>(cookie);
                return dfeCookie is null ? new DfeCookie() : dfeCookie;
            }
        }

        private void DeleteCookie()
        {
            _context.HttpContext.Response.Cookies.Delete("cookies_preferences_set");
        }

        private void CreateCookie(string key, bool value, bool visibility = true, bool rejected = false)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Secure = true;
            cookieOptions.HttpOnly = true;
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddYears(1));

            var cookie = new DfeCookie { IsVisible = visibility, HasApproved = value, IsRejected = rejected };
            var serializedCookie = JsonSerializer.Serialize(cookie);
            _context.HttpContext.Response.Cookies.Append(key, serializedCookie, cookieOptions);
        }
    }
}