using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

public static class ScriptHelper
{
    //TODO: check if script exists
    public static IHtmlContent IncludeViewScript(this IHtmlHelper htmlHelper)
    {
        var area = htmlHelper.ViewContext.RouteData.Values["area"]?.ToString().ToLower();
        var controller = htmlHelper.ViewContext.RouteData.Values["controller"]?.ToString().ToLower();
        var action = htmlHelper.ViewContext.RouteData.Values["action"]?.ToString().ToLower();
        //TODD: check if area not null
        var scriptPath = $"/js/{area}/{controller}/{action}.js";

        return new HtmlString($"<script src=\"{scriptPath}\"></script>");
    }
}
