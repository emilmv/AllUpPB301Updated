using Microsoft.AspNetCore.Mvc;

namespace AllUp.ViewComponents;
public class FooterViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(IDictionary<string, string> settings)
    {
        return View(await Task.FromResult(settings));
    }
}