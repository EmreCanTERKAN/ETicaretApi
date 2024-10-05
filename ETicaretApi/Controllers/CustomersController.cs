using ETicaretApi.Data;
using ETicaretApi.Dto;
using ETicaretApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ETicaretContext _context;

        public CustomersController(ETicaretContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (customer is null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Create (Customer customer)
        {
            _context.Customers.Add(customer);
           await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] CustomerFilterDto filter)
        {
            var query = _context.Customers.AsQueryable();
            if (filter.StartDate.HasValue)
            {
                query = query.Where(c => c.SignUpDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(c => c.SignUpDate <= filter.EndDate.Value);
            }
            if (!string.IsNullOrEmpty(filter.NameSearch))
            {
                query = query.Where(c => c.FirstName.Contains(filter.NameSearch) || c.LastName.Contains(filter.NameSearch));
            }

            if (!string.IsNullOrEmpty(filter.EmailSearch))
            {
                query = query.Where(c => c.Email.Contains(filter.EmailSearch));
            }

            query = query.OrderBy(c => c.LastName)
                                .ThenBy(c => c.FirstName);

            var totalCount = await query.CountAsync();
            var totalPages = Math.Ceiling(totalCount / (double)filter.Page);

            var customers = await query
                                 .Skip((filter.Page - 1) * filter.PageSize)
                                 .Take(filter.PageSize)
                                 .Select(c => new CustomerDto
                                 {
                                     Id = c.Id,
                                     Email = c.Email,
                                     FullName = c.FirstName,
                                     SignUpDate = c.SignUpDate


                                 }).ToListAsync();

            var response = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                Customers = customers
            };

            return Ok(response);

        }
    }
}
