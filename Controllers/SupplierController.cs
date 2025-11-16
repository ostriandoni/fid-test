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
                    query = query.Where(s => s.Province != null && s.Province.ToLower() == lowerProvinceName);
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
                    query = query.Where(s => s.City != null && s.City.ToLower() == lowerCityName);
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

        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddSupplier([FromBody] AddSupplierModel model)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(model.SupplierCode) || string.IsNullOrWhiteSpace(model.SupplierName))
            {
                return BadRequest("Supplier Code and Supplier Name are required.");
            }

            // 2. Lookup Province and City Names using the IDs
            string provinceName = null;
            string cityName = null;

            if (model.ProvinceId.HasValue)
            {
                // Asynchronously query the Province table for the name
                provinceName = await _context.Provinces
                    .Where(p => p.ProvinceId == model.ProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefaultAsync();
            }

            if (model.CityId.HasValue)
            {
                // Asynchronously query the City table for the name
                cityName = await _context.Cities
                    .Where(c => c.CityId == model.CityId.Value)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync();
            }

            // Optional: Check if the IDs provided actually exist in the database
            // if (model.ProvinceId.HasValue && provinceName == null) return NotFound("Invalid Province ID.");
            // if (model.CityId.HasValue && cityName == null) return NotFound("Invalid City ID.");


            // 3. Map to the Supplier Entity
            var newSupplier = new Supplier
            {
                SupplierCode = model.SupplierCode.Trim(),
                SupplierName = model.SupplierName.Trim(),

                // Assign the looked-up names (will be null if nothing was selected/found)
                Province = provinceName,
                City = cityName,

                Address = model.Address?.Trim(), // Use null conditional operator for optional fields
                ContactPerson = model.ContactPerson?.Trim(),
            };

            // 4. Save to Database
            try
            {
                _context.Suppliers.Add(newSupplier);
                await _context.SaveChangesAsync();

                // 5. Return success status
                return Ok(new { message = "Supplier successfully added.", supplierId = newSupplier.SupplierId });
            }
            catch (DbUpdateException ex)
            {
                // Handle database errors (e.g., unique constraint violation on SupplierCode)
                // Log the exception details here
                return StatusCode(500, $"Database error occurred: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteSuppliers([FromBody] List<int> supplierIds)
        {
            if (supplierIds == null || !supplierIds.Any())
            {
                return BadRequest(new { message = "No supplier IDs provided for deletion." });
            }

            // 1. Retrieve the entities to be deleted
            // Use Where(s => supplierIds.Contains(s.SupplierId)) to select all suppliers whose IDs are in the provided list.
            var suppliersToDelete = await _context.Suppliers
                .Where(s => supplierIds.Contains(s.SupplierId))
                .ToListAsync();

            if (!suppliersToDelete.Any())
            {
                return NotFound(new { message = "No matching suppliers found for the provided IDs." });
            }

            // 2. Remove the entities from the context
            _context.Suppliers.RemoveRange(suppliersToDelete);

            // 3. Save changes to the database
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = $"{suppliersToDelete.Count} supplier(s) successfully deleted." });
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error during bulk delete: {ex}");
                return StatusCode(500, new { message = "A database error occurred during deletion." });
            }
        }
    }
}