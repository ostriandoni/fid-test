using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Fujitsu.Data;
using Fujitsu.Models;
using Fujitsu.ViewModels;

public class ProvinceController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProvinceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        // 1. Query the master data table
        //    Use the EXACT property name: ProvinceName
        var provinces = _context.Provinces.OrderBy(p => p.ProvinceName).ToList();

        // 2. Transform the data into SelectListItem objects
        var provinceItems = provinces.Select(p => new SelectListItem
        {
            Value = p.ProvinceId.ToString(),
            Text = p.ProvinceName
        });

        // 3. Create and populate the View Model
        var viewModel = new ProvinceViewModel
        {
            ProvinceList = provinceItems
        };

        return View(viewModel);
    }
}