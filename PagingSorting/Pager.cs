﻿using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Routing;
using System.Collections;

namespace MvcPaging
{
	public class Pager
	{
		private ViewContext viewContext;
		private readonly int pageSize;
		private readonly int currentPage;
		private readonly int totalItemCount;
		private readonly RouteValueDictionary linkWithoutPageValuesDictionary;
		private readonly AjaxOptions ajaxOptions;
        private readonly string sortBy;
        private readonly bool? sortDescending;

		public Pager(ViewContext viewContext, int pageSize, int currentPage, int totalItemCount, RouteValueDictionary valuesDictionary, AjaxOptions ajaxOptions)
		{
			this.viewContext = viewContext;
			this.pageSize = pageSize;
			this.currentPage = currentPage;
			this.totalItemCount = totalItemCount;
			this.linkWithoutPageValuesDictionary = valuesDictionary;
			this.ajaxOptions = ajaxOptions;
		}

        public Pager(ViewContext viewContext, int pageSize, int currentPage, int totalItemCount, string sortBy, bool? sortDescending, RouteValueDictionary valuesDictionary, AjaxOptions ajaxOptions)
        {
            this.viewContext = viewContext;
            this.pageSize = pageSize;
            this.currentPage = currentPage;
            this.totalItemCount = totalItemCount;
            this.sortBy = sortBy;
            this.sortDescending = sortDescending;
            this.linkWithoutPageValuesDictionary = valuesDictionary;
            this.ajaxOptions = ajaxOptions;
        }

		public HtmlString RenderHtml()
		{
			var pageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
			const int nrOfPagesToDisplay = 10;

			var sb = new StringBuilder();

			// Previous
			sb.Append(currentPage > 1 ? GeneratePageLink("&lt;", currentPage - 1) : "<span class=\"disabled\">&lt;</span>");

			var start = 1;
			var end = pageCount;

			if (pageCount > nrOfPagesToDisplay)
			{
				var middle = (int)Math.Ceiling(nrOfPagesToDisplay / 2d) - 1;
				var below = (currentPage - middle);
				var above = (currentPage + middle);

				if (below < 4)
				{
					above = nrOfPagesToDisplay;
					below = 1;
				}
				else if (above > (pageCount - 4))
				{
					above = pageCount;
					below = (pageCount - nrOfPagesToDisplay);
				}

				start = below;
				end = above;
			}

			if (start > 3)
			{
				sb.Append(GeneratePageLink("1", 1));
				sb.Append(GeneratePageLink("2", 2));
				sb.Append("...");
			}
			
			for (var i = start; i <= end; i++)
			{
				if (i == currentPage || (currentPage <= 0 && i == 0))
				{
					sb.AppendFormat("<span class=\"current\">{0}</span>", i);
				}
				else
				{
					sb.Append(GeneratePageLink(i.ToString(), i));
				}
			}
			if (end < (pageCount - 3))
			{
				sb.Append("...");
				sb.Append(GeneratePageLink((pageCount - 1).ToString(), pageCount - 1));
				sb.Append(GeneratePageLink(pageCount.ToString(), pageCount));
			}

			// Next
			sb.Append(currentPage < pageCount ? GeneratePageLink("&gt;", (currentPage + 1)) : "<span class=\"disabled\">&gt;</span>");

			return new HtmlString(sb.ToString());
		}

		private string GeneratePageLink(string linkText, int pageNumber)
		{
            var pageLinkValueDictionary = new RouteValueDictionary(linkWithoutPageValuesDictionary) { {"Page",pageNumber},{"SortBy", sortBy},{"SortDescending",sortDescending} };
            
			// To be sure we get the right route, ensure the controller and action are specified.
			var routeDataValues = viewContext.RequestContext.RouteData.Values;
			if (!pageLinkValueDictionary.ContainsKey("controller") && routeDataValues.ContainsKey("controller"))
			{
				pageLinkValueDictionary.Add("controller", routeDataValues["controller"]);
			}
			if (!pageLinkValueDictionary.ContainsKey("action") && routeDataValues.ContainsKey("action"))
			{
				pageLinkValueDictionary.Add("action", routeDataValues["action"]);
			}

			// 'Render' virtual path.
			var virtualPathForArea = RouteTable.Routes.GetVirtualPathForArea(viewContext.RequestContext, pageLinkValueDictionary);

			if (virtualPathForArea == null)
				return null;

			var stringBuilder = new StringBuilder("<a");

			if (ajaxOptions != null)
				foreach (var ajaxOption in ajaxOptions.ToUnobtrusiveHtmlAttributes())
					stringBuilder.AppendFormat(" {0}=\"{1}\"", ajaxOption.Key, ajaxOption.Value);

			stringBuilder.AppendFormat(" href=\"{0}\">{1}</a>", virtualPathForArea.VirtualPath, linkText);

			return stringBuilder.ToString();
		}
	}
}