﻿using System.Web;
using System.Web.Mvc;

namespace MediKeeper_Items
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}