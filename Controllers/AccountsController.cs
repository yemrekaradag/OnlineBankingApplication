using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApplication.Context;
using OnlineBankingApplication.Entities;

namespace OnlineBankingApplication.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly BaseDbContext _context;
        private static readonly object _lock = new object();

        public AccountsController(BaseDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccountBalance), new { id = account.Id }, account);
        }

        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(int id, [FromBody] decimal amount)
        {
            lock (_lock)
            {
                var account = _context.Accounts.Find(id);
                if (account == null) return NotFound();

                account.Balance += amount;
                _context.SaveChanges();
            }

            return NoContent();
        }

        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(int id, [FromBody] decimal amount)
        {
            lock (_lock)
            {
                var account = _context.Accounts.Find(id);
                if (account == null) return NotFound();

                if (account.Balance < amount) return BadRequest("Insufficient funds.");

                account.Balance -= amount;
                _context.SaveChanges();
            }

            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<IActionResult> GetAccountBalance(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            return Ok(account.Balance);
        }
    }
}
