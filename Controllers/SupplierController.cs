using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fujitsu.Data;
using Fujitsu.Models;
using Fujitsu.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;

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
            var suppliers = _context.Suppliers.ToList();

            // C. Combine everything into the dedicated View Model
            var viewModel = new SupplierIndexViewModel
            {
                Suppliers = suppliers,
                ProvinceList = provinceItems,
                SelectedProvinceId = null
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult SearchSuppliers(SupplierIndexViewModel model)
        {
            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(model.SupplierCodeFilter))
            {
                // Using ToLower() for case-insensitive matching if needed, though Contains() is generally case-insensitive in SQL by default.
                query = query.Where(s => s.SupplierCode != null && s.SupplierCode.ToLower().Contains(model.SupplierCodeFilter.ToLower()));
            }

            if (model.SelectedProvinceId.HasValue && model.SelectedProvinceId.Value > 0)
            {
                var provinceName = _context.Provinces
                    .Where(p => p.ProvinceId == model.SelectedProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(provinceName))
                {
                    var lowerProvinceName = provinceName.ToLower();
                    query = query.Where(s => s.Province != null && s.Province.ToLower() == lowerProvinceName);
                }
            }

            if (model.SelectedCityId.HasValue && model.SelectedCityId.Value > 0)
            {
                var cityName = _context.Cities
                    .Where(c => c.CityId == model.SelectedCityId.Value)
                    .Select(c => c.CityName)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(cityName))
                {
                    var lowerCityName = cityName.ToLower();
                    query = query.Where(s => s.City != null && s.City.ToLower() == lowerCityName);
                }
            }

            model.Suppliers = query.ToList();

            return PartialView("_SupplierResultsTable", model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadSuppliers(SupplierIndexViewModel viewModel)
        {
            var supplierCodeFilter = viewModel.SupplierCodeFilter;
            var selectedProvinceId = viewModel.SelectedProvinceId;
            var selectedCityId = viewModel.SelectedCityId;

            var query = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(supplierCodeFilter))
            {
                query = query.Where(s => s.SupplierCode.Contains(supplierCodeFilter) ||
                                         s.SupplierName.Contains(supplierCodeFilter));
            }

            if (selectedProvinceId.HasValue && selectedProvinceId.Value > 0)
            {
                var provinceName = await _context.Provinces
                    .Where(p => p.ProvinceId == selectedProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(provinceName))
                {
                    query = query.Where(s => s.Province == provinceName);
                }
            }

            if (selectedCityId.HasValue && selectedCityId.Value > 0)
            {
                var cityName = await _context.Cities
                    .Where(c => c.CityId == selectedCityId.Value)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(cityName))
                {
                    query = query.Where(s => s.City == cityName);
                }
            }

            var suppliers = await query.ToListAsync();


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Supplier Data");

                // Define Column Headers
                worksheet.Cell(1, 1).Value = "Supplier Code";
                worksheet.Cell(1, 2).Value = "Supplier Name";
                worksheet.Cell(1, 3).Value = "Address";
                worksheet.Cell(1, 4).Value = "Province";
                worksheet.Cell(1, 5).Value = "City";
                worksheet.Cell(1, 6).Value = "PIC";

                // Apply basic header formatting (optional)
                worksheet.Row(1).Style.Font.Bold = true;

                // Populate Data Rows
                int currentRow = 2;
                foreach (var supplier in suppliers)
                {
                    worksheet.Cell(currentRow, 1).Value = supplier.SupplierCode;
                    worksheet.Cell(currentRow, 2).Value = supplier.SupplierName;
                    worksheet.Cell(currentRow, 3).Value = supplier.Address;
                    worksheet.Cell(currentRow, 4).Value = supplier.Province;
                    worksheet.Cell(currentRow, 5).Value = supplier.City;
                    worksheet.Cell(currentRow, 6).Value = supplier.ContactPerson;
                    currentRow++;
                }

                // Auto-fit columns for readability (optional)
                worksheet.Columns().AdjustToContents();

                // Save the workbook to a memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Supplier_Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"
                    );
                }
            }
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
                provinceName = await _context.Provinces
                    .Where(p => p.ProvinceId == model.ProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefaultAsync();
            }

            if (model.CityId.HasValue)
            {
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
                Address = model.Address?.Trim(),
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

        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _context.Suppliers
                .Where(s => s.SupplierId == id)
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierCode,
                    s.SupplierName,
                    s.Address,
                    s.ContactPerson,
                    ProvinceId = _context.Provinces.Where(p => p.ProvinceName == s.Province).Select(p => p.ProvinceId).FirstOrDefault(),
                    CityId = _context.Cities.Where(c => c.CityName == s.City).Select(c => c.CityId).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                return NotFound(new { message = "Supplier not found." });
            }

            return Ok(supplier);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateSupplier([FromBody] UpdateSupplierModel model)
        {
            // 1. Validation (ensure required fields are present)
            if (model.SupplierId <= 0 || string.IsNullOrWhiteSpace(model.SupplierName) || string.IsNullOrWhiteSpace(model.ContactPerson))
            {
                return BadRequest(new { message = "Supplier ID, Name, and Contact Person are required for update." });
            }

            // 2. Find the existing supplier entity
            var supplierToUpdate = await _context.Suppliers.FindAsync(model.SupplierId);

            if (supplierToUpdate == null)
            {
                return NotFound(new { message = $"Supplier with ID {model.SupplierId} not found." });
            }

            // 3. Lookup Province and City Names (reuse logic from AddSupplier)
            string provinceName = null;
            string cityName = null;

            if (model.ProvinceId.HasValue)
            {
                provinceName = await _context.Provinces
                    .Where(p => p.ProvinceId == model.ProvinceId.Value)
                    .Select(p => p.ProvinceName)
                    .FirstOrDefaultAsync();
            }

            if (model.CityId.HasValue)
            {
                cityName = await _context.Cities
                    .Where(c => c.CityId == model.CityId.Value)
                    .Select(c => c.CityName)
                    .FirstOrDefaultAsync();
            }

            // 4. Apply Updates
            supplierToUpdate.SupplierName = model.SupplierName.Trim();
            supplierToUpdate.Address = model.Address?.Trim();
            supplierToUpdate.ContactPerson = model.ContactPerson.Trim();
            supplierToUpdate.Province = provinceName;
            supplierToUpdate.City = cityName;

            // 5. Save Changes
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Supplier successfully updated." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new { message = "Concurrency conflict. The supplier may have been deleted or modified by another user." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"A database error occurred: {ex.Message}" });
            }
        }
    }
}