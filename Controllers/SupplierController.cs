using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fujitsu.Data;
using Fujitsu.Models;
using Fujitsu.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fujitsu.Controllers
{
    // MVC Controller to serve the Index view (Handles the URL /Supplier)
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Supplier/Index
        public IActionResult Index()
        {
            // A. Query the master data for the dropdown
            var provinces = _context.Provinces.OrderBy(p => p.ProvinceName).ToList();
            var provinceItems = provinces.Select(p => new SelectListItem
            {
                Value = p.ProvinceId.ToString(),
                Text = p.ProvinceName
            });

            // B. Query the main data for the page (Suppliers)
            var suppliers = _context.Suppliers.ToList(); // Add filtering here later

            // C. Combine everything into the dedicated View Model
            var viewModel = new SupplierIndexViewModel
            {
                Suppliers = suppliers,
                ProvinceList = provinceItems,
                SelectedProvinceId = null // Default to "All"
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult SearchSuppliers(SupplierIndexViewModel model)
        {
            var query = _context.Suppliers.AsQueryable();

            // --- Filter by Supplier Code ---
            if (!string.IsNullOrEmpty(model.SupplierCodeFilter))
            {
                // Using ToLower() for case-insensitive matching if needed, though Contains() is generally case-insensitive in SQL by default.
                query = query.Where(s => s.SupplierCode != null && s.SupplierCode.ToLower().Contains(model.SupplierCodeFilter.ToLower()));
            }

            // --- Filter by Province (By Name Lookup) ---
            if (model.SelectedProvinceId.HasValue && model.SelectedProvinceId.Value > 0)
            {
                var provinceName = _context.Provinces
                    .Where(p => p.ProvinceId == model.SelectedProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(provinceName))
                {
                    // FIX: Use ToLower() on both sides for case-insensitive comparison
                    // This is translatable to SQL
                    var lowerProvinceName = provinceName.ToLower();
                    query = query.Where(s => s.Province != null && 
                                            s.Province.ToLower() == lowerProvinceName); 
                }
            }
            
            // --- Filter by City (By Name Lookup) ---
            if (model.SelectedCityId.HasValue && model.SelectedCityId.Value > 0)
            {
                var cityName = _context.Cities
                    .Where(c => c.CityId == model.SelectedCityId.Value)
                    .Select(c => c.CityName)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(cityName))
                {
                    // FIX: Use ToLower() on both sides for case-insensitive comparison
                    var lowerCityName = cityName.ToLower();
                    query = query.Where(s => s.City != null && 
                                            s.City.ToLower() == lowerCityName);
                }
            }
            
            // Execute query and prepare results
            model.Suppliers = query.ToList(); 

            return PartialView("_SupplierResultsTable", model);
        }
    }

    // API Controller for AJAX/Fetch requests from the client (Handles the URL /api/supplier)
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupplierApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("cities")]
        public IActionResult GetCitiesByProvinceId(int provinceId)
        {
            // Query the City data based on the selected ProvinceId
            var cities = _context.Cities
                .Where(c => c.ProvinceId == provinceId)
                .OrderBy(c => c.CityName)
                .Select(c => new 
                {
                    value = c.CityId,
                    text = c.CityName
                })
                .ToList();

            return Ok(cities);
        }

        // GET: api/Supplier?code=X&province=Y&city=Z
        // This endpoint handles the search and initial data load
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers(
            [FromQuery] string code, 
            [FromQuery] string province, 
            [FromQuery] string city)
        {
            IQueryable<Supplier> suppliers = _context.Suppliers;

            // Apply filters based on query parameters
            if (!string.IsNullOrEmpty(code))
            {
                // Searches code and name
                suppliers = suppliers.Where(s => s.SupplierCode.Contains(code) || s.SupplierName.Contains(code));
            }

            if (!string.IsNullOrEmpty(province))
            {
                suppliers = suppliers.Where(s => s.Province == province);
            }

            if (!string.IsNullOrEmpty(city))
            {
                suppliers = suppliers.Where(s => s.City == city);
            }

            // Execute the query and return the results as JSON
            return await suppliers.ToListAsync();
        }

        // POST: api/Supplier
        // This endpoint handles creating a new supplier record
        [HttpPost]
        public async Task<ActionResult<Supplier>> PostSupplier(Supplier supplier)
        {
            // Basic server-side validation for required fields
            if (string.IsNullOrEmpty(supplier.SupplierCode) || string.IsNullOrEmpty(supplier.SupplierName))
            {
                return BadRequest("Supplier Code and Supplier Name are required.");
            }

            // Ensure we don't accidentally try to update an existing ID
            supplier.SupplierId = 0; 

            // Add the new supplier to the DbContext
            _context.Suppliers.Add(supplier);
            
            // Save changes to the SQLite database
            await _context.SaveChangesAsync();

            // Return the newly created supplier, including the generated SupplierId
            // Uses CreatedAtAction (201 Created) for REST convention
            return CreatedAtAction(nameof(GetSuppliers), new { id = supplier.SupplierId }, supplier);
        }
    }
}