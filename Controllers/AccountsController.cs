using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApplication.Context;
using OnlineBankingApplication.Entities;
using OnlineBankingApplication.Services;
using System.Text.Json;

namespace OnlineBankingApplication.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly BaseDbContext _context;
        private readonly MessageQueueService _mqService;
        private readonly ResilientHttpClient _resilientHttpClient;
        private static readonly object _lock = new object();

        public AccountsController(BaseDbContext context, MessageQueueService mqService, ResilientHttpClient resilientHttpClient)
        {
            _context = context;
            _mqService = mqService;
            _resilientHttpClient = resilientHttpClient;
        }
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

                _mqService.Publish($"Account {id} deposited {amount}.");

                var content = new StringContent(JsonSerializer.Serialize(new { id, amount }), System.Text.Encoding.UTF8, "application/json");
                var response = _resilientHttpClient.PostAsync("http://example.com/notify", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "External service call failed.");
                }
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

                _mqService.Publish($"Account {id} withdrew {amount}.");

                var content = new StringContent(JsonSerializer.Serialize(new { id, amount }), System.Text.Encoding.UTF8, "application/json");
                var response = _resilientHttpClient.PostAsync("http://example.com/notify", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "External service call failed.");
                }
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
