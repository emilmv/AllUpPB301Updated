using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AllUp.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(HeaderVM headerVM)
    {
        return View(await Task.FromResult(headerVM));
    }
}
